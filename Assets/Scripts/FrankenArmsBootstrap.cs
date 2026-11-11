using UnityEngine;

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

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void CreateRuntimeSlice()
    {
        if (FindAnyObjectByType<FrankenArmsBootstrap>() != null)
            return;

        var bootstrap = new GameObject("FrankenArms Runtime Slice");
        bootstrap.AddComponent<FrankenArmsBootstrap>();
        DontDestroyOnLoad(bootstrap);
    }

    private void Awake()
    {
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
        Object.Destroy(floor.GetComponent<MeshCollider>());
    }

    private static void BuildPlayer()
    {
        var playerObject = new GameObject("Player");
        playerObject.transform.position = Vector3.zero;
        var renderer = playerObject.AddComponent<SpriteRenderer>();
        renderer.sprite = RuntimeSpriteFactory.CreatePlayer();
        renderer.color = PlayerColor;
        renderer.sortingOrder = 10;

        var body = playerObject.AddComponent<Rigidbody2D>();
        body.gravityScale = 0f;
        body.freezeRotation = true;
        var collider = playerObject.AddComponent<CircleCollider2D>();
        collider.radius = 0.38f;
        playerObject.AddComponent<PlayerMovement>();
        playerObject.AddComponent<PlayerCombat>();
        playerObject.AddComponent<WeaponFusionDemo>();
        playerObject.AddComponent<PlayerHealth>();
        playerObject.AddComponent<PixelAnimator>().Initialize(RuntimeSpriteFactory.CreatePlayerFrames());
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
        station.Initialize(player.GetComponent<PlayerCombat>(), playerRenderer);
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
    }

    private static Material CreateMaterial(Color color)
    {
        var shader = Shader.Find("Universal Render Pipeline/2D/Sprite-Unlit-Default") ?? Shader.Find("Sprites/Default");
        var material = new Material(shader) { color = color };
        return material;
    }

}
}
