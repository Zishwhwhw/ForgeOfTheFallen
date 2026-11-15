using UnityEngine;

namespace FrankenArms
{
    public sealed class ScreenShake : MonoBehaviour
    {
        private static ScreenShake instance;
        private Vector3 basePosition;
        private float remaining;
        private float strength;

        private void Awake()
        {
            instance = this;
            basePosition = transform.position;
        }

        internal static void Pulse(float duration, float amount)
        {
            if (!GameSettings.ScreenShake) return;
            if (instance == null) return;
            instance.remaining = Mathf.Max(instance.remaining, duration);
            instance.strength = Mathf.Max(instance.strength, amount);
        }

        private void LateUpdate()
        {
            if (remaining <= 0f)
            {
                transform.position = basePosition;
                return;
            }
            remaining -= Time.unscaledDeltaTime;
            transform.position = basePosition + (Vector3)(Random.insideUnitCircle * strength);
            strength = Mathf.MoveTowards(strength, 0f, Time.unscaledDeltaTime * 2f);
        }
    }
}
