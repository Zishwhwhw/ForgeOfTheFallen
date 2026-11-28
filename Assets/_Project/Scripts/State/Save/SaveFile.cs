using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Linq;

namespace FotF.State.Save
{
    public enum LoadStatus
    {
        /// <summary>Файл прочитан как есть.</summary>
        Loaded,
        /// <summary>Файл старой версии, применены миграции.</summary>
        Migrated,
        /// <summary>Основной файл битый — загружен file.bak.</summary>
        RestoredFromBackup,
        /// <summary>Файла нет — новый (первый запуск).</summary>
        CreatedNew,
        /// <summary>Основной и резервный файлы битые — новый; игроку показать сообщение.</summary>
        ResetAfterCorruption,
    }

    public sealed class LoadResult<T>
    {
        public T Data;
        public LoadStatus Status;
        /// <summary>Подробности для лога (не для игрока).</summary>
        public string Detail;
        public bool PlayerShouldBeNotified => Status == LoadStatus.ResetAfterCorruption;
    }

    /// <summary>
    /// JSON-сейв (ARCHITECTURE §8.4): {"saveVersion": N, ...}; старая версия → цепочка Migrate_N_to_N+1;
    /// запись атомарно: file.tmp → проверка чтения → замена, прежний файл — file.bak; битый файл → .bak, иначе новый.
    /// Новые поля State получают значения по умолчанию без миграции.
    /// </summary>
    public static class SaveFile
    {
        /// <summary>Миграция JSON версии N → N+1 (ключ словаря — N).</summary>
        public delegate JObject Migration(JObject json);

        public static readonly JsonSerializerSettings Settings = new JsonSerializerSettings
        {
            Formatting = Formatting.Indented,
            MissingMemberHandling = MissingMemberHandling.Ignore,
            ObjectCreationHandling = ObjectCreationHandling.Replace,
            Converters = { new StringEnumConverter() },
        };

        static readonly Encoding Utf8 = new UTF8Encoding(false);

        public static string BackupPath(string path) => path + ".bak";
        public static string TempPath(string path) => path + ".tmp";

        public static void Write<T>(string path, T data)
        {
            Directory.CreateDirectory(Path.GetDirectoryName(Path.GetFullPath(path))!);
            string json = JsonConvert.SerializeObject(data, Settings);
            string tmp = TempPath(path);
            File.WriteAllText(tmp, json, Utf8);
            JsonConvert.DeserializeObject<T>(File.ReadAllText(tmp, Utf8), Settings);   // проверка чтения до замены
            if (File.Exists(path)) File.Replace(tmp, path, BackupPath(path));
            else File.Move(tmp, path);
        }

        public static LoadResult<T> Read<T>(string path, int currentVersion, IReadOnlyDictionary<int, Migration> migrations = null)
            where T : new()
        {
            if (!File.Exists(path) && !File.Exists(BackupPath(path)))
                return new LoadResult<T> { Data = new T(), Status = LoadStatus.CreatedNew };

            string mainError;
            try
            {
                var data = Parse<T>(File.ReadAllText(path, Utf8), currentVersion, migrations, out bool migrated);
                return new LoadResult<T> { Data = data, Status = migrated ? LoadStatus.Migrated : LoadStatus.Loaded };
            }
            catch (Exception e) { mainError = e.Message; }

            try
            {
                var data = Parse<T>(File.ReadAllText(BackupPath(path), Utf8), currentVersion, migrations, out _);
                return new LoadResult<T> { Data = data, Status = LoadStatus.RestoredFromBackup, Detail = $"{Path.GetFileName(path)}: {mainError}" };
            }
            catch (Exception e)
            {
                return new LoadResult<T> { Data = new T(), Status = LoadStatus.ResetAfterCorruption, Detail = $"{Path.GetFileName(path)}: {mainError}; .bak: {e.Message}" };
            }
        }

        /// <summary>Разбор с миграциями. Бросает исключение, если JSON битый или нет нужной миграции.</summary>
        public static T Parse<T>(string text, int currentVersion, IReadOnlyDictionary<int, Migration> migrations, out bool migrated)
        {
            var json = JObject.Parse(text);
            int version = json.Value<int?>("saveVersion") ?? 1;
            migrated = false;
            for (; version < currentVersion; version++)
            {
                if (migrations == null || !migrations.TryGetValue(version, out var migrate))
                    throw new InvalidDataException($"нет миграции saveVersion {version} → {version + 1}");
                json = migrate(json) ?? throw new InvalidDataException($"миграция {version} → {version + 1} вернула null");
                migrated = true;
            }
            if (migrated) json["saveVersion"] = currentVersion;
            var data = json.ToObject<T>(JsonSerializer.Create(Settings));
            if (data == null) throw new InvalidDataException("пустой сейв");
            return data;
        }
    }

    /// <summary>Миграции сейвов. Пока версии 1 — пусто; добавлять Migrate_N_to_N+1 сюда (ARCHITECTURE §8.5).</summary>
    public static class SaveMigrations
    {
        public static readonly IReadOnlyDictionary<int, SaveFile.Migration> Profile = new Dictionary<int, SaveFile.Migration>();
        public static readonly IReadOnlyDictionary<int, SaveFile.Migration> Settings = new Dictionary<int, SaveFile.Migration>();

        public static LoadResult<ProfileState> LoadProfile(string path)
        {
            var r = SaveFile.Read<ProfileState>(path, ProfileState.CurrentVersion, Profile);
            r.Data.Sanitize();
            return r;
        }

        public static LoadResult<SettingsState> LoadSettings(string path)
        {
            var r = SaveFile.Read<SettingsState>(path, SettingsState.CurrentVersion, Settings);
            r.Data.Sanitize();
            return r;
        }
    }
}
