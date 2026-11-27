using System.Collections.Generic;
using FotF.Data;
using FotF.Logic;
using FotF.Logic.Combat;
using FotF.State;
using UnityEngine;

namespace FotF.View.Combat
{
    /// <summary>
    /// Оружие в руках: вращается за прицелом на 360° (GDD §5.5), показывает спрайт (у гибрида — база + деталь + оттенок,
    /// GDD §6.4), исполняет атаку (взмах или снаряд) по статам WeaponInstance, когда Logic разрешил выстрел.
    /// </summary>
    public sealed class WeaponView : MonoBehaviour
    {
        [SerializeField] WeaponCatalog catalog;
        [SerializeField] SpriteRenderer weaponRenderer;
        [Tooltip("Сменная деталь второго родителя у гибрида")]
        [SerializeField] SpriteRenderer detailRenderer;
        [SerializeField] Transform muzzle;
        [SerializeField] ProjectileView projectilePrefab;
        [SerializeField] SpriteRenderer swingEffect;
        [SerializeField] float swingEffectSeconds = 0.08f;
        [SerializeField] LayerMask hittableMask = ~0;
        [Tooltip("Отступ оружия от центра героя, юниты")]
        [SerializeField] float holdDistance = 0.45f;
        [Tooltip("Цвет редкости «Франкен» (Art Bible §3: #f77622)")]
        [SerializeField] Color frankenTint = new Color32(0xff, 0xc8, 0x9a, 0xff);

        readonly List<Collider2D> _overlaps = new List<Collider2D>();
        WeaponDefinition _baseDef, _detailDef;
        float _bobUnits;
        Vector2 _hand;
        bool _behindBody;
        bool _hasHand;
        readonly HashSet<IDamageable> _hitThisSwing = new HashSet<IDamageable>();
        WeaponInstance _weapon;
        float _swingFxLeft;

        public WeaponCatalog Catalog => catalog;

        public void Show(WeaponInstance weapon)
        {
            _weapon = weapon;
            var baseDef = weapon != null && catalog ? catalog.Find(weapon.DefinitionId) : null;
            var detailDef = weapon != null && weapon.IsHybrid && catalog ? catalog.Find(weapon.DetailId) : null;
            _baseDef = baseDef; _detailDef = detailDef;
            if (weaponRenderer)
            {
                weaponRenderer.sprite = baseDef ? baseDef.sprite : null;
                weaponRenderer.color = weapon != null && weapon.IsHybrid ? frankenTint : Color.white;
                PlaceByGrip(weaponRenderer.transform, baseDef);
            }
            if (detailRenderer)
            {
                detailRenderer.sprite = detailDef ? detailDef.sprite : null;
                detailRenderer.enabled = detailDef != null;
                PlaceByGrip(detailRenderer.transform, detailDef);
            }
        }

        /// <summary>Поворачивает спрайт по spriteForwardAngle и сдвигает так, чтобы точка хвата была в руке (начале этого объекта).</summary>
        static void PlaceByGrip(Transform t, WeaponDefinition def)
        {
            t.localScale = Vector3.one;
            if (t.TryGetComponent(out SpriteRenderer r)) r.flipY = false;
            if (HasBaked(def)) { t.localPosition = Vector3.zero; return; }   // хват — точка опоры запечённого кадра
            var rotation = Quaternion.Euler(0, 0, def ? -def.spriteForwardAngle : 0f);
            t.localRotation = rotation;
            t.localPosition = def && def.sprite && def.gripPixel != Vector2.zero
                ? rotation * -(Vector3)SpritePixelToLocal(def.sprite, def.gripPixel)
                : Vector3.zero;
        }

        /// <summary>Пиксель спрайта (от левого нижнего угла) → локальные координаты спрайта в юнитах.</summary>
        public static Vector2 SpritePixelToLocal(Sprite sprite, Vector2 pixel) =>
            (pixel + Vector2.one * 0.5f - sprite.pivot) / sprite.pixelsPerUnit;

        static bool HasBaked(WeaponDefinition def) => def && def.rotatedSprites != null && def.rotatedSprites.Length > 0;

        /// <summary>Мировая точка вылета снаряда: дуло оружия (с учётом хвата и поворота), иначе общая точка muzzle.</summary>
        public Vector3 MuzzleWorld
        {
            get
            {
                if (!_baseDef || !_baseDef.sprite || _baseDef.muzzlePixel == Vector2.zero) return muzzle ? muzzle.position : transform.position;
                Vector2 fromGrip = SpritePixelToLocal(_baseDef.sprite, _baseDef.muzzlePixel)
                                   - (_baseDef.gripPixel != Vector2.zero ? SpritePixelToLocal(_baseDef.sprite, _baseDef.gripPixel) : Vector2.zero);
                return transform.TransformPoint(Quaternion.Euler(0, 0, -_baseDef.spriteForwardAngle) * fromGrip);
            }
        }

        /// <summary>Индекс запечённого кадра под угол прицела (градусы, против часовой от +x).</summary>
        public static int BakedIndex(float aimDegrees, int count)
        {
            float step = 360f / count;
            int i = Mathf.RoundToInt(aimDegrees / step) % count;
            return i < 0 ? i + count : i;
        }

        /// <summary>
        /// Запечённый кадр: гасим поворот и зеркало опоры (спрайт ровно по осям, без «рваных» пикселей),
        /// берём кадр ближайшего угла; слева — зеркало по вертикали, как раньше (оружие не вверх ногами).
        /// </summary>
        void ApplyBaked(SpriteRenderer r, WeaponDefinition def, float angle, bool left)
        {
            if (!r || !HasBaked(def)) return;
            r.transform.localRotation = Quaternion.Euler(0, 0, left ? angle : -angle);
            r.transform.localScale = new Vector3(1f, left ? -1f : 1f, 1f);
            r.sprite = def.rotatedSprites[BakedIndex(left ? -angle : angle, def.rotatedSprites.Length)];
            r.flipY = left;
        }

        /// <summary>Покачивание тела на текущем кадре, пиксели: рука с оружием двигается вместе с телом.</summary>
        public void SetBob(int pixels, float pixelsPerUnit = 32f) => _bobUnits = pixels / pixelsPerUnit;

        /// <summary>Точка руки у тела (локально к герою, юниты) и рисовать ли оружие за телом (взгляд от камеры).</summary>
        public void SetHand(Vector2 localUnits, bool behindBody)
        {
            _hand = localUnits; _behindBody = behindBody; _hasHand = true;
        }

        public void Aim(Vector2 aimDir)
        {
            float angle = Mathf.Atan2(aimDir.y, aimDir.x) * Mathf.Rad2Deg;
            transform.localRotation = Quaternion.Euler(0, 0, angle);
            Vector2 anchor = _hasHand ? _hand : Vector2.up * _pivotHeight;
            transform.localPosition = (Vector3)(anchor + aimDir * holdDistance) + Vector3.up * _bobUnits;
            transform.localScale = new Vector3(1f, aimDir.x < 0f ? -1f : 1f, 1f);       // слева оружие не вверх ногами
            ApplyBaked(weaponRenderer, _baseDef, angle, aimDir.x < 0f);
            ApplyBaked(detailRenderer, _detailDef, angle, aimDir.x < 0f);
            bool behind = _hasHand ? _behindBody : aimDir.y > 0.3f;
            int order = behind ? -1 : 1;                                                   // спиной к камере — оружие за телом
            if (weaponRenderer) weaponRenderer.sortingOrder = order;
            if (detailRenderer) detailRenderer.sortingOrder = order + 1;
        }

        float _pivotHeight;
        void Awake() { _pivotHeight = transform.localPosition.y; }

        public void Attack(Vector2 aimDir, Team team, IRandom rng)
        {
            if (_weapon == null) return;
            if (_weapon.AttackType == AttackType.Melee) Swing(aimDir, team, rng);
            else Shoot(aimDir, team, rng);
        }

        void Shoot(Vector2 aimDir, Team team, IRandom rng)
        {
            if (projectilePrefab == null) return;
            Vector3 origin = MuzzleWorld;
            int count = Mathf.Max(1, _weapon.ProjectilesPerShot);
            for (int i = 0; i < count; i++)
            {
                float spread = count > 1
                    ? Mathf.Lerp(-_weapon.SpreadDegrees * 0.5f, _weapon.SpreadDegrees * 0.5f, i / (float)(count - 1))
                    : (rng.NextFloat() - 0.5f) * _weapon.SpreadDegrees;
                Vector2 dir = Quaternion.Euler(0, 0, spread) * aimDir;
                var roll = WeaponAttack.RollDamage(_weapon.Damage, _weapon.CritChance, rng);
                var p = Instantiate(projectilePrefab, origin, Quaternion.identity);
                p.Launch(dir, _weapon.ProjectileSpeed, _weapon.ProjectileLifetime, new HitInfo(roll.Amount, roll.IsCrit, dir, _weapon.StunSeconds, team));
            }
        }

        void Swing(Vector2 aimDir, Team team, IRandom rng)
        {
            _hitThisSwing.Clear();
            var filter = new ContactFilter2D { useTriggers = true, useLayerMask = true, layerMask = hittableMask };
            Vector2 center = transform.parent.position + Vector3.up * _pivotHeight;
            Physics2D.OverlapCircle(center, _weapon.MeleeRadius, filter, _overlaps);
            float half = _weapon.MeleeArcDegrees * 0.5f;
            foreach (var col in _overlaps)
            {
                Vector2 to = (Vector2)col.bounds.center - center;
                if (Vector2.Angle(aimDir, to) > half) continue;
                if (_weapon.CutsProjectiles && col.TryGetComponent(out ProjectileView projectile)) { Destroy(projectile.gameObject); continue; }
                var target = col.GetComponentInParent<IDamageable>();
                if (target == null || target.Team == team || !_hitThisSwing.Add(target)) continue;
                var roll = WeaponAttack.RollDamage(_weapon.Damage, _weapon.CritChance, rng);
                target.TakeHit(new HitInfo(roll.Amount, roll.IsCrit, to.normalized, _weapon.StunSeconds, team));
            }
            if (swingEffect) { swingEffect.enabled = true; _swingFxLeft = swingEffectSeconds; }
        }

        void Update()
        {
            if (_swingFxLeft > 0f && (_swingFxLeft -= Time.deltaTime) <= 0f && swingEffect) swingEffect.enabled = false;
            if (_weapon != null && _weapon.IsJackpot && weaponRenderer)
                weaponRenderer.color = Color.Lerp(new Color32(0xf7, 0x76, 0x22, 0xff), new Color32(0x2c, 0xe8, 0xf5, 0xff), Mathf.PingPong(Time.time, 1f)); // переливание идеального сплава
        }

        void OnDrawGizmosSelected()
        {
            if (_weapon == null || _weapon.AttackType != AttackType.Melee || transform.parent == null) return;
            Gizmos.color = new Color(1f, 0.6f, 0.2f, 0.4f);
            Gizmos.DrawWireSphere(transform.parent.position + Vector3.up * _pivotHeight, _weapon.MeleeRadius);
        }
    }
}
