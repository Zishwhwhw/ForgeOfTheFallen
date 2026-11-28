using System;

namespace FotF.State.Save
{
    /// <summary>settings.json (ARCHITECTURE §8.4): звук, язык, слайдер game feel, тряска, раскладка.</summary>
    [Serializable]
    public sealed class SettingsState
    {
        public const int CurrentVersion = 1;
        public const string FileName = "settings.json";

        public int saveVersion = CurrentVersion;
        public float masterVolume = 1f;
        public float musicVolume = 1f;
        public float sfxVolume = 1f;
        /// <summary>Код языка (ru/en/pl, GDD §10.4). Пусто — язык системы.</summary>
        public string language = "";
        /// <summary>Сила отклика боя, 0..1 (GDD §10.2: слайдер game feel).</summary>
        public float gameFeel = 1f;
        public bool screenShake = true;
        /// <summary>Переназначенные клавиши: JSON overrides Input System. Пусто — по умолчанию.</summary>
        public string bindingOverrides = "";

        /// <summary>Привести значения к допустимым (ручная правка файла не должна ломать игру).</summary>
        public void Sanitize()
        {
            masterVolume = Clamp01(masterVolume); musicVolume = Clamp01(musicVolume); sfxVolume = Clamp01(sfxVolume);
            gameFeel = Clamp01(gameFeel);
            language ??= ""; bindingOverrides ??= "";
        }

        static float Clamp01(float v) => float.IsNaN(v) ? 1f : v < 0f ? 0f : v > 1f ? 1f : v;
    }
}
