using System;
using System.Collections.Generic;

namespace FotF.State.Save
{
    /// <summary>
    /// profile.json (ARCHITECTURE §8.4): мета между забегами. Только состояние и id — без чисел баланса.
    /// </summary>
    [Serializable]
    public sealed class ProfileState
    {
        public const int CurrentVersion = 1;
        public const string FileName = "profile.json";

        public int saveVersion = CurrentVersion;
        /// <summary>Кристаллы — мета-валюта (GDD §9.3).</summary>
        public int crystals;
        /// <summary>id героев, открытых сверх стартовых (стартовые — HeroDefinition.unlockedFromStart).</summary>
        public List<string> unlockedHeroes = new List<string>();
        /// <summary>Дневник сплавов: какие пары уже сплавлялись.</summary>
        public List<FusionRecord> fusionDiary = new List<FusionRecord>();
        public ProfileStats stats = new ProfileStats();
        /// <summary>id открытого в убежище (GDD §9).</summary>
        public List<string> refugeUnlocks = new List<string>();

        /// <summary>
        /// Сущность убрали из игры, а её id остался в сейве (ARCHITECTURE §8.5): не падать, убрать запись и сообщить в лог.
        /// </summary>
        public void DropUnknownIds(Func<string, bool> isKnown, List<string> log)
        {
            Drop(unlockedHeroes, isKnown, log, "герой");
            Drop(refugeUnlocks, isKnown, log, "убежище");
            for (int i = fusionDiary.Count - 1; i >= 0; i--)
            {
                var r = fusionDiary[i];
                if (r != null && isKnown(r.parentA) && isKnown(r.parentB)) continue;
                log?.Add($"дневник сплавов: убрана пара {r?.parentA} + {r?.parentB} (нет такого оружия)");
                fusionDiary.RemoveAt(i);
            }
        }

        static void Drop(List<string> ids, Func<string, bool> isKnown, List<string> log, string what)
        {
            for (int i = ids.Count - 1; i >= 0; i--)
            {
                if (ids[i] != null && isKnown(ids[i])) continue;
                log?.Add($"{what}: убран неизвестный id {ids[i]}");
                ids.RemoveAt(i);
            }
        }

        public void Sanitize()
        {
            unlockedHeroes ??= new List<string>(); fusionDiary ??= new List<FusionRecord>();
            refugeUnlocks ??= new List<string>(); stats ??= new ProfileStats();
            if (crystals < 0) crystals = 0;
        }
    }

    [Serializable]
    public sealed class FusionRecord
    {
        public string parentA;
        public string parentB;
        public int timesForged;
        public bool jackpot;
    }

    [Serializable]
    public sealed class ProfileStats
    {
        public int runs;
        public int deaths;
        public int victories;
        public int kills;
        public int bestFloor;
        public double playSeconds;
    }
}
