using UnityEngine;

namespace FrankenArms
{
    /// <summary>
    /// Persistent, non-combat-power progression from the GDD. Runtime state is
    /// kept in PlayerPrefs; content definitions remain immutable assets.
    /// </summary>
    public static class MetaProgression
    {
        private const string CrystalsKey = "FrankenArms.Crystals";
        private const string BlueprintsKey = "FrankenArms.Blueprints";
        private const string DiaryKey = "FrankenArms.DiaryEntries";
        private const string GardenKey = "FrankenArms.Room.Garden";
        private const string WorkshopKey = "FrankenArms.Room.Workshop";
        private const string ModulePackKey = "FrankenArms.ModulePack.Basic";

        public static int Crystals { get { return PlayerPrefs.GetInt(CrystalsKey, 0); } }
        public static int Blueprints { get { return PlayerPrefs.GetInt(BlueprintsKey, 0); } }
        public static int DiaryEntries { get { return PlayerPrefs.GetInt(DiaryKey, 0); } }
        public static bool GardenUnlocked { get { return PlayerPrefs.GetInt(GardenKey, 0) == 1; } }
        public static bool WorkshopUnlocked { get { return PlayerPrefs.GetInt(WorkshopKey, 0) == 1; } }
        public static bool ModulePackUnlocked { get { return PlayerPrefs.GetInt(ModulePackKey, 0) == 1; } }

        public static void RecordBossClear(string bossName)
        {
            AddCrystals(3);
            UnlockBlueprint();
            UnlockDiaryEntry();
            Debug.Log("Meta progression saved: " + bossName + " // crystal +3 // blueprint +1 // diary +1");
        }

        public static void AddCrystals(int amount)
        {
            PlayerPrefs.SetInt(CrystalsKey, Crystals + Mathf.Max(0, amount));
            PlayerPrefs.Save();
        }

        public static void UnlockBlueprint()
        {
            PlayerPrefs.SetInt(BlueprintsKey, Blueprints + 1);
            PlayerPrefs.Save();
        }

        public static void UnlockDiaryEntry()
        {
            PlayerPrefs.SetInt(DiaryKey, DiaryEntries + 1);
            PlayerPrefs.Save();
        }

        public static void AddBlueprint()
        {
            UnlockBlueprint();
        }

        public static void AddDiaryEntry()
        {
            UnlockDiaryEntry();
        }

        public static bool UnlockGarden()
        {
            if (GardenUnlocked || !TrySpendCrystals(3)) return false;
            PlayerPrefs.SetInt(GardenKey, 1);
            PlayerPrefs.Save();
            return true;
        }

        public static bool UnlockWorkshop()
        {
            if (WorkshopUnlocked || !TrySpendCrystals(5)) return false;
            PlayerPrefs.SetInt(WorkshopKey, 1);
            PlayerPrefs.Save();
            return true;
        }

        public static bool UnlockModulePack()
        {
            if (ModulePackUnlocked || Blueprints < 1) return false;
            PlayerPrefs.SetInt(BlueprintsKey, Blueprints - 1);
            PlayerPrefs.SetInt(ModulePackKey, 1);
            PlayerPrefs.Save();
            return true;
        }

        private static bool TrySpendCrystals(int amount)
        {
            if (amount < 0 || Crystals < amount) return false;
            PlayerPrefs.SetInt(CrystalsKey, Crystals - amount);
            PlayerPrefs.Save();
            return true;
        }
    }
}
