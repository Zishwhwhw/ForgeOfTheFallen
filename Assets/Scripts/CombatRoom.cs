using UnityEngine;

namespace FrankenArms
{
    public sealed class CombatRoom : MonoBehaviour
    {
        private int remaining;
        private bool started;
        private bool cleared;
        private int lastBossRewardRoom = -1;
        private RoomDoor[] doors;
        private int roomIndex = 1;
        private int routeModifier;
        private RunSummaryUI summary;
        private HeroSelectionController heroSelection;
        private DungeonRouteDefinition route;
        private RoomNodeDefinition currentNode;

        internal int RoomIndex { get { return roomIndex; } }
        internal bool IsCleared { get { return cleared; } }
        internal int RemainingEnemies { get { return remaining; } }
        internal RoomNodeType CurrentNodeType { get { return currentNode == null ? RoomNodeType.Combat : currentNode.nodeType; } }
        internal string NextNodeLabel
        {
            get
            {
                if (route == null || roomIndex >= 24) return "FINAL";
                var next = route.GetNode(roomIndex + 1);
                return next == null ? "UNKNOWN" : next.nodeType.ToString().ToUpperInvariant();
            }
        }

        internal string RoomKind
        {
            get
            {
                if (CurrentNodeType == RoomNodeType.Boss) return "BOSS GATE";
                if (routeModifier == 3) return "FORGE ROUTE";
                if (currentNode != null && !string.IsNullOrEmpty(currentNode.displayName)) return currentNode.displayName.ToUpperInvariant();
                switch (roomIndex % 5)
                {
                    case 2: return "PURSUIT";
                    case 3: return "CROSSFIRE";
                    case 4: return "GAUNTLET";
                    case 0: return "ELITE GATE";
                    default: return "WARMUP";
                }
            }
        }

        private void Start()
        {
            heroSelection = FindAnyObjectByType<HeroSelectionController>();
            route = Resources.Load<DungeonRouteDefinition>("Content/Routes/FrankenArmsMainRoute");
            currentNode = route == null ? null : route.GetNode(roomIndex);
            if (route == null)
                Debug.LogWarning("Main authored route asset was not found; CombatRoom is using its vertical-slice fallback.");
        }

        private void Update()
        {
            if (!started && (heroSelection == null || heroSelection.IsSelected))
                BeginWave();
        }

        private void BeginWave()
        {
            if (started) return;
            started = true;
            currentNode = route == null ? null : route.GetNode(roomIndex);
            RunProgress.RecordRoomReached(roomIndex);
            var positions = GetWavePositions();
            if (IsSafeNode())
            {
                remaining = 0;
                cleared = true;
                OpenDoors();
                CreateNodeReward();
                CreateSummary();
                Debug.Log("Authored non-combat node opened: " + RoomKind);
                return;
            }
            remaining = positions.Length;
            CreateDoors();
            if (routeModifier == 3 && roomIndex != 6)
            {
                remaining = 0;
                cleared = true;
                OpenDoors();
                CreateSummary();
                Debug.Log(string.Format("Forge route {0} opened", roomIndex));
                return;
            }
            for (var i = 0; i < positions.Length; i++)
                SpawnEnemy(positions[i], IsChaserSlot(i, positions.Length), IsShooterSlot(i), CurrentNodeType == RoomNodeType.Boss && i == 0);
            Debug.Log(string.Format("Combat room {0} started: enemies {1}", roomIndex, positions.Length));
        }

        private Vector3[] GetWavePositions()
        {
            if (CurrentNodeType == RoomNodeType.Boss)
                return new[] { new Vector3(3f, 0f) };
            if (roomIndex % 5 == 2)
                return new[] { new Vector3(3f, 0f), new Vector3(-2.5f, 2f), new Vector3(-2.5f, -2f), new Vector3(0f, 3f) };
            if (roomIndex % 5 == 3)
                return new[] { new Vector3(3f, 2.5f), new Vector3(3f, -2.5f), new Vector3(-3f, 0f), new Vector3(0f, 2.5f), new Vector3(0f, -2.5f) };
            if (roomIndex % 5 == 0)
                return new[] { new Vector3(3f, 2.5f), new Vector3(3f, -2.5f), new Vector3(-3f, 0f), new Vector3(0f, 2.5f), new Vector3(0f, -2.5f), new Vector3(0f, 0f) };
            return new[] { new Vector3(3f, 0f), new Vector3(-2.5f, 2f), new Vector3(-2.5f, -2f) };
        }

        private bool IsChaserSlot(int index, int count)
        {
            var cycle = roomIndex % 5;
            return (cycle == 2 || cycle == 3) && index == count - 1 || cycle == 0 && index >= count - 2;
        }

        private bool IsShooterSlot(int index)
        {
            return (roomIndex % 5 == 3 || routeModifier == 2 && index == 0) && index < 2;
        }

        private bool IsSafeNode()
        {
            if (currentNode == null) return routeModifier == 3 && roomIndex != 6;
            return currentNode.nodeType == RoomNodeType.Forge
                || currentNode.nodeType == RoomNodeType.Reward
                || currentNode.nodeType == RoomNodeType.Secret
                || currentNode.nodeType == RoomNodeType.Shop
                || currentNode.nodeType == RoomNodeType.Shrine;
        }

        private void CreateNodeReward()
        {
            if (currentNode != null
                && currentNode.nodeType != RoomNodeType.Reward
                && currentNode.nodeType != RoomNodeType.Secret)
                return;
            var reward = new GameObject("Authored Node Reward");
            reward.transform.position = currentNode != null && currentNode.nodeType == RoomNodeType.Secret
                ? new Vector3(2f, 1.5f, 0f)
                : Vector3.zero;
            var renderer = reward.AddComponent<SpriteRenderer>();
            renderer.sprite = RuntimeSpriteFactory.CreateCoin();
            renderer.color = currentNode != null && currentNode.nodeType == RoomNodeType.Secret
                ? new Color(0.75f, 0.35f, 1f, 1f)
                : new Color(1f, 0.85f, 0.2f, 1f);
            renderer.sortingOrder = 11;
            var collider = reward.AddComponent<CircleCollider2D>();
            collider.isTrigger = true;
            collider.radius = 0.35f;
            var pickup = reward.AddComponent<CoinPickup>();
            var credits = (currentNode == null ? 10 : Mathf.Max(10, currentNode.rewardCredits)) * RunMode.RewardMultiplier;
            pickup.Initialize(credits, currentNode != null && currentNode.nodeType == RoomNodeType.Secret ? 30 : 20);

            if (currentNode != null && currentNode.nodeType != RoomNodeType.Forge)
            {
                var module = new GameObject(currentNode.nodeType == RoomNodeType.Secret ? "Secret Module Cache" : "Reward Module Cache");
                module.transform.position = reward.transform.position + new Vector3(0f, -1.1f, 0f);
                module.AddComponent<ModuleLootPickup>().Initialize(currentNode.nodeType == RoomNodeType.Secret);
            }
        }

        internal void StartNextRoom()
        {
            StartNextRoom(1);
        }

        internal void StartNextRoom(int routeChoice)
        {
            if (!cleared) return;
            roomIndex++;
            routeModifier = Mathf.Clamp(routeChoice, 1, 3);
            cleared = false;
            started = false;
            if (doors != null)
                for (var i = 0; i < doors.Length; i++) doors[i].Lock();
            BeginWave();
            Debug.Log("Entered room " + roomIndex);
        }

        private void OpenDoors()
        {
            if (doors == null) return;
            for (var i = 0; i < doors.Length; i++) doors[i].Open();
        }

        private void CreateSummary()
        {
            if (summary == null)
            {
                summary = gameObject.AddComponent<RunSummaryUI>();
                summary.Initialize(this);
            }
        }

        private void SpawnEnemy(Vector3 position, bool isChaser, bool isShooter, bool isBoss)
        {
            var enemyObject = new GameObject(isBoss ? "Boss Gate" : isChaser ? "Enemy Chaser" : isShooter ? "Enemy Shooter" : "Enemy Target");
            enemyObject.transform.position = position;
            var isElite = isBoss || isChaser && roomIndex % 5 == 0;
            var renderer = enemyObject.AddComponent<SpriteRenderer>();
            var authoredSprite = isElite
                ? RuntimeSpriteFactory.CreateEliteEnemy()
                : isChaser ? RuntimeSpriteFactory.CreateEnemyChaser() : RuntimeSpriteFactory.CreateEnemy();
            renderer.sprite = authoredSprite;
            renderer.color = isBoss
                ? new Color(0.9f, 0.2f, 0.85f, 1f)
                : isElite
                ? new Color(0.75f, 0.45f, 1f, 1f)
                : isChaser
                ? new Color(1f, 0.55f, 0.18f, 1f)
                : isShooter
                ? new Color(1f, 0.78f, 0.18f, 1f)
                : new Color(0.95f, 0.25f, 0.3f, 1f);
            renderer.sortingOrder = 9;
            if (isBoss) enemyObject.transform.localScale = new Vector3(1.45f, 1.45f, 1f);
            var body = enemyObject.AddComponent<Rigidbody2D>();
            body.bodyType = RigidbodyType2D.Kinematic;
            body.gravityScale = 0f;
            body.freezeRotation = true;
            body.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
            var collider = enemyObject.AddComponent<CircleCollider2D>();
            collider.radius = 0.38f;
            var baseHealth = isBoss ? 18 : isChaser && roomIndex % 5 == 0 ? 4 : 3;
            enemyObject.AddComponent<EnemyTarget>().Initialize(this, Mathf.CeilToInt(baseHealth * RunMode.EnemyHealthMultiplier));
            enemyObject.AddComponent<StatusEffectController>();
            enemyObject.AddComponent<EnemyContactDamage>();
            if (isChaser) enemyObject.AddComponent<EnemyChaser>();
            if (isShooter) enemyObject.AddComponent<EnemyShooter>();
            if (isBoss)
                enemyObject.AddComponent<EnemyBoss>().Initialize(roomIndex);
            enemyObject.AddComponent<PixelAnimator>().Initialize(RuntimeSpriteFactory.CreateEnemyFrames(authoredSprite));
        }

        internal void EnemyDefeated()
        {
            remaining--;
            if (remaining > 0 || cleared) return;
            cleared = true;
            if (CurrentNodeType == RoomNodeType.Boss && lastBossRewardRoom != roomIndex)
            {
                lastBossRewardRoom = roomIndex;
                Achievements.Unlock("boss_clear");
                SpawnBossMetaRewards();
            }
            OpenDoors();
            CreateSummary();
            Debug.Log("Combat room cleared: reward spawned");
            var reward = new GameObject("Room Reward");
            reward.transform.position = Vector3.zero;
            var renderer = reward.AddComponent<SpriteRenderer>();
            renderer.sprite = RuntimeSpriteFactory.CreateCoin();
            renderer.color = new Color(1f, 0.85f, 0.2f, 1f);
            renderer.sortingOrder = 11;
            var collider = reward.AddComponent<CircleCollider2D>();
            collider.isTrigger = true;
            collider.radius = 0.35f;
            reward.AddComponent<CoinPickup>();
        }

        private void SpawnBossMetaRewards()
        {
            SpawnMetaPickup("Crystal Reward", new Vector3(-0.75f, 0f, 0f), MetaPickupType.Crystal);
            SpawnMetaPickup("Blueprint Reward", new Vector3(0f, 0f, 0f), MetaPickupType.Blueprint);
            SpawnMetaPickup("Diary Reward", new Vector3(0.75f, 0f, 0f), MetaPickupType.Diary);
            Debug.Log("Boss meta rewards spawned: crystal, blueprint, diary");
        }

        private static void SpawnMetaPickup(string name, Vector3 position, MetaPickupType type)
        {
            var reward = new GameObject(name);
            reward.transform.position = position;
            reward.AddComponent<MetaPickup>().Initialize(type);
        }

        private void CreateDoors()
        {
            doors = new[]
            {
                CreateDoor(new Vector3(-7.5f, 0f, 0f), false),
                CreateDoor(new Vector3(7.5f, 0f, 0f), true)
            };
        }

        private RoomDoor CreateDoor(Vector3 position, bool isExit)
        {
            var doorObject = new GameObject("Room Door");
            doorObject.transform.position = position;
            doorObject.transform.localScale = new Vector3(0.5f, 2.2f, 1f);
            var renderer = doorObject.AddComponent<SpriteRenderer>();
            renderer.sprite = RuntimeSpriteFactory.CreateDoor();
            renderer.sortingOrder = 8;
            var door = doorObject.AddComponent<RoomDoor>();
            door.Lock();
            if (isExit)
            {
                var exit = doorObject.AddComponent<RoomExit>();
                exit.Initialize(this);
                door.SetExit(exit);
            }
            return door;
        }
    }
}
