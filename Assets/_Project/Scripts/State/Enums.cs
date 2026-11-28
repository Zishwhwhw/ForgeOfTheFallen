namespace FotF.Data
{
    /// <summary>Редкость оружия (GDD §5.2). Franken — только сплавы.</summary>
    public enum Rarity { Common, Rare, Epic, Legendary, Franken }

    /// <summary>Стихии (GDD §5.4). В срезе используются Fire, Ice, Lightning, Poison.</summary>
    public enum Element { Physical, Fire, Ice, Lightning, Poison, Rust, Dark, Holy, Blood }

    /// <summary>Тип атаки оружия.</summary>
    public enum AttackType { Melee, Projectile }

    /// <summary>Семейство оружия (GDD §7.1).</summary>
    public enum WeaponFamily { Sword, Sledgehammer, Pistol, Shotgun, Rifle, Bow, Crossbow, Staff }

    /// <summary>Тип хвата оружия (Art Bible §5.1).</summary>
    public enum HoldType { OneHandMelee, TwoHandHeavy, Pistol, TwoHandGun, Staff, Bow }

    /// <summary>Класс размера оружия: S ≤ 1/3 роста, M ≤ 3/4, L ≤ роста (Art Bible §5.1).</summary>
    public enum SizeClass { S, M, L }

    /// <summary>Ранг врага (GDD §7.4–7.6).</summary>
    public enum EnemyTier { Normal, Elite, MiniBoss }

    /// <summary>Как статус ведёт себя при повторном наложении (GDD §7.3).</summary>
    public enum Stacking { None, RefreshTimer, Accumulate }

    /// <summary>Форма атаки босса (GDD §7.7).</summary>
    public enum AttackShape { Circle, Ring, Fan, Line }
}
