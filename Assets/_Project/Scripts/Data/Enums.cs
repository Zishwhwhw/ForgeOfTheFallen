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
}
