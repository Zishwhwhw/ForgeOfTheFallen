namespace FotF.State
{
    /// <summary>Слот оружия в руках героя (runtime).</summary>
    public sealed class WeaponSlotState
    {
        public string WeaponId;
        /// <summary>Экземпляр в руке (статы для атаки; у гибрида — свои).</summary>
        public WeaponInstance Instance;
        /// <summary>Секунд до следующей атаки.</summary>
        public float CooldownLeft;
        public bool IsEmpty => string.IsNullOrEmpty(WeaponId);
    }

    /// <summary>Runtime-состояние героя в забеге (A3: отдельно от Data). Меняется только через Logic.</summary>
    public sealed class HeroState
    {
        public int Hp, MaxHp;
        public int Armor, MaxArmor;
        public int Mana, MaxMana;
        /// <summary>Секунд до начала восстановления брони.</summary>
        public float ArmorRegenDelayLeft;
        /// <summary>Накопитель дробной брони при восстановлении.</summary>
        public float ArmorRegenAccumulator;

        public int DashCharges, MaxDashCharges;
        /// <summary>Секунд до восстановления следующего заряда рывка.</summary>
        public float DashRechargeLeft;
        /// <summary>Секунд до конца текущего рывка (0 — не в рывке).</summary>
        public float DashTimeLeft;
        public float InvulnerableLeft;

        /// <summary>Золото забега (GDD §9.3: только внутри забега).</summary>
        public int Gold;

        public readonly WeaponSlotState[] Weapons = { new WeaponSlotState(), new WeaponSlotState() };
        public int ActiveSlot;

        public bool IsDead => Hp <= 0;
        public bool IsDashing => DashTimeLeft > 0f;
        public bool IsInvulnerable => InvulnerableLeft > 0f;
        public WeaponSlotState ActiveWeapon => Weapons[ActiveSlot];
    }
}
