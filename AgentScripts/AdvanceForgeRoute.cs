using System.Reflection;
using UnityEngine;

public static class AdvanceForgeRoute
{
    public static void Main()
    {
        var enemies = Object.FindObjectsByType<FrankenArms.EnemyTarget>();
        for (var i = 0; i < enemies.Length; i++) Object.Destroy(enemies[i].gameObject);
        var room = Object.FindAnyObjectByType<FrankenArms.CombatRoom>();
        var cleared = typeof(FrankenArms.CombatRoom).GetField("cleared", BindingFlags.Instance | BindingFlags.NonPublic);
        cleared.SetValue(room, true);
        typeof(FrankenArms.CombatRoom).GetMethod("StartNextRoom", BindingFlags.Instance | BindingFlags.NonPublic, null, new[] { typeof(int) }, null).Invoke(room, new object[] { 3 });
    }
}
