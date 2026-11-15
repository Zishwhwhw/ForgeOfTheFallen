using System.Collections.Generic;
using UnityEngine;

namespace FrankenArms
{
    public static class Achievements
    {
        private static readonly string[] IDs = { "first_kill", "first_fusion", "first_meta", "boss_clear", "daily_clear" };
        private static readonly string[] Labels = { "FIRST KILL", "FIRST FUSION", "FIRST META LOOT", "BOSS CLEAR", "DAILY CLEAR" };
        private static readonly string[] Descriptions = { "Defeat your first threat.", "Equip a fused weapon.", "Collect a persistent reward.", "Defeat an authored boss.", "Complete the daily route." };
        private const string Prefix = "FrankenArms.Achievement.";

        public static int UnlockedCount
        {
            get
            {
                var count = 0;
                for (var i = 0; i < IDs.Length; i++) if (IsUnlocked(IDs[i])) count++;
                return count;
            }
        }

        public static bool IsUnlocked(string id) { return PlayerPrefs.GetInt(Prefix + id, 0) == 1; }
        public static int Count { get { return IDs.Length; } }
        public static string LabelAt(int index) { return Labels[Mathf.Clamp(index, 0, Labels.Length - 1)]; }
        public static string DescriptionAt(int index) { return Descriptions[Mathf.Clamp(index, 0, Descriptions.Length - 1)]; }
        public static bool IsUnlockedAt(int index) { return IsUnlocked(IDs[Mathf.Clamp(index, 0, IDs.Length - 1)]); }

        public static void Unlock(string id)
        {
            if (string.IsNullOrEmpty(id) || IsUnlocked(id)) return;
            PlayerPrefs.SetInt(Prefix + id, 1);
            PlayerPrefs.Save();
            Debug.Log("Achievement unlocked: " + id);
        }
    }
}
