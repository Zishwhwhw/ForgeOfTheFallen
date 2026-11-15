using System.Collections.Generic;
using UnityEngine;

namespace FrankenArms
{
    /// <summary>Small zero-setup SFX service for the vertical slice.</summary>
    public sealed class GameAudio : MonoBehaviour
    {
        private static GameAudio instance;
        private readonly Dictionary<string, AudioClip> clips = new();
        private AudioSource source;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void Create()
        {
            if (instance != null) return;
            var go = new GameObject("Game Audio");
            instance = go.AddComponent<GameAudio>();
            DontDestroyOnLoad(go);
        }

        private void Awake()
        {
            if (instance != null && instance != this)
            {
                Destroy(gameObject);
                return;
            }
            instance = this;
            source = gameObject.AddComponent<AudioSource>();
            source.playOnAwake = false;
            source.spatialBlend = 0f;
            source.volume = 0.65f * GameSettings.MasterVolume * GameSettings.SfxVolume;
        }

        public static void Play(string clipName, float volume = 1f)
        {
            if (instance == null) Create();
            if (instance == null || instance.source == null) return;
            if (!instance.clips.TryGetValue(clipName, out var clip))
            {
                clip = Resources.Load<AudioClip>("Audio/" + clipName);
                instance.clips[clipName] = clip;
            }
            instance.source.volume = 0.65f * GameSettings.MasterVolume * GameSettings.SfxVolume;
            if (clip != null) instance.source.PlayOneShot(clip, Mathf.Clamp01(volume));
        }
    }
}
