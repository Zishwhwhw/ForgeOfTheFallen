using UnityEngine;
using UnityEngine.SceneManagement;

namespace FrankenArms
{

/// <summary>
/// Builds the first playable test room at runtime so a fresh scene needs no
/// manual Inspector wiring.
/// </summary>
public sealed class FrankenArmsBootstrap : MonoBehaviour
{
    private static readonly Color FloorColor = new(0.08f, 0.10f, 0.15f, 1f);
    private static readonly Color PlayerColor = new(0.25f, 0.85f, 0.95f, 1f);

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void RegisterSceneFlow()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void CreateRuntimeSlice()
    {
        EnsureRuntimeSlice(SceneManager.GetActiveScene());
    }

    private static void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        EnsureRuntimeSlice(scene);
    }

    private static void EnsureRuntimeSlice(Scene scene)
    {
        if (scene.name != SceneFlow.Run)
            return;
        if (FindAnyObjectByType<FrankenArmsBootstrap>() != null)
            return;

        var bootstrap = new GameObject("FrankenArms Runtime Slice");
        bootstrap.AddComponent<FrankenArmsBootstrap>();
    }

        private void Awake()
        {
            RunStats.BeginRun();
            BuildRoom();
        BuildPlayer();
        BuildCombatRoom();
        BuildFusionStation();
        ConfigureCamera();
    }

    private static void BuildRoom()
    {
        var floor = GameObject.CreatePrimitive(PrimitiveType.Quad);
        floor.name = "Test Room Floor";
        floor.transform.position = Vector3.zero;
        floor.transform.localScale = new Vector3(16f, 10f, 1f);
        floor.transform.rotation = Quaternion.Euler(0f, 0f, 0f);
        var renderer = floor.GetComponent<MeshRenderer>();
        renderer.material = CreateMaterial(FloorColor);
        var authoredFloor = Resources.Load<Texture2D>("Art/Generated/dungeon-floor-v1");
        var floorTexture = authoredFloor != null ? authoredFloor : RuntimeSpriteFactory.CreateFloorTile().texture;
        floorTexture.filterMode = FilterMode.Point;
        renderer.material.color = authoredFloor != null ? Color.white : FloorColor;
        floorTexture.wrapMode = TextureWrapMode.Repeat;
        renderer.material.mainTexture = floorTexture;
        renderer.material.mainTextureScale = new Vector2(16f, 10f);
        Object.Destroy(floor.GetComponent<MeshCollider>());
        BuildRoomBounds();
    }

    private static void BuildRoomBounds()
    {
        CreateWall("North Wall", new Vector3(0f, 5f, 0f), new Vector3(16f, 1f, 1f));
        CreateWall("South Wall", new Vector3(0f, -5f, 0f), new Vector3(16f, 1f, 1f));
        CreateWall("West Wall", new Vector3(-8f, 0f, 0f), new Vector3(1f, 10f, 1f));
        CreateWall("East Wall", new Vector3(8f, 0f, 0f), new Vector3(1f, 10f, 1f));
    }

    private static void CreateWall(string name, Vector3 position, Vector3 scale)
    {
        var wall = new GameObject(name);
        wall.transform.position = position;
        wall.transform.localScale = scale;
        var renderer = wall.AddComponent<SpriteRenderer>();
        renderer.sprite = RuntimeSpriteFactory.CreateWall();
        renderer.sortingOrder = 6;
        var collider = wall.AddComponent<BoxCollider2D>();
        collider.size = Vector2.one;
    }

    private static void BuildPlayer()
    {
        var playerObject = new GameObject("Player");
        playerObject.transform.position = Vector3.zero;
        var renderer = playerObject.AddComponent<SpriteRenderer>();
        renderer.sprite = RuntimeSpriteFactory.CreatePlayer();
        renderer.color = Color.white;
        renderer.sortingOrder = 10;

        var body = playerObject.AddComponent<Rigidbody2D>();
        body.gravityScale = 0f;
        body.freezeRotation = true;
        var collider = playerObject.AddComponent<CircleCollider2D>();
        collider.radius = 0.38f;
        playerObject.AddComponent<PlayerMovement>();
        playerObject.AddComponent<PlayerDash>();
        playerObject.AddComponent<PlayerMana>();
        playerObject.AddComponent<PlayerArmor>();
        playerObject.AddComponent<StatusEffectController>();
        playerObject.AddComponent<PlayerCombat>();
        playerObject.AddComponent<PlayerHealth>();
        playerObject.AddComponent<RunHUD>();
        playerObject.AddComponent<RunMinimapUI>();
        playerObject.AddComponent<PauseController>();
        playerObject.AddComponent<PixelAnimator>().Initialize(RuntimeSpriteFactory.CreatePlayerFrames());
        playerObject.AddComponent<HeroSelectionController>();
        playerObject.AddComponent<HeroActiveAbilityController>();
        BuildPet(playerObject.transform);
    }

    private static void BuildPet(Transform player)
    {
        var petObject = new GameObject("Pet Dog");
        petObject.transform.position = player.position + new Vector3(-0.75f, -0.75f, 0f);
        petObject.transform.localScale = new Vector3(0.62f, 0.62f, 1f);
        var renderer = petObject.AddComponent<SpriteRenderer>();
        renderer.sprite = RuntimeSpriteFactory.CreatePlayer();
        renderer.color = new Color(0.82f, 0.62f, 0.35f, 1f);
        renderer.sortingOrder = 9;
        petObject.AddComponent<PetDog>();
    }

    private static void BuildCombatRoom()
    {
        var roomObject = new GameObject("Combat Room");
        roomObject.AddComponent<CombatRoom>();
    }

    private static void BuildFusionStation()
    {
        var stationObject = new GameObject("Fusion Station");
        stationObject.transform.position = new Vector3(-3f, 2.2f, 0f);
        var renderer = stationObject.AddComponent<SpriteRenderer>();
            renderer.sprite = RuntimeSpriteFactory.CreateDoor();
        renderer.color = new Color(0.7f, 0.25f, 0.95f, 1f);
        renderer.sortingOrder = 8;
        var player = GameObject.Find("Player");
        var playerRenderer = player.GetComponent<SpriteRenderer>();
        var station = stationObject.AddComponent<FusionStation>();
        station.Initialize(player.GetComponent<PlayerCombat>(), playerRenderer, player.transform);
        var importedBridge = stationObject.AddComponent<ImportedModuleForgeBridge>();
        importedBridge.Initialize(player.GetComponent<PlayerCombat>(), playerRenderer, player.transform);
        var shop = stationObject.AddComponent<RunShopStation>();
        shop.Initialize(player.GetComponent<PlayerHealth>(), player.GetComponent<PlayerMana>(), player.transform);
        var shrine = stationObject.AddComponent<RunShrineStation>();
        shrine.Initialize(player.GetComponent<PlayerHealth>(), player.GetComponent<PlayerMana>(), player.transform);
    }

    private static void ConfigureCamera()
    {
        var camera = Camera.main;
        if (camera == null)
        {
            var cameraObject = new GameObject("Main Camera");
            camera = cameraObject.AddComponent<Camera>();
            cameraObject.AddComponent<AudioListener>();
            camera.tag = "MainCamera";
        }

        camera.orthographic = true;
        camera.orthographicSize = 6f;
        camera.transform.position = new Vector3(0f, 0f, -10f);
        camera.backgroundColor = new Color(0.015f, 0.02f, 0.04f, 1f);
        camera.clearFlags = CameraClearFlags.SolidColor;
        camera.gameObject.AddComponent<ScreenShake>();
    }

    private static Material CreateMaterial(Color color)
    {
        var shader = Shader.Find("Universal Render Pipeline/2D/Sprite-Unlit-Default") ?? Shader.Find("Sprites/Default");
        var material = new Material(shader) { color = color };
        return material;
    }

}
}
