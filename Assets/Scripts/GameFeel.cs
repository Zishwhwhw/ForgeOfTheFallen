using System.Collections;
using UnityEngine;

namespace FrankenArms
{
    /// <summary>Very short unscaled hit-stop for high-impact hits.</summary>
    public sealed class GameFeel : MonoBehaviour
    {
        private static GameFeel instance;
        private Coroutine stopRoutine;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void Create()
        {
            if (instance != null) return;
            var go = new GameObject("Game Feel");
            instance = go.AddComponent<GameFeel>();
            DontDestroyOnLoad(go);
        }

        internal static void HitStop(float duration)
        {
            if (instance == null) Create();
            if (instance != null) instance.StartHitStop(duration);
        }

        private void StartHitStop(float duration)
        {
            if (stopRoutine != null) StopCoroutine(stopRoutine);
            stopRoutine = StartCoroutine(HitStopRoutine(Mathf.Clamp(duration, 0.01f, 0.08f)));
        }

        private IEnumerator HitStopRoutine(float duration)
        {
            var previousScale = Time.timeScale;
            if (previousScale <= 0f) yield break;
            Time.timeScale = 0f;
            yield return new WaitForSecondsRealtime(duration);
            Time.timeScale = previousScale;
            stopRoutine = null;
        }
    }
}
