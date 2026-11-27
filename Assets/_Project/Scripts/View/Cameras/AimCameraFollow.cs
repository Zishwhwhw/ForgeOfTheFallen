using FotF.View.Heroes;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace FotF.View.Cameras
{
    /// <summary>Камера следует за героем со сдвигом к прицелу (GDD §3.1) и не выходит за границы комнаты.</summary>
    public sealed class AimCameraFollow : MonoBehaviour
    {
        [SerializeField] HeroController target;
        [Tooltip("Доля расстояния до прицела, на которую сдвигается камера")]
        [SerializeField, Range(0f, 0.5f)] float aimLead = 0.2f;
        [Tooltip("Максимальный сдвиг к прицелу, юниты")]
        [SerializeField] float maxLead = 2.5f;
        [SerializeField] float smoothTime = 0.12f;
        [Tooltip("Тайлмап комнаты: камера не показывает пустоту за ним. Пусто — без ограничения")]
        [SerializeField] Tilemap roomBounds;
        [Tooltip("На сколько юнитов можно выйти за край тайлмапа (стены и дверь над комнатой)")]
        [SerializeField, Min(0f)] float boundsPadding = 1f;

        Vector3 _velocity;
        Camera _camera;

        void Awake() => _camera = GetComponent<Camera>();

        void LateUpdate()
        {
            if (target == null) return;
            Vector2 hero = target.transform.position;
            Vector2 lead = Vector2.ClampMagnitude((target.AimWorldPoint - hero) * aimLead, maxLead);
            Vector3 goal = ClampToRoom(new Vector3(hero.x + lead.x, hero.y + lead.y, transform.position.z));
            transform.position = Vector3.SmoothDamp(transform.position, goal, ref _velocity, smoothTime);
        }

        Vector3 ClampToRoom(Vector3 goal)
        {
            if (roomBounds == null || _camera == null || !_camera.orthographic) return goal;
            Bounds local = roomBounds.localBounds;
            Vector3 min = roomBounds.transform.TransformPoint(local.min) - Vector3.one * boundsPadding;
            Vector3 max = roomBounds.transform.TransformPoint(local.max) + Vector3.one * boundsPadding;
            float halfHeight = _camera.orthographicSize;
            float halfWidth = halfHeight * _camera.aspect;
            goal.x = ClampAxis(goal.x, min.x, max.x, halfWidth);
            goal.y = ClampAxis(goal.y, min.y, max.y, halfHeight);
            return goal;
        }

        /// <summary>Держит видимую половину кадра внутри [min, max]; если комната уже кадра — центрирует.</summary>
        public static float ClampAxis(float value, float min, float max, float halfView)
        {
            float lo = min + halfView, hi = max - halfView;
            return lo > hi ? (min + max) * 0.5f : Mathf.Clamp(value, lo, hi);
        }
    }
}
