using System.Collections.Generic;
using System.IO;
using System.Linq;
using FotF.Data;
using FotF.Logic.Generation;
using FotF.View.Enemies;
using FotF.View.Heroes;
using FotF.View.UI;
using FotF.View.World;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering.Universal;
using UnityEngine.Tilemaps;

namespace FotF.EditorTools.World
{
    /// <summary>
    /// Серые заготовки этажа: шаблоны комнат (префабы с тайлмапом формы), портал, тайлсет, сцена Run.
    /// Существующие префабы не перезаписываются — после создания их правят в редакторе (ARCHITECTURE §3).
    /// </summary>
    public static class FloorAuthoring
    {
        const string RoomsDir = "Assets/_Project/Prefabs/Rooms";
        const string Interact = "Assets/_Project/Prefabs/Interactables";
        const string TilesDir = "Assets/_Project/Art/Tiles/Castle/Tiles";
        const string TilesetPath = "Assets/_Project/Art/Tiles/Castle/tileset_castle_r3.asset";
        const string PortalPath = Interact + "/prop_portal.prefab";
        const string TorchPath = "Assets/_Project/Prefabs/Interactables/fx_torch_light.prefab";
        const string RunScene = "Assets/_Project/Scenes/Run.unity";
        const string GreyBoxScene = "Assets/_Project/Scenes/GreyBox.unity";
        const string Props = "Assets/_Project/Art/Sprites/Props/";

        sealed class Spec
        {
            public string name; public int w, h; public RoomKind[] kinds; public int cut;   // cut — срез углов (крест)
            public bool fight; public string label; public string decor; public bool start; public string prop;
        }

        static readonly Spec[] Specs =
        {
            new Spec { name = "room_start", w = 12, h = 10, kinds = new[] { RoomKind.Start }, start = true },
            new Spec { name = "room_combat_hall", w = 18, h = 10, kinds = new[] { RoomKind.Combat, RoomKind.Elite }, fight = true },
            new Spec { name = "room_combat_square", w = 16, h = 14, kinds = new[] { RoomKind.Combat, RoomKind.Elite }, fight = true },
            new Spec { name = "room_combat_long", w = 24, h = 8, kinds = new[] { RoomKind.Combat }, fight = true },
            new Spec { name = "room_combat_cross", w = 20, h = 14, cut = 4, kinds = new[] { RoomKind.Combat }, fight = true },
            new Spec { name = "room_forge", w = 14, h = 10, kinds = new[] { RoomKind.Forge }, prop = Interact + "/prop_forge.prefab" },
            new Spec { name = "room_chest", w = 10, h = 8, kinds = new[] { RoomKind.Chest }, prop = Interact + "/prop_chest.prefab" },
            new Spec { name = "room_merchant", w = 12, h = 10, kinds = new[] { RoomKind.Merchant }, label = "Торговец\n(следующая поставка)" },
            new Spec { name = "room_fountain", w = 10, h = 10, kinds = new[] { RoomKind.Fountain }, decor = "prop_fountain", label = "Фонтан\n(следующая поставка)" },
            new Spec { name = "room_shrine", w = 10, h = 10, kinds = new[] { RoomKind.Shrine }, decor = "prop_shrine", label = "Святилище\n(следующая поставка)" },
            new Spec { name = "room_cage", w = 10, h = 10, kinds = new[] { RoomKind.Cage }, decor = "prop_helper_cage", label = "Клетка помощника\n(следующая поставка)" },
            new Spec { name = "room_miniboss", w = 20, h = 14, cut = 3, kinds = new[] { RoomKind.MiniBoss }, label = "Арена Капитана стражи\n(мини-босс — следующая поставка)" },
            new Spec { name = "room_secret", w = 10, h = 8, kinds = new[] { RoomKind.Secret }, label = "Секрет" },
        };

        [MenuItem("FotF/Мир/Собрать серые заготовки этажа")]
        public static void BuildAll()
        {
            var tileset = EnsureTileset();
            EnsurePortal();
            if (!AssetDatabase.IsValidFolder(RoomsDir)) AssetDatabase.CreateFolder("Assets/_Project/Prefabs", "Rooms");
            var templates = Specs.Select(s => EnsureRoom(s, tileset)).ToArray();
            EnsureRunScene(tileset, templates);
            AssetDatabase.SaveAssets();
            Debug.Log($"[Этаж] заготовки готовы: {templates.Length} шаблонов, сцена {RunScene}");
        }

        static TerrainTileset EnsureTileset()
        {
            var set = AssetDatabase.LoadAssetAtPath<TerrainTileset>(TilesetPath);
            if (set == null) { set = ScriptableObject.CreateInstance<TerrainTileset>(); AssetDatabase.CreateAsset(set, TilesetPath); }
            var tiles = AssetDatabase.FindAssets("t:TileBase", new[] { TilesDir }).Select(g => AssetDatabase.LoadAssetAtPath<TileBase>(AssetDatabase.GUIDToAssetPath(g))).ToList();
            set.collisionTile = tiles.FirstOrDefault(t => t.name == "tile_collision");
            set.tiles = tiles.Where(t => t.name.StartsWith("tile_castle_r3_")).OrderBy(t => t.name).ToArray();
            EditorUtility.SetDirty(set);
            return set;
        }

        static void EnsurePortal()
        {
            if (File.Exists(PortalPath)) return;
            var go = new GameObject("prop_portal");
            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = AssetDatabase.LoadAssetAtPath<Sprite>(Props + "prop_floor_portal.png");
            var col = go.AddComponent<CircleCollider2D>(); col.isTrigger = true; col.radius = 1f;
            go.AddComponent<PortalView>();
            PrefabUtility.SaveAsPrefabAsset(go, PortalPath);
            Object.DestroyImmediate(go);
        }

        static RoomTemplate EnsureRoom(Spec s, TerrainTileset tileset)
        {
            string path = $"{RoomsDir}/{s.name}.prefab";
            var existing = AssetDatabase.LoadAssetAtPath<RoomTemplate>(path);
            if (existing) return existing;

            var root = new GameObject(s.name);
            root.AddComponent<Grid>();
            var t = root.AddComponent<RoomTemplate>();
            t.kinds = s.kinds;

            var shapeGo = new GameObject("Shape");
            shapeGo.transform.SetParent(root.transform, false);
            var shape = shapeGo.AddComponent<Tilemap>();
            var rend = shapeGo.AddComponent<TilemapRenderer>(); rend.sortingOrder = -20;
            var floorTile = tileset.Get("llll");
            var cells = new HashSet<Vector2Int>();
            for (int y = 0; y < s.h; y++)
                for (int x = 0; x < s.w; x++)
                {
                    bool corner = s.cut > 0 && (x < s.cut || x >= s.w - s.cut) && (y < s.cut - 1 || y >= s.h - s.cut + 1);
                    if (corner) continue;
                    shape.SetTile(new Vector3Int(x, y, 0), floorTile);
                    cells.Add(new Vector2Int(x, y));
                }
            t.shape = shape;
            Vector3 center = new Vector3(s.w / 2 + 0.5f, s.h / 2 + 0.5f, 0f);

            var torch = AssetDatabase.LoadAssetAtPath<GameObject>(TorchPath);
            if (torch) { var l = (GameObject)PrefabUtility.InstantiatePrefab(torch, root.transform); l.transform.localPosition = new Vector3(s.w / 2f, s.h + 0.3f, 0f); }

            if (s.start)
            {
                t.heroSpawn = Child(root.transform, "HeroSpawn", center + Vector3.down * 2f);
                var pickup = AssetDatabase.LoadAssetAtPath<GameObject>(Interact + "/pickup_weapon.prefab");
                if (pickup) { var p = (GameObject)PrefabUtility.InstantiatePrefab(pickup, root.transform); p.transform.localPosition = center + new Vector3(-3f, 0f, 0f); }
            }
            if (s.prop != null)
            {
                var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(s.prop);
                var p = (GameObject)PrefabUtility.InstantiatePrefab(prefab, root.transform);
                p.transform.localPosition = center + Vector3.up;
            }
            if (s.decor != null)
            {
                var d = new GameObject(s.decor); d.transform.SetParent(root.transform, false);
                d.transform.localPosition = center + Vector3.up;
                var sr = d.AddComponent<SpriteRenderer>(); sr.sprite = AssetDatabase.LoadAssetAtPath<Sprite>(Props + s.decor + ".png");
                var bc = d.AddComponent<BoxCollider2D>(); bc.size = new Vector2(1.6f, 0.8f); bc.offset = new Vector2(0f, 0.4f);
            }
            if (s.label != null) Label(root.transform, s.label, center + Vector3.down * 1.5f);
            if (s.fight) Fight(root.transform, t, s, cells, center);
            t.portalPoint = Child(root.transform, "PortalPoint", center + Vector3.up * 2f);

            var saved = PrefabUtility.SaveAsPrefabAsset(root, path).GetComponent<RoomTemplate>();
            Object.DestroyImmediate(root);
            return saved;
        }

        static void Fight(Transform root, RoomTemplate t, Spec s, HashSet<Vector2Int> cells, Vector3 center)
        {
            var go = new GameObject("Fight"); go.transform.SetParent(root, false);
            go.transform.localPosition = center;
            var box = go.AddComponent<BoxCollider2D>(); box.isTrigger = true; box.size = new Vector2(s.w - 4f, s.h - 4f);
            var rc = go.AddComponent<RoomController>();
            var so = new SerializedObject(rc);
            so.FindProperty("encounter").objectReferenceValue = AssetDatabase.LoadAssetAtPath<EncounterDefinition>("Assets/_Project/Data/Rooms/encounter_greybox.asset");
            var enemies = so.FindProperty("enemyPrefabs"); enemies.arraySize = 1;
            enemies.GetArrayElementAtIndex(0).objectReferenceValue = AssetDatabase.LoadAssetAtPath<EnemyController>("Assets/_Project/Prefabs/Enemies/enemy_skeleton_swordsman.prefab");
            var fracs = new[] { (0.2f, 0.25f), (0.8f, 0.25f), (0.2f, 0.75f), (0.8f, 0.75f), (0.5f, 0.85f), (0.5f, 0.15f) };
            var points = so.FindProperty("spawnPoints"); points.arraySize = fracs.Length;
            for (int i = 0; i < fracs.Length; i++)
            {
                var want = new Vector2(fracs[i].Item1 * s.w, fracs[i].Item2 * s.h);
                var cell = cells.OrderBy(c => (new Vector2(c.x + 0.5f, c.y + 0.5f) - want).sqrMagnitude).First();
                points.GetArrayElementAtIndex(i).objectReferenceValue = Child(go.transform, "Spawn", new Vector3(cell.x + 0.5f, cell.y + 0.5f) - center, true);
            }
            so.FindProperty("chestPrefab").objectReferenceValue = AssetDatabase.LoadAssetAtPath<ChestView>(Interact + "/prop_chest.prefab");
            so.FindProperty("chestPoint").objectReferenceValue = Child(go.transform, "ChestPoint", Vector3.zero, true);
            so.ApplyModifiedPropertiesWithoutUndo();
            t.fight = rc;
        }

        static Transform Child(Transform parent, string name, Vector3 local, bool _ = false)
        {
            var go = new GameObject(name); go.transform.SetParent(parent, false); go.transform.localPosition = local;
            return go.transform;
        }

        static void Label(Transform parent, string text, Vector3 local)
        {
            var go = new GameObject("Label"); go.transform.SetParent(parent, false); go.transform.localPosition = local;
            var tmp = go.AddComponent<TextMeshPro>();
            tmp.font = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>("Assets/_Project/Art/UI/Fonts/font_ui_dynamic.asset");
            tmp.text = text; tmp.fontSize = 3f; tmp.alignment = TextAlignmentOptions.Center;
            tmp.rectTransform.sizeDelta = new Vector2(10f, 3f);
            go.GetComponent<MeshRenderer>().sortingOrder = 50;
        }

        static void EnsureRunScene(TerrainTileset tileset, RoomTemplate[] templates)
        {
            if (!File.Exists(RunScene)) AssetDatabase.CopyAsset(GreyBoxScene, RunScene);
            var scene = EditorSceneManager.OpenScene(RunScene, OpenSceneMode.Single);
            foreach (var name in new[] { "Room_GreyBox", "prop_forge", "pickup_weapon", "prop_training_dummy", "Light_Torch" })
                foreach (var go in scene.GetRootGameObjects().Where(g => g.name == name).ToArray()) Object.DestroyImmediate(go);

            var floorMap = GameObject.Find("Room_Floor").GetComponent<Tilemap>();
            var collisionMap = GameObject.Find("Room_Collision").GetComponent<Tilemap>();
            floorMap.ClearAllTiles(); collisionMap.ClearAllTiles();

            var runnerGo = GameObject.Find("FloorRunner");
            if (runnerGo == null) runnerGo = new GameObject("FloorRunner");
            if (!runnerGo.TryGetComponent(out FloorRunner runner)) runner = runnerGo.AddComponent<FloorRunner>();
            var content = runnerGo.transform.Find("Floor");
            if (content == null) content = Child(runnerGo.transform, "Floor", Vector3.zero);
            var hud = Object.FindAnyObjectByType<HeroHudView>();
            if (hud) { var hso = new SerializedObject(hud); hso.FindProperty("room").objectReferenceValue = null; hso.ApplyModifiedPropertiesWithoutUndo(); }

            var so = new SerializedObject(runner);
            so.FindProperty("floor").objectReferenceValue = AssetDatabase.LoadAssetAtPath<FloorDefinition>("Assets/_Project/Data/Floors/floor_castle.asset");
            so.FindProperty("constants").objectReferenceValue = AssetDatabase.LoadAssetAtPath<BalanceConstants>("Assets/_Project/Data/Constants/constants_slice.asset");
            AssetDatabase.SaveAssets();
            templates = Specs.Select(sp => AssetDatabase.LoadAssetAtPath<RoomTemplate>($"{RoomsDir}/{sp.name}.prefab")).ToArray();   // ссылки на ассеты, а не на временные объекты
            var arr = so.FindProperty("templates"); arr.arraySize = templates.Length;
            for (int i = 0; i < templates.Length; i++) arr.GetArrayElementAtIndex(i).objectReferenceValue = templates[i];
            so.FindProperty("hero").objectReferenceValue = Object.FindAnyObjectByType<HeroController>();
            so.FindProperty("hud").objectReferenceValue = hud;
            so.FindProperty("floorMap").objectReferenceValue = floorMap;
            so.FindProperty("collisionMap").objectReferenceValue = collisionMap;
            so.FindProperty("tileset").objectReferenceValue = tileset;
            so.FindProperty("contentRoot").objectReferenceValue = content;
            so.FindProperty("doorPrefab").objectReferenceValue = AssetDatabase.LoadAssetAtPath<DoorView>(Interact + "/prop_door.prefab");
            so.FindProperty("portalPrefab").objectReferenceValue = AssetDatabase.LoadAssetAtPath<PortalView>(PortalPath);
            so.ApplyModifiedPropertiesWithoutUndo();

            EditorSceneManager.SaveScene(scene);
            var list = EditorBuildSettings.scenes.Where(s => s.path != RunScene).ToList();
            list.Insert(0, new EditorBuildSettingsScene(RunScene, true));
            EditorBuildSettings.scenes = list.ToArray();
        }
    }
}
