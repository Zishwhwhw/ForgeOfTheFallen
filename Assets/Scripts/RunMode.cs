using UnityEngine;

namespace FrankenArms
{
    public static class RunMode
    {
        private const string Key = "FrankenArms.RunMode";
        public static int Current { get { return PlayerPrefs.GetInt(Key, 0); } }
        public static bool Hardcore { get { return Current == 1; } }
        public static bool Daily { get { return Current == 2; } }
        public static string DisplayName { get { return Daily ? "DAILY" : Hardcore ? "HARDCORE" : "NORMAL"; } }
        public static string DailyModifierA { get { return Daily ? (DailySeed % 3 == 0 ? "HOT CORE" : DailySeed % 3 == 1 ? "IRON HIDE" : "RICH VEINS") : string.Empty; } }
        public static string DailyModifierB { get { return Daily ? (DailySeed % 2 == 0 ? "FAST THREATS" : "ARCANE LOOT") : string.Empty; } }
        public static float EnemyHealthMultiplier
        {
            get
            {
                if (Hardcore) return 2f;
                if (!Daily) return 1f;
                return DailyModifierA == "IRON HIDE" ? 1.5f : DailyModifierB == "FAST THREATS" ? 1.35f : 1.25f;
            }
        }
        public static int RewardMultiplier { get { return Hardcore || Daily ? (Daily && DailyModifierA == "RICH VEINS" ? 3 : 2) : 1; } }
        public static void Toggle()
        {
            PlayerPrefs.SetInt(Key, (Current + 1) % 3);
            PlayerPrefs.Save();
        }

        public static int DailySeed
        {
            get
            {
                var now = System.DateTime.UtcNow;
                return now.Year * 10000 + now.Month * 100 + now.Day;
            }
        }
    }
}
