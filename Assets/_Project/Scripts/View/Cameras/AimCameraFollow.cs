using FotF.View.Heroes;
using UnityEngine;

namespace FotF.View.Cameras
{
    /// <summary>Камера следует за героем со сдвигом к прицелу (GDD §3.1).</summary>
    public sealed class AimCameraFollow : MonoBehaviour
    {
        [SerializeField] HeroController target;
        [Tooltip("Доля расстояния до прицела, на которую сдвигается камера")]
        [SerializeField, Range(0f, 0.5f)] float aimLead = 0.2f;
        [Tooltip("Максимальный сдвиг к прицелу, юниты")]
        [SerializeField] float maxLead = 2.5f;
        [SerializeField] float smoothTime = 0.12f;

        Vector3 _velocity;

        void LateUpdate()
        {
            if (target == null) return;
            Vector2 hero = target.transform.position;
            Vector2 lead = Vector2.ClampMagnitude((target.AimWorldPoint - hero) * aimLead, maxLead);
            Vector3 goal = new Vector3(hero.x + lead.x, hero.y + lead.y, transform.position.z);
            transform.position = Vector3.SmoothDamp(transform.position, goal, ref _velocity, smoothTime);
        }
    }
}
