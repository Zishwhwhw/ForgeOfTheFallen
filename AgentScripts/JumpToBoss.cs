using System.Reflection;
using UnityEngine;

public static class JumpToBoss
{
    public static void Main()
    {
        var enemies = Object.FindObjectsByType<FrankenArms.EnemyTarget>();
        for (var i = 0; i < enemies.Length; i++) Object.Destroy(enemies[i].gameObject);

        var room = Object.FindAnyObjectByType<FrankenArms.CombatRoom>();
        var roomType = typeof(FrankenArms.CombatRoom);
        roomType.GetField("roomIndex", BindingFlags.Instance | BindingFlags.NonPublic).SetValue(room, 5);
        roomType.GetField("cleared", BindingFlags.Instance | BindingFlags.NonPublic).SetValue(room, true);
        roomType.GetField("started", BindingFlags.Instance | BindingFlags.NonPublic).SetValue(room, true);
        roomType.GetMethod("StartNextRoom", BindingFlags.Instance | BindingFlags.NonPublic).Invoke(room, null);
    }
}
