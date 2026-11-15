using UnityEngine;
using UnityEngine.SceneManagement;

namespace FrankenArms
{
    /// <summary>Scene-aware ambient loop using the locally imported hub audio.</summary>
    public sealed class AmbientAudio : MonoBehaviour
    {
        private static AmbientAudio instance;
        private AudioSource source;
        private string currentClip;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void Create()
        {
            if (instance != null) return;
            var go = new GameObject("Ambient Audio");
            instance = go.AddComponent<AmbientAudio>();
            DontDestroyOnLoad(go);
        }

        private void Awake()
        {
            source = gameObject.AddComponent<AudioSource>();
            source.loop = true;
            source.playOnAwake = false;
            source.spatialBlend = 0f;
            SceneManager.sceneLoaded += OnSceneLoaded;
        }

        private void Start()
        {
            OnSceneLoaded(SceneManager.GetActiveScene(), LoadSceneMode.Single);
        }

        private void OnDestroy()
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
        }

        private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            var clipName = scene.name == SceneFlow.Lobby ? "KitchenTheme" : string.Empty;
            if (clipName == currentClip) return;
            currentClip = clipName;
            source.Stop();
            if (string.IsNullOrEmpty(clipName)) return;
            var clip = Resources.Load<AudioClip>("Audio/" + clipName);
            if (clip == null) return;
            source.clip = clip;
            source.volume = 0.3f * GameSettings.MasterVolume;
            source.Play();
        }
    }
}
