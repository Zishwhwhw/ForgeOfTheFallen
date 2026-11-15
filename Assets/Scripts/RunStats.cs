using UnityEngine;

namespace FrankenArms
{
    /// <summary>Ephemeral statistics for the current run result screen.</summary>
    public static class RunStats
    {
        public static float StartedAt { get; private set; }
        public static int Kills { get; private set; }
        public static int CreditsCollected { get; private set; }
        public static int MetaCollected { get; private set; }
        public static int AbilitiesUsed { get; private set; }
        public static float ElapsedSeconds { get { return Mathf.Max(0f, Time.time - StartedAt); } }

        public static void BeginRun()
        {
            StartedAt = Time.time;
            Kills = 0;
            CreditsCollected = 0;
            MetaCollected = 0;
            AbilitiesUsed = 0;
            if (RunMode.Daily)
                Random.InitState(RunMode.DailySeed);
        }

        public static void RecordKill() { Kills++; }
        public static void RecordCredits(int amount) { CreditsCollected += Mathf.Max(0, amount); }
        public static void RecordMeta() { MetaCollected++; }
        public static void RecordAbility() { AbilitiesUsed++; }
    }
}
