using UnityEngine;

namespace FotF.View.Combat
{
    /// <summary>
    /// Микропауза на убийстве и тяжёлых ударах (GDD §10.2: 20–30 мс). Объект кладётся в сцену в редакторе.
    /// </summary>
    public sealed class HitStop : MonoBehaviour
    {
        [Tooltip("Множитель силы ощущений (GDD §10.2: слайдер 0/1/2)")]
        [SerializeField, Range(0f, 2f)] float strength = 1f;

        static HitStop _instance;
        float _realTimeLeft;

        void Awake() { _instance = this; }
        void OnDestroy() { if (_instance == this) { _instance = null; Time.timeScale = 1f; } }

        public static void Request(float seconds)
        {
            if (_instance == null || _instance.strength <= 0f) return;
            _instance._realTimeLeft = Mathf.Max(_instance._realTimeLeft, seconds * _instance.strength);
            Time.timeScale = 0f;
        }

        void Update()
        {
            if (_realTimeLeft <= 0f) return;
            _realTimeLeft -= Time.unscaledDeltaTime;
            if (_realTimeLeft <= 0f) Time.timeScale = 1f;
        }
    }
}
