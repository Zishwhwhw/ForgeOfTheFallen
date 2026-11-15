using System.Collections.Generic;
using UnityEngine;

namespace FrankenArms
{
    public sealed class EnemyBoss : MonoBehaviour
    {
        [SerializeField] private float moveSpeed = 0.65f;
        [SerializeField] private float burstInterval = 1.8f;
        private Rigidbody2D body;
        private Transform target;
        private float burstTimer;
        private EnemyTarget healthTarget;
        private bool phaseTwo;
        private int bossFloor = 6;
        private bool telegraphing;
        private float telegraphTimer;
        private readonly List<GameObject> telegraphObjects = new();

        internal void Initialize(int floor)
        {
            bossFloor = floor;
        }

        private void Awake()
        {
            body = GetComponent<Rigidbody2D>();
            target = FindAnyObjectByType<PlayerMovement>()?.transform;
            healthTarget = GetComponent<EnemyTarget>();
            burstTimer = 1.2f;
        }

        private void FixedUpdate()
        {
            if (body == null || target == null) return;
            UpdatePhase();
            var offset = (Vector2)(target.position - transform.position);
            if (offset.sqrMagnitude > (phaseTwo ? 4.84f : 6.25f))
                body.MovePosition(body.position + offset.normalized * (phaseTwo ? moveSpeed * 1.35f : moveSpeed) * Time.fixedDeltaTime);
        }

        private void Update()
        {
            if (target == null) return;
            UpdatePhase();
            if (telegraphing)
            {
                telegraphTimer -= Time.deltaTime;
                if (telegraphTimer <= 0f)
                {
                    telegraphing = false;
                    ClearTelegraph();
                    FireBurst();
                    burstTimer = phaseTwo ? 1.15f : burstInterval;
                }
                return;
            }
            burstTimer -= Time.deltaTime;
            if (burstTimer > 0f) return;
            ShowTelegraph();
        }

        private void UpdatePhase()
        {
            if (phaseTwo || healthTarget == null || healthTarget.HealthRatio > 0.5f) return;
            phaseTwo = true;
            GameAudio.Play("Explosion", 0.65f);
            GameFeel.HitStop(0.05f);
            ScreenShake.Pulse(0.22f, 0.16f);
            Debug.Log("Boss phase 2: accelerated burst pattern");
        }

        private void FireBurst()
        {
            var direction = (Vector2)(target.position - transform.position);
            if (direction.sqrMagnitude < 0.01f) return;
            direction.Normalize();
            foreach (var boltDirection in GetBurstDirections(direction)) SpawnBolt(boltDirection);
        }

        private List<Vector2> GetBurstDirections(Vector2 direction)
        {
            var directions = new List<Vector2>();
            if (bossFloor >= 24)
                AddFan(directions, direction, phaseTwo ? 80f : 60f, phaseTwo ? 9 : 7);
            else if (bossFloor >= 20)
                AddFan(directions, direction, phaseTwo ? 68f : 48f, phaseTwo ? 7 : 5);
            else if (bossFloor >= 15)
                AddFan(directions, direction, phaseTwo ? 74f : 58f, phaseTwo ? 8 : 6);
            else if (bossFloor >= 10)
            {
                directions.Add(direction);
                directions.Add(Rotate(direction, 90f));
                directions.Add(Rotate(direction, -90f));
                directions.Add(Rotate(direction, 180f));
                if (phaseTwo)
                {
                    directions.Add(Rotate(direction, 35f));
                    directions.Add(Rotate(direction, -35f));
                }
            }
            else
            {
                directions.Add(direction);
                directions.Add(Rotate(direction, 22f));
                directions.Add(Rotate(direction, -22f));
                if (phaseTwo)
                {
                    directions.Add(Rotate(direction, 42f));
                    directions.Add(Rotate(direction, -42f));
                }
            }
            return directions;
        }

        private static void AddFan(List<Vector2> directions, Vector2 direction, float degrees, int count)
        {
            for (var i = 0; i < count; i++)
            {
                var t = count <= 1 ? 0.5f : (float)i / (count - 1);
                directions.Add(Rotate(direction, Mathf.Lerp(-degrees, degrees, t)));
            }
        }

        private void ShowTelegraph()
        {
            var direction = (Vector2)(target.position - transform.position);
            if (direction.sqrMagnitude < 0.01f) return;
            direction.Normalize();
            telegraphing = true;
            telegraphTimer = 0.35f;
            foreach (var boltDirection in GetBurstDirections(direction))
            {
                var warning = new GameObject("Boss Telegraph");
                var line = warning.AddComponent<LineRenderer>();
                line.material = new Material(Shader.Find("Sprites/Default"));
                line.startColor = new Color(1f, 0.3f, 0.35f, 0.7f);
                line.endColor = new Color(1f, 0.7f, 0.25f, 0.15f);
                line.startWidth = 0.07f;
                line.endWidth = 0.02f;
                line.positionCount = 2;
                line.sortingOrder = 10;
                line.SetPosition(0, transform.position);
                line.SetPosition(1, transform.position + (Vector3)boltDirection * 5.5f);
                telegraphObjects.Add(warning);
            }
        }

        private void ClearTelegraph()
        {
            for (var i = 0; i < telegraphObjects.Count; i++)
                if (telegraphObjects[i] != null) Destroy(telegraphObjects[i]);
            telegraphObjects.Clear();
        }

        private void OnDestroy()
        {
            ClearTelegraph();
        }

        private static Vector2 Rotate(Vector2 value, float degrees)
        {
            var radians = degrees * Mathf.Deg2Rad;
            var sin = Mathf.Sin(radians);
            var cos = Mathf.Cos(radians);
            return new Vector2(value.x * cos - value.y * sin, value.x * sin + value.y * cos);
        }

        private void SpawnBolt(Vector2 direction)
        {
            var projectileObject = new GameObject("Boss Bolt");
            projectileObject.transform.position = transform.position + (Vector3)(direction * 0.72f);
            var renderer = projectileObject.AddComponent<SpriteRenderer>();
            renderer.sprite = RuntimeSpriteFactory.CreateProjectile();
            renderer.color = bossFloor >= 24
                ? new Color(1f, 0.25f, 0.35f, 1f)
                : bossFloor >= 15
                    ? new Color(0.75f, 0.35f, 1f, 1f)
                    : new Color(0.8f, 0.3f, 1f, 1f);
            renderer.sortingOrder = 12;
            var boltBody = projectileObject.AddComponent<Rigidbody2D>();
            boltBody.bodyType = RigidbodyType2D.Kinematic;
            boltBody.gravityScale = 0f;
            boltBody.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
            var collider = projectileObject.AddComponent<CircleCollider2D>();
            collider.isTrigger = true;
            collider.radius = 0.14f;
            projectileObject.AddComponent<EnemyProjectile>().Launch(direction, 5.1f, StatusEffectType.Frozen);
        }
    }
}
