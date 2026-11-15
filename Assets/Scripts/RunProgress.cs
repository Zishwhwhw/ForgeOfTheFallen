using UnityEngine;

namespace FrankenArms
{
    public static class RunProgress
    {
        private const string CreditsKey = "FrankenArms.Credits";
        private const string BestRoomKey = "FrankenArms.BestRoom";

        public static int Credits { get { return PlayerPrefs.GetInt(CreditsKey, 0); } }
        public static int BestRoom { get { return PlayerPrefs.GetInt(BestRoomKey, 0); } }
        private static string DailyKey(string suffix) { return "FrankenArms.Daily." + RunMode.DailySeed + "." + suffix; }
        public static int DailyBestRoom { get { return PlayerPrefs.GetInt(DailyKey("BestRoom"), 0); } }
        public static int DailyBestKills { get { return PlayerPrefs.GetInt(DailyKey("BestKills"), 0); } }

        public static void AddCredits(int amount)
        {
            PlayerPrefs.SetInt(CreditsKey, Credits + amount);
            PlayerPrefs.Save();
        }

        public static bool TrySpendCredits(int amount)
        {
            if (amount < 0 || Credits < amount) return false;
            PlayerPrefs.SetInt(CreditsKey, Credits - amount);
            PlayerPrefs.Save();
            return true;
        }

        public static void RecordRoomReached(int room)
        {
            if (room <= BestRoom) return;
            PlayerPrefs.SetInt(BestRoomKey, room);
            PlayerPrefs.Save();
        }

        public static void RecordDailyResult(int room, int kills)
        {
            if (!RunMode.Daily) return;
            if (room > DailyBestRoom) PlayerPrefs.SetInt(DailyKey("BestRoom"), room);
            if (kills > DailyBestKills) PlayerPrefs.SetInt(DailyKey("BestKills"), kills);
            PlayerPrefs.Save();
        }
    }
}
