using UnityEngine;

public static class CheckGeneratedArt
{
    public static void Main()
    {
        var hero = Resources.Load<Sprite>("Art/Generated/hero-gunsmith-v1");
        var enemies = Resources.LoadAll<Sprite>("Art/Generated/enemy-roster-v1");
        var floor = Resources.Load<Texture2D>("Art/Generated/dungeon-floor-v1");
        Debug.Log(string.Format("Generated art check: hero={0}, enemySprites={1}, floor={2}", hero != null, enemies.Length, floor != null));
        for (var i = 0; i < enemies.Length; i++) Debug.Log("Enemy sprite name: " + enemies[i].name);
    }
}
