using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using FotF.State.Save;
using Newtonsoft.Json.Linq;
using NUnit.Framework;

namespace FotF.Tests.Data
{
    /// <summary>Сейвы (ARCHITECTURE §8.4–8.5): версии, миграции, атомарная запись, .bak, неизвестные id, снимок схемы.</summary>
    public class SaveTests
    {
        const string Samples = "Assets/Tests/SaveSamples";
        string _dir;

        [SetUp] public void SetUp() { _dir = Path.Combine(Path.GetTempPath(), "fotf_save_tests_" + Guid.NewGuid().ToString("N")); Directory.CreateDirectory(_dir); }
        [TearDown] public void TearDown() { if (Directory.Exists(_dir)) Directory.Delete(_dir, true); }

        string P(string name) => Path.Combine(_dir, name);

        [Test]
        public void NoFile_CreatesNewDefaults()
        {
            var r = SaveMigrations.LoadProfile(P("profile.json"));
            Assert.AreEqual(LoadStatus.CreatedNew, r.Status);
            Assert.AreEqual(ProfileState.CurrentVersion, r.Data.saveVersion);
            Assert.IsFalse(r.PlayerShouldBeNotified);
        }

        [Test]
        public void WriteThenRead_RoundTrips()
        {
            var p = new ProfileState { crystals = 7 };
            p.unlockedHeroes.Add("hero.mage");
            p.fusionDiary.Add(new FusionRecord { parentA = "weapon.a", parentB = "weapon.b", timesForged = 1 });
            p.stats.kills = 12;
            SaveFile.Write(P("profile.json"), p);

            var r = SaveMigrations.LoadProfile(P("profile.json"));
            Assert.AreEqual(LoadStatus.Loaded, r.Status);
            Assert.AreEqual(7, r.Data.crystals);
            CollectionAssert.AreEqual(new[] { "hero.mage" }, r.Data.unlockedHeroes);
            Assert.AreEqual("weapon.b", r.Data.fusionDiary[0].parentB);
            Assert.AreEqual(12, r.Data.stats.kills);
            Assert.IsFalse(File.Exists(SaveFile.TempPath(P("profile.json"))), "tmp не должен оставаться");
        }

        [Test]
        public void SecondWrite_KeepsPreviousAsBak()
        {
            SaveFile.Write(P("settings.json"), new SettingsState { musicVolume = 0.3f });
            SaveFile.Write(P("settings.json"), new SettingsState { musicVolume = 0.9f });
            var bak = JObject.Parse(File.ReadAllText(SaveFile.BackupPath(P("settings.json"))));
            Assert.AreEqual(0.3f, (float)bak["musicVolume"], 1e-5f);
            Assert.AreEqual(0.9f, SaveMigrations.LoadSettings(P("settings.json")).Data.musicVolume, 1e-5f);
        }

        [Test]
        public void CorruptedMain_LoadsBak()
        {
            SaveFile.Write(P("profile.json"), new ProfileState { crystals = 5 });
            SaveFile.Write(P("profile.json"), new ProfileState { crystals = 9 });
            File.WriteAllText(P("profile.json"), "{ битый");
            var r = SaveMigrations.LoadProfile(P("profile.json"));
            Assert.AreEqual(LoadStatus.RestoredFromBackup, r.Status);
            Assert.AreEqual(5, r.Data.crystals);
        }

        [Test]
        public void MainAndBakCorrupted_NewProfile_AndPlayerIsNotified()
        {
            File.WriteAllText(P("profile.json"), "мусор");
            File.WriteAllText(SaveFile.BackupPath(P("profile.json")), "");
            var r = SaveMigrations.LoadProfile(P("profile.json"));
            Assert.AreEqual(LoadStatus.ResetAfterCorruption, r.Status);
            Assert.AreEqual(0, r.Data.crystals);
            Assert.IsTrue(r.PlayerShouldBeNotified);
        }

        [Test]
        public void OldFile_WithoutNewField_GetsDefault_WithoutMigration()
        {
            File.WriteAllText(P("settings.json"), "{\"saveVersion\":1,\"musicVolume\":0.4}");
            var r = SaveMigrations.LoadSettings(P("settings.json"));
            Assert.AreEqual(LoadStatus.Loaded, r.Status);
            Assert.AreEqual(0.4f, r.Data.musicVolume, 1e-5f);
            Assert.IsTrue(r.Data.screenShake, "отсутствующее поле — значение по умолчанию");
            Assert.AreEqual(1f, r.Data.gameFeel);
        }

        [Test]
        public void MigrationChain_AppliesInOrder_AndBumpsVersion()
        {
            var migrations = new Dictionary<int, SaveFile.Migration>
            {
                [1] = j => { j["crystals"] = (int)j["gems"]; j.Remove("gems"); return j; },   // v1 → v2: gems → crystals
                [2] = j => { j["crystals"] = (int)j["crystals"] * 10; return j; },          // v2 → v3: ×10
            };
            var p = SaveFile.Parse<ProfileState>("{\"saveVersion\":1,\"gems\":3}", 3, migrations, out bool migrated);
            Assert.IsTrue(migrated);
            Assert.AreEqual(30, p.crystals);
            Assert.AreEqual(3, p.saveVersion);
        }

        [Test]
        public void MissingMigration_FallsBackToBak()
        {
            File.WriteAllText(P("profile.json"), "{\"saveVersion\":1,\"crystals\":1}");
            File.WriteAllText(SaveFile.BackupPath(P("profile.json")), "{\"saveVersion\":2,\"crystals\":2}");
            var r = SaveFile.Read<ProfileState>(P("profile.json"), 2, new Dictionary<int, SaveFile.Migration>());
            Assert.AreEqual(LoadStatus.RestoredFromBackup, r.Status);
            Assert.AreEqual(2, r.Data.crystals);
        }

        [Test]
        public void UnknownIds_AreDropped_WithLog_AndDoNotCrash()
        {
            var p = SaveFile.Parse<ProfileState>(File.ReadAllText(Path.Combine(Samples, "profile_v1.json")), ProfileState.CurrentVersion, SaveMigrations.Profile, out _);
            var known = new HashSet<string> { "weapon.eternal_sword" };                  // маг и гвоздомёт «убраны из игры»
            var log = new List<string>();
            p.DropUnknownIds(known.Contains, log);
            Assert.IsEmpty(p.unlockedHeroes);
            Assert.IsEmpty(p.fusionDiary);
            Assert.AreEqual(2, log.Count);
            Assert.AreEqual(42, p.crystals, "остальное не тронуто");
        }

        [TestCase("profile_v1.json")]
        [TestCase("settings_v1.json")]
        public void Samples_LoadWithCurrentCode(string sample)
        {
            string text = File.ReadAllText(Path.Combine(Samples, sample));
            if (sample.StartsWith("profile"))
            {
                var p = SaveFile.Parse<ProfileState>(text, ProfileState.CurrentVersion, SaveMigrations.Profile, out _);
                Assert.AreEqual(42, p.crystals); Assert.AreEqual(230, p.stats.kills);
            }
            else
            {
                var s = SaveFile.Parse<SettingsState>(text, SettingsState.CurrentVersion, SaveMigrations.Settings, out _);
                Assert.AreEqual("ru", s.language); Assert.IsFalse(s.screenShake);
            }
        }

        [Test]
        public void Settings_Sanitize_ClampsHandEditedValues()
        {
            File.WriteAllText(P("settings.json"), "{\"saveVersion\":1,\"masterVolume\":5,\"sfxVolume\":-1,\"language\":null}");
            var s = SaveMigrations.LoadSettings(P("settings.json")).Data;
            Assert.AreEqual(1f, s.masterVolume); Assert.AreEqual(0f, s.sfxVolume); Assert.AreEqual("", s.language);
        }

        /// <summary>
        /// ARCHITECTURE §8.5: поля State-моделей сейва = снимок. Добавили поле — обновите снимок (миграция не нужна).
        /// Переименовали/удалили/сменили тип — saveVersion + 1, Migrate_N_to_N+1, образец старого сейва, затем снимок.
        /// </summary>
        [Test]
        public void SaveSchemaSnapshot_MatchesCode()
        {
            string snapshotPath = Path.Combine(Samples, "save_schema_snapshot.txt");
            var current = Describe(typeof(ProfileState), typeof(SettingsState));
            var snapshot = File.ReadAllLines(snapshotPath).Where(l => l.Length > 0 && !l.StartsWith("#")).ToList();
            var removed = snapshot.Except(current).ToList();
            var added = current.Except(snapshot).ToList();
            Assert.IsEmpty(removed, "поля сейва удалены/переименованы/сменили тип — нужна миграция и saveVersion + 1:\n" + string.Join("\n", removed) +
                                    "\nновые строки:\n" + string.Join("\n", added));
            Assert.IsEmpty(added, $"новые поля сейва — допишите их в {snapshotPath} (миграция не нужна):\n" + string.Join("\n", added));
        }

        static List<string> Describe(params Type[] roots)
        {
            var lines = new List<string>();
            var seen = new HashSet<Type>();
            var queue = new Queue<Type>(roots);
            while (queue.Count > 0)
            {
                var t = queue.Dequeue();
                if (!seen.Add(t)) continue;
                if (roots.Contains(t))
                    lines.Add($"{t.Name}.CurrentVersion={t.GetField("CurrentVersion").GetValue(null)}");
                foreach (var f in t.GetFields(BindingFlags.Public | BindingFlags.Instance))
                {
                    lines.Add($"{t.Name}.{f.Name}:{TypeName(f.FieldType)}");
                    var inner = f.FieldType.IsGenericType ? f.FieldType.GetGenericArguments()[0] : f.FieldType;
                    if (inner.Namespace == typeof(ProfileState).Namespace) queue.Enqueue(inner);
                }
            }
            return lines;
        }

        static string TypeName(Type t) => t.IsGenericType
            ? $"{t.Name.Split('`')[0]}<{string.Join(",", t.GetGenericArguments().Select(TypeName))}>"
            : t.Name;
    }
}
