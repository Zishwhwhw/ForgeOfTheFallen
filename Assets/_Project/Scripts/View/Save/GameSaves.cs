using System.IO;
using FotF.State.Save;
using UnityEngine;

namespace FotF.View.Save
{
    /// <summary>Сейвы в Application.persistentDataPath (ARCHITECTURE §8.4). Логика чтения/записи — FotF.State.Save.</summary>
    public static class GameSaves
    {
        public static string ProfilePath => Path.Combine(Application.persistentDataPath, ProfileState.FileName);
        public static string SettingsPath => Path.Combine(Application.persistentDataPath, SettingsState.FileName);

        public static LoadResult<ProfileState> LoadProfile() => Report(SaveMigrations.LoadProfile(ProfilePath));
        public static LoadResult<SettingsState> LoadSettings() => Report(SaveMigrations.LoadSettings(SettingsPath));

        public static void Save(ProfileState profile) => SaveFile.Write(ProfilePath, profile);
        public static void Save(SettingsState settings) => SaveFile.Write(SettingsPath, settings);

        static LoadResult<T> Report<T>(LoadResult<T> r)
        {
            if (r.Status == LoadStatus.RestoredFromBackup) Debug.LogWarning($"[Сейв] восстановлен из .bak — {r.Detail}");
            else if (r.Status == LoadStatus.ResetAfterCorruption) Debug.LogError($"[Сейв] повреждён, создан новый — {r.Detail}");
            return r;
        }
    }
}
