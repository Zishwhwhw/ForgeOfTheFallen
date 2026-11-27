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
        readonly HashSet<IDamageable> _hitThisSwing = new HashSet<IDamageable>();
        WeaponInstance _weapon;
        float _swingFxLeft;

        public WeaponCatalog Catalog => catalog;

        public void Show(WeaponInstance weapon)
        {
            _weapon = weapon;
            var baseDef = weapon != null && catalog ? catalog.Find(weapon.DefinitionId) : null;
            var detailDef = weapon != null && weapon.IsHybrid && catalog ? catalog.Find(weapon.DetailId) : null;
            if (weaponRenderer)
            {
                weaponRenderer.sprite = baseDef ? baseDef.sprite : null;
                weaponRenderer.color = weapon != null && weapon.IsHybrid ? frankenTint : Color.white;
                weaponRenderer.transform.localRotation = Quaternion.Euler(0, 0, baseDef ? -baseDef.spriteForwardAngle : 0f);
            }
            if (detailRenderer)
            {
                detailRenderer.sprite = detailDef ? detailDef.sprite : null;
                detailRenderer.enabled = detailDef != null;
                detailRenderer.transform.localRotation = Quaternion.Euler(0, 0, detailDef ? -detailDef.spriteForwardAngle : 0f);
            }
        }

        public void Aim(Vector2 aimDir)
        {
            float angle = Mathf.Atan2(aimDir.y, aimDir.x) * Mathf.Rad2Deg;
            transform.localRotation = Quaternion.Euler(0, 0, angle);
            transform.localPosition = (Vector3)(aimDir * holdDistance) + Vector3.up * _pivotHeight;
            transform.localScale = new Vector3(1f, aimDir.x < 0f ? -1f : 1f, 1f);       // слева оружие не вверх ногами
            int order = aimDir.y > 0.3f ? -1 : 1;                                          // при прицеле вверх — за героем
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
            Vector3 origin = muzzle ? muzzle.position : transform.position;
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
