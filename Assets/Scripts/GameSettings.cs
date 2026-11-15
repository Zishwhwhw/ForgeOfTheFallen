using UnityEngine;

namespace FrankenArms
{
    public static class GameSettings
    {
        private const string MasterKey = "FrankenArms.Settings.Master";
        private const string SfxKey = "FrankenArms.Settings.Sfx";
        private const string ShakeKey = "FrankenArms.Settings.Shake";

        public static float MasterVolume { get { return PlayerPrefs.GetFloat(MasterKey, 1f); } }
        public static float SfxVolume { get { return PlayerPrefs.GetFloat(SfxKey, 1f); } }
        public static bool ScreenShake { get { return PlayerPrefs.GetInt(ShakeKey, 1) == 1; } }

        public static void SetMaster(float value) { PlayerPrefs.SetFloat(MasterKey, Mathf.Clamp01(value)); PlayerPrefs.Save(); }
        public static void SetSfx(float value) { PlayerPrefs.SetFloat(SfxKey, Mathf.Clamp01(value)); PlayerPrefs.Save(); }
        public static void SetScreenShake(bool enabled) { PlayerPrefs.SetInt(ShakeKey, enabled ? 1 : 0); PlayerPrefs.Save(); }
    }
}
