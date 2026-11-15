using System.Reflection;
using FrankenArms;
using UnityEngine;

public static class SelectHeroForTest
{
    public static void Main()
    {
        var selector = Object.FindAnyObjectByType<HeroSelectionController>();
        if (selector == null) throw new System.Exception("HeroSelectionController not found");
        var choose = typeof(HeroSelectionController).GetMethod("Choose", BindingFlags.Instance | BindingFlags.NonPublic);
        choose.Invoke(selector, new object[] { 1 });
        Debug.Log("Test hero selected: Mage");
    }
}
