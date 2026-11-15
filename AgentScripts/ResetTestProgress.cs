using UnityEngine;

public static class ResetTestProgress
{
    public static void Main()
    {
        PlayerPrefs.DeleteKey("FrankenArms.Credits");
        PlayerPrefs.DeleteKey("FrankenArms.BestRoom");
        PlayerPrefs.DeleteKey("FrankenArms.Crystals");
        PlayerPrefs.DeleteKey("FrankenArms.Blueprints");
        PlayerPrefs.DeleteKey("FrankenArms.DiaryEntries");
        PlayerPrefs.Save();
    }
}
