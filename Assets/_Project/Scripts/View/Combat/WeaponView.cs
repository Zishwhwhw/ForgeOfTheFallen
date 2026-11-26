using System.Collections.Generic;
using FotF.Data;
using FotF.Logic;
using FotF.Logic.Combat;
using UnityEngine;

namespace FotF.View.Combat
{
    /// <summary>
    /// Оружие в руках: вращается за прицелом на 360° (GDD §5.5), показывает спрайт, исполняет атаку
    /// (взмах или снаряд), когда Logic разрешил выстрел.
    /// </summary>
    public sealed class WeaponView : MonoBehaviour
    {
        [SerializeField] SpriteRenderer weaponRenderer;
        [SerializeField] Transform muzzle;
        [SerializeField] ProjectileView projectilePrefab;
        [SerializeField] SpriteRenderer swingEffect;
        [SerializeField] float swingEffectSeconds = 0.08f;
        [SerializeField] LayerMask hittableMask = ~0;
        [Tooltip("Отступ оружия от центра героя, юниты")]
        [SerializeField] float holdDistance = 0.45f;

        readonly List<Collider2D> _overlaps = new List<Collider2D>();
        readonly HashSet<IDamageable> _hitThisSwing = new HashSet<IDamageable>();
        WeaponDefinition _weapon;
        float _swingFxLeft;

        public void Show(WeaponDefinition weapon)
        {
            _weapon = weapon;
            if (!weaponRenderer) return;
            weaponRenderer.sprite = weapon ? weapon.sprite : null;
            // спрайт лежит на дочернем объекте: доворачиваем его так, чтобы ствол/клинок смотрел вдоль +X пивота
            weaponRenderer.transform.localRotation = Quaternion.Euler(0, 0, weapon ? -weapon.spriteForwardAngle : 0f);
        }

        /// <summary>Повернуть за прицелом. aimDir — нормализованный вектор от героя.</summary>
        public void Aim(Vector2 aimDir)
        {
            float angle = Mathf.Atan2(aimDir.y, aimDir.x) * Mathf.Rad2Deg;
            transform.localRotation = Quaternion.Euler(0, 0, angle);
            transform.localPosition = (Vector3)(aimDir * holdDistance);
            // зеркалим пивот по его оси Y, чтобы оружие слева не висело вверх ногами (работает при любом угле спрайта)
            transform.localScale = new Vector3(1f, aimDir.x < 0f ? -1f : 1f, 1f);
            if (weaponRenderer) weaponRenderer.sortingOrder = aimDir.y > 0.3f ? -1 : 1; // при прицеле вверх — за героем
        }

        /// <summary>Атака уже разрешена Logic (кулдаун, мана). Здесь только исполнение.</summary>
        public void Attack(Vector2 aimDir, Team team, IRandom rng)
        {
            if (_weapon == null) return;
            if (_weapon.attackType == AttackType.Melee) Swing(aimDir, team, rng);
            else Shoot(aimDir, team, rng);
        }

        void Shoot(Vector2 aimDir, Team team, IRandom rng)
        {
            if (projectilePrefab == null) return;
            Vector3 origin = muzzle ? muzzle.position : transform.position;
            int count = Mathf.Max(1, _weapon.projectilesPerShot);
            for (int i = 0; i < count; i++)
            {
                float spread = count > 1
                    ? Mathf.Lerp(-_weapon.spreadDegrees * 0.5f, _weapon.spreadDegrees * 0.5f, i / (float)(count - 1))
                    : (rng.NextFloat() - 0.5f) * _weapon.spreadDegrees;
                Vector2 dir = Quaternion.Euler(0, 0, spread) * aimDir;
                var roll = WeaponAttack.RollDamage(_weapon.damage, _weapon.critChance, rng);
                var p = Instantiate(projectilePrefab, origin, Quaternion.identity);
                p.Launch(dir, _weapon.projectileSpeed, _weapon.projectileLifetime,
                    new HitInfo(roll.Amount, roll.IsCrit, dir, _weapon.stunSeconds, team));
            }
        }

        void Swing(Vector2 aimDir, Team team, IRandom rng)
        {
            _hitThisSwing.Clear();
            var filter = new ContactFilter2D { useTriggers = true, useLayerMask = true, layerMask = hittableMask };
            Physics2D.OverlapCircle(transform.parent.position, _weapon.meleeRadius, filter, _overlaps);
            float half = _weapon.meleeArcDegrees * 0.5f;
            foreach (var col in _overlaps)
            {
                Vector2 to = (Vector2)col.bounds.center - (Vector2)transform.parent.position;
                if (Vector2.Angle(aimDir, to) > half) continue;
                if (_weapon.cutsProjectiles && col.TryGetComponent(out ProjectileView projectile)) { Destroy(projectile.gameObject); continue; }
                var target = col.GetComponentInParent<IDamageable>();
                if (target == null || target.Team == team || !_hitThisSwing.Add(target)) continue;
                var roll = WeaponAttack.RollDamage(_weapon.damage, _weapon.critChance, rng);
                target.TakeHit(new HitInfo(roll.Amount, roll.IsCrit, to.normalized, _weapon.stunSeconds, team));
            }
            if (swingEffect) { swingEffect.enabled = true; _swingFxLeft = swingEffectSeconds; }
        }

        void Update()
        {
            if (_swingFxLeft > 0f && (_swingFxLeft -= Time.deltaTime) <= 0f && swingEffect) swingEffect.enabled = false;
        }

        void OnDrawGizmosSelected()
        {
            if (_weapon == null || _weapon.attackType != AttackType.Melee || transform.parent == null) return;
            Gizmos.color = new Color(1f, 0.6f, 0.2f, 0.4f);
            Gizmos.DrawWireSphere(transform.parent.position, _weapon.meleeRadius);
        }
    }
}
