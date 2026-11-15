using UnityEngine;

// Rarity перенесён в Weapons/RarityColors.cs (5 уровней, как в Soul Knight).
// Модульная система (Grip/Barrel) выведена из игрового цикла — см. план, редакция 4.

[CreateAssetMenu(fileName = "NewBarrel", menuName = "ModularWeapon/Barrel or Blade (Attachment)")]
public class WeaponBarrelSO : ScriptableObject
{
    public string barrelName = "Standard Barrel";
    [TextArea(2, 3)] public string description = "";
    [TextArea(1, 2)] public string flavorText = "";
    public Rarity rarity = Rarity.Common;
    public Color barrelColor = Color.yellow;
    public Sprite icon;
    [Tooltip("Спрайт ствола/лезвия; пивот = точка крепления к рукояти")]
    public Sprite heldSprite;
    [Tooltip("Дуло/острие относительно точки крепления, в юнитах")]
    public Vector2 muzzlePoint = new Vector2(0.4f, 0f);
    [Tooltip("Держится двумя руками (дробовик, двуручный меч, винтовка)")]
    public bool twoHanded;
    public GameObject projectilePrefab;

    [Header("Ballistics / Stats")]
    public float baseDamage = 10f;
    public float projectileSpeed = 16f;
    public int bulletsPerShot = 1;
    public float spreadAngle = 3f;
    public ElementType element = ElementType.Physical;
    public int energyCost = 1;

    [Header("Blade / Melee Hybrid ('Оружейный Франкенштейн')")]
    [Tooltip("Если включено — модуль действует как меч/клинок: взмах вблизи, сбивание пуль")]
    public bool isBlade = false;
    public float meleeRange = 1.6f;
    public float meleeArc = 120f;
    [Tooltip("Уничтожать или отражать вражеские пули при взмахе")]
    public bool deflectsBullets = true;
    [Tooltip("Выпускать энергетическую волну/пули при взмахе мечом")]
    public bool shootsWaveOnSwing = true;

    [Header("Gamble Mode")]
    public bool isGambleBarrel = false;
    [Tooltip("Шанс джекпота при выстреле (если включён азартный режим)")]
    [Range(0f, 1f)] public float jackpotChance = 1f / 6f;
    [Tooltip("Шанс осечки")]
    [Range(0f, 1f)] public float misfireChance = 1f / 6f;

    public static Color RarityColor(Rarity r) => r switch
    {
        Rarity.Rare => new Color(0.3f, 0.6f, 1f),
        Rarity.Epic => new Color(0.7f, 0.35f, 1f),
        Rarity.Legendary => new Color(1f, 0.65f, 0.1f),
        _ => new Color(0.8f, 0.8f, 0.8f)
    };

    public static Color ElementColor(ElementType e) => e switch
    {
        ElementType.Fire => new Color(1f, 0.45f, 0.1f),
        ElementType.Ice => new Color(0.55f, 0.9f, 1f),
        ElementType.Electric => new Color(1f, 0.95f, 0.3f),
        ElementType.Acid => new Color(0.5f, 1f, 0.3f),
        _ => new Color(1f, 0.85f, 0.4f)
    };
}
