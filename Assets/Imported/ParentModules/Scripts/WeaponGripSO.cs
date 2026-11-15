using UnityEngine;

[CreateAssetMenu(fileName = "NewGrip", menuName = "ModularWeapon/Grip (Base)")]
public class WeaponGripSO : ScriptableObject
{
    public string gripName = "Standard Grip";
    [TextArea(2, 3)] public string description = "";
    [TextArea(1, 2)] public string flavorText = "";
    public Rarity rarity = Rarity.Common;
    public Color gripColor = Color.gray;
    public Sprite icon;
    [Tooltip("Спрайт в руках; пивот = точка хвата")]
    public Sprite heldSprite;
    [Tooltip("Куда крепится ствол относительно хвата, в юнитах")]
    public Vector2 barrelSocket = new Vector2(0.25f, 0.06f);
    [Tooltip("Точка второй руки относительно хвата (для двуручных стволов)")]
    public Vector2 offHandPoint = new Vector2(0.4f, -0.02f);

    [Header("Fire Rate & Multiplier")]
    public float fireRate = 0.25f;
    public float damageMultiplier = 1f;
    public float knockbackForce = 3f;

    [Header("Perks (множители игрока)")]
    [Tooltip("Бонус к скорости передвижения (0.1 = +10%)")]
    public float moveSpeedBonus;
    [Tooltip("Шанс критического выстрела x2 урона")]
    [Range(0f, 1f)] public float critChance;
}