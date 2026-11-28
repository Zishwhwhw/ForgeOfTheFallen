using UnityEngine;

namespace FotF.Data
{
    /// <summary>
    /// Описание оружия (GDD §5, §7.1). Только данные: числа контента живут здесь (A2).
    /// Поля с пометкой «калибровать» не заданы в GDD и подбираются на плейтесте.
    /// </summary>
    [CreateAssetMenu(menuName = "FotF/Data/Weapon", fileName = "weapon_")]
    public sealed class WeaponDefinition : ScriptableObject
    {
        [Tooltip("Стабильный ID, например weapon.eternal_sword")]
        public string id;
        [Tooltip("Ключ локализации названия")]
        public string nameKey;
        [Tooltip("Название до подключения локализации (RU)")]
        public string displayName;
        public WeaponFamily family;
        public Rarity rarity;
        public Element element = Element.Physical;
        public AttackType attackType;

        [Header("Урон")]
        public int damage = 10;
        [Range(0f, 1f)] public float critChance = 0.05f;
        [Tooltip("Атак в секунду (калибровать)")]
        public float attacksPerSecond = 2f;
        [Tooltip("Расход маны за атаку (GDD §5.3: ближнее и стартовое — 0)")]
        public int manaCost;

        [Header("Снаряд (AttackType.Projectile)")]
        public int projectilesPerShot = 1;
        [Tooltip("Разброс в градусах (калибровать)")]
        public float spreadDegrees;
        [Tooltip("Скорость снаряда, юниты/с (калибровать)")]
        public float projectileSpeed = 14f;
        [Tooltip("Время жизни снаряда, с (калибровать)")]
        public float projectileLifetime = 1.2f;
        [Tooltip("Оглушение при попадании, с (Гвоздомёт: 0.5)")]
        public float stunSeconds;

        [Header("Ближний бой (AttackType.Melee)")]
        [Tooltip("Радиус взмаха, юниты (калибровать)")]
        public float meleeRadius = 1.3f;
        [Tooltip("Угол взмаха, градусы (калибровать)")]
        public float meleeArcDegrees = 120f;
        [Tooltip("Срезает вражеские пули во взмахе (Вечный меч)")]
        public bool cutsProjectiles;

        [Header("Статус и механика (GDD §5.4, §7.1)")]
        [Range(0f, 1f), Tooltip("Шанс наложить статус стихии (калибровать)")]
        public float statusChance;
        [Tooltip("Нестандартная механика: перегрев, натяжение, очередь, самонаводка…")]
        public SpecialMechanic special = new SpecialMechanic();

        [Header("Хват (Art Bible §5.1)")]
        public HoldType holdType;
        public SizeClass sizeClass = SizeClass.M;

        [Header("Вид")]
        public Sprite sprite;
        [Tooltip("Куда смотрит ствол/клинок на картинке, градусы (0 — вправо, 225 — вниз-влево)")]
        public float spriteForwardAngle;
        [Tooltip("Точка хвата в пикселях спрайта, от левого нижнего угла (Art Bible §5.1). (0,0) — держать за центр")]
        public Vector2 gripPixel;
        [Tooltip("Дуло / навершие в пикселях спрайта, от левого нижнего угла: отсюда вылетает снаряд. (0,0) — общая точка muzzle")]
        public Vector2 muzzlePixel;
        [Tooltip("Запечённые RotSprite-углы (кадр i смотрит на i·360/N°, хват в центре кадра). Пусто — сырой поворот. Tools/art/bake_weapon_rotations.py")]
        public Sprite[] rotatedSprites = System.Array.Empty<Sprite>();
    }
}
