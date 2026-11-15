using System.IO;
using UnityEditor;
using UnityEngine;

public static class CreateHeroAssets
{
    public static void Main()
    {
        var folder = "Assets/Resources/Content/Heroes";
        Directory.CreateDirectory(folder);
        Create(folder, "Knight", "Knight", "Frontline", "High armor and HP; forgiving first run.", 10, 6, 70, 4.7f, 1, 5.0f, new Color(0.95f, 0.72f, 0.3f, 1f));
        Create(folder, "Mage", "Mage", "Chaos", "Fast base weapon and high mana for experimental fusions.", 6, 2, 130, 5.2f, 1, 5.6f, new Color(0.35f, 0.85f, 1f, 1f));
        Create(folder, "Mechanic", "Mechanic", "Utility", "Balanced frame with a stronger, slower starting shot.", 8, 4, 100, 5.0f, 2, 4.4f, new Color(1f, 0.55f, 0.25f, 1f));
        CreateExtra(folder, "Vampire", "Vampire", "Sustain", 8, 3, 90, 5.1f, 1, 5.1f, "Shadow leap", "Blood grab", "Bat swarm heals on takedown", new Color(0.85f, 0.2f, 0.45f, 1f));
        CreateExtra(folder, "Witch", "Witch", "Chaos", 7, 2, 120, 5.0f, 1, 5.4f, "Broom dash", "Three random hexes", "Cauldron amplifies status effects", new Color(0.7f, 0.35f, 0.95f, 1f));
        CreateExtra(folder, "Necromancer", "Necromancer", "Summon", 7, 2, 100, 4.8f, 1, 4.8f, "Bone roll", "Raise a skeleton", "Higher summon limit", new Color(0.55f, 0.75f, 0.85f, 1f));
        CreateExtra(folder, "Archer", "Archer", "Precision", 7, 2, 80, 5.4f, 1, 5.8f, "Backstep shot", "Piercing arrow", "Range critical bonus", new Color(0.35f, 0.9f, 0.45f, 1f));
        CreateExtra(folder, "Soldier", "Soldier", "Firepower", 9, 4, 80, 4.9f, 1, 6.0f, "Combat roll", "Fragment grenade", "Ten-second bullet storm", new Color(0.85f, 0.75f, 0.35f, 1f));
        CreateExtra(folder, "Druid", "Druid", "Survival", 9, 3, 100, 4.7f, 1, 4.7f, "Leaf slide", "Root snare", "Bear form; food is stronger", new Color(0.35f, 0.75f, 0.35f, 1f));
        CreateExtra(folder, "Elf", "Elf", "Precision", 7, 2, 110, 5.3f, 1, 5.5f, "Wind glide", "Mark weakness", "Ricochet moon circle", new Color(0.65f, 0.85f, 0.95f, 1f));
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh(ImportAssetOptions.ForceUpdate);
    }

    private static void Create(string folder, string fileName, string name, string role, string description, int hp, int armor, int mana, float speed, int damage, float rate, Color tint)
    {
        var path = folder + "/" + fileName + ".asset";
        var hero = AssetDatabase.LoadAssetAtPath<FrankenArms.HeroDefinition>(path);
        if (hero == null)
        {
            hero = ScriptableObject.CreateInstance<FrankenArms.HeroDefinition>();
            AssetDatabase.CreateAsset(hero, path);
        }
        hero.displayName = name;
        hero.role = role;
        hero.description = description;
        hero.maxHealth = hp;
        hero.maxArmor = armor;
        hero.maxMana = mana;
        hero.moveSpeed = speed;
        hero.baseDamage = damage;
        hero.baseShotsPerSecond = rate;
        hero.tint = tint;
        hero.dashAbility = "Shield rush";
        hero.activeAbility = "Counter block";
        hero.ultimateOrPassive = "Bastion: high starting armor";
        EditorUtility.SetDirty(hero);
    }

    private static void CreateExtra(string folder, string fileName, string name, string role, int hp, int armor, int mana, float speed, int damage, float rate, string dash, string active, string passive, Color tint)
    {
        Create(folder, fileName, name, role, "GDD roster definition; unlock and ability wiring follow the meta pass.", hp, armor, mana, speed, damage, rate, tint);
        var hero = AssetDatabase.LoadAssetAtPath<FrankenArms.HeroDefinition>(folder + "/" + fileName + ".asset");
        hero.dashAbility = dash;
        hero.activeAbility = active;
        hero.ultimateOrPassive = passive;
        EditorUtility.SetDirty(hero);
    }
}
