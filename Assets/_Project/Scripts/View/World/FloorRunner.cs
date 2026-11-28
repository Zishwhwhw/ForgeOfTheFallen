using System.Collections.Generic;
using FotF.Data;
using FotF.Logic;
using FotF.Logic.Generation;
using FotF.View.Combat;
using FotF.View.Enemies;
using FotF.View.Heroes;
using FotF.View.UI;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace FotF.View.World
{
    /// <summary>
    /// Сборщик этажа в сцене Run (ARCHITECTURE §4): FloorGenerator строит граф → для каждой комнаты
    /// ставится шаблон-префаб на сетке, коридоры между связанными комнатами, стены рисует DualGridPainter.
    /// Портал — в последней зачищенной обязательной боевой комнате (GDD §3.1); через него — следующий этаж.
    /// </summary>
    public sealed class FloorRunner : MonoBehaviour
    {
        [Header("Данные")]
        [SerializeField] FloorDefinition floor;
        [SerializeField] BalanceConstants constants;
        [SerializeField] RoomTemplate[] templates = System.Array.Empty<RoomTemplate>();

        [Header("Сцена")]
        [SerializeField] HeroController hero;
        [SerializeField] HeroHudView hud;
        [SerializeField] Tilemap floorMap;
        [SerializeField] Tilemap collisionMap;
        [SerializeField] TerrainTileset tileset;
        [SerializeField] Transform contentRoot;

        [Header("Префабы")]
        [SerializeField] DoorView doorPrefab;
        [SerializeField] PortalView portalPrefab;

        [Header("Сетка (клеток-тайлов на комнату)")]
        [SerializeField] Vector2Int pitch = new Vector2Int(30, 24);
        [Tooltip("Ширина коридора = 2·N+1 клеток")]
        [SerializeField] int corridorHalfWidth = 1;
        [Tooltip("0 — случайный seed на каждый этаж")]
        [SerializeField] int fixedSeed;

        public int FloorNumber { get; private set; } = 1;
        public int Seed { get; private set; }
        public FloorPlan Plan { get; private set; }
        public IReadOnlyDictionary<FloorRoom, RoomTemplate> Rooms => _rooms;
        public event System.Action<FloorRunner> FloorBuilt;

        readonly Dictionary<FloorRoom, RoomTemplate> _rooms = new Dictionary<FloorRoom, RoomTemplate>();
        readonly HashSet<RoomController> _required = new HashSet<RoomController>();
        PortalView _portal;

        void Start() => Build(1);

        public FloorRules Rules() => new FloorRules
        {
            CombatMin = floor.combatRoomsMin, CombatMax = floor.combatRoomsMax, MaxBranches = floor.maxBranches,
            Forge = floor.forgeChance, Merchant = floor.merchantChance, Fountain = floor.fountainChance, Shrine = floor.shrineChance,
            Cage = floor.cageChance, Secret = floor.secretChance, Elite = floor.eliteChance, ChestRoom = floor.chestRoomChance,
            MiniBoss = constants ? constants.miniBossRoomChance : 0.3f,
        };

        public void Build(int floorNumber)
        {
            Clear();
            FloorNumber = floorNumber;
            Seed = fixedSeed != 0 ? fixedSeed + floorNumber : System.Environment.TickCount ^ (floorNumber * 7919);
            var rng = new SeededRandom(Seed);
            Plan = FloorGenerator.Generate(Rules(), rng);

            var layout = FloorLayout.Compute(Plan, r => Pick(r.Kind, rng), pitch, corridorHalfWidth);
            foreach (var room in Plan.Rooms)
                if (!room.Hidden && !layout.Rooms.ContainsKey(room)) Debug.LogError($"[Этаж] нет шаблона для {room.Kind}", this);
            foreach (var kv in layout.Rooms)
            {
                var inst = Instantiate(kv.Value.Template, new Vector3(kv.Value.Offset.x, kv.Value.Offset.y, 0f), Quaternion.identity, contentRoot);
                inst.name = $"{kv.Key.Kind}_{kv.Key.Index}";
                inst.shape.GetComponent<TilemapRenderer>().enabled = false;
                _rooms[kv.Key] = inst;
            }
            var walk = layout.Walk;
            var doors = new Dictionary<FloorRoom, List<DoorView>>();
            foreach (var d in layout.Doors) AddDoor(d.Room, d.Position, d.Sideways, doors);

            var missing = DualGridPainter.Paint(walk, floorMap, collisionMap, tileset);
            if (missing.Count > 0) Debug.LogWarning($"[Этаж] в тайлсете нет тайлов для углов: {string.Join(", ", missing)}", this);

            foreach (var kv in _rooms)
            {
                var fight = kv.Value.fight;
                if (fight == null) continue;
                fight.SetDoors(doors.TryGetValue(kv.Key, out var list) ? list.ToArray() : System.Array.Empty<DoorView>());
                if (hud) fight.Announce += hud.ShowAnnouncement;
                if (kv.Key.RequiredForPortal) { _required.Add(fight); fight.Cleared += OnRoomCleared; }
            }

            var start = _rooms[Plan.Start];
            hero.transform.position = start.heroSpawn ? start.heroSpawn.position : start.transform.position;
            if (hud) hud.ShowAnnouncement($"Этаж {FloorNumber} / {floor.floorsPerBiome}");
            FloorBuilt?.Invoke(this);
        }

        RoomTemplate Pick(RoomKind kind, IRandom rng)
        {
            var fits = new List<RoomTemplate>();
            foreach (var t in templates) if (t && t.Serves(kind)) fits.Add(t);
            return fits.Count == 0 ? null : fits[rng.NextInt(0, fits.Count)];
        }

        void AddDoor(FloorRoom room, Vector2 at, bool sideways, Dictionary<FloorRoom, List<DoorView>> doors)
        {
            var fight = _rooms[room].fight;
            if (fight == null || doorPrefab == null) return;
            var d = Instantiate(doorPrefab, at, sideways ? Quaternion.Euler(0, 0, 90f) : Quaternion.identity, _rooms[room].transform);
            d.transform.localScale = new Vector3(1.5f * (corridorHalfWidth * 2 + 1) / 3f, 1f, 1f);
            if (!doors.TryGetValue(room, out var list)) doors[room] = list = new List<DoorView>();
            list.Add(d);
        }

        void OnRoomCleared(RoomController room)
        {
            _required.Remove(room);
            if (_required.Count > 0) { if (hud) hud.ShowAnnouncement($"Комната зачищена · осталось {_required.Count}"); return; }
            RoomTemplate t = null;
            foreach (var kv in _rooms) if (kv.Value.fight == room) t = kv.Value;
            var at = t && t.portalPoint ? t.portalPoint.position : room.transform.position;
            _portal = Instantiate(portalPrefab, at, Quaternion.identity, contentRoot);
            _portal.Used += _ => Next();
            if (hud) hud.ShowAnnouncement("Открылся портал вниз");
        }

        void Next()
        {
            int next = FloorNumber + 1;
            if (next > floor.floorsPerBiome)
            {
                // Босс Голем — отдельная сцена (следующая поставка). Пока — новый забег с первого этажа.
                next = 1;
            }
            Build(next);
            if (hud && FloorNumber == 1) hud.ShowAnnouncement("Босс Голем — скоро. Новый забег: этаж 1");
        }

        public void Clear()
        {
            _required.Clear(); _rooms.Clear(); _portal = null;
            if (contentRoot) for (int i = contentRoot.childCount - 1; i >= 0; i--) Destroy(contentRoot.GetChild(i).gameObject);
            foreach (var p in FindObjectsByType<WeaponPickupView>(FindObjectsInactive.Exclude)) Destroy(p.gameObject);
            foreach (var p in FindObjectsByType<ProjectileView>(FindObjectsInactive.Exclude)) Destroy(p.gameObject);
            CorpseRegistry.ClearFloor();
            if (floorMap) floorMap.ClearAllTiles();
            if (collisionMap) collisionMap.ClearAllTiles();
        }
    }
}
