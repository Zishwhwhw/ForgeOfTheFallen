using UnityEngine;

/// <summary>Редкость оружия/предметов (как в Soul Knight: белое → зелёное → синее → фиолетовое → оранжевое).</summary>
public enum Rarity { Common, Uncommon, Rare, Epic, Legendary }

/// <summary>Единые цвета редкостей для UI, подсветки пикапов и сундуков.</summary>
public static class RarityColors
{
    public static readonly Color Common = new Color(0.72f, 0.72f, 0.72f);
    public static readonly Color Uncommon = new Color(0.30f, 0.80f, 0.31f);
    public static readonly Color Rare = new Color(0.24f, 0.55f, 1.00f);
    public static readonly Color Epic = new Color(0.71f, 0.33f, 0.94f);
    public static readonly Color Legendary = new Color(1.00f, 0.62f, 0.17f);

    public static Color Of(Rarity rarity) => rarity switch
    {
        Rarity.Common => Common,
        Rarity.Uncommon => Uncommon,
        Rarity.Rare => Rare,
        Rarity.Epic => Epic,
        Rarity.Legendary => Legendary,
        _ => Common
    };

    /// <summary>Порядковый номер редкости для сравнений (больше = реже).</summary>
    public static int Rank(Rarity rarity) => (int)rarity;
}
