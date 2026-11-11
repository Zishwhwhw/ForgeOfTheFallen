using UnityEngine;

namespace FrankenArms
{
    public static class RunProgress
    {
        private const string CreditsKey = "FrankenArms.Credits";

        public static int Credits { get { return PlayerPrefs.GetInt(CreditsKey, 0); } }

        public static void AddCredits(int amount)
        {
            PlayerPrefs.SetInt(CreditsKey, Credits + amount);
            PlayerPrefs.Save();
        }
    }
}
