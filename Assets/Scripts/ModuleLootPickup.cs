using UnityEngine;

namespace FrankenArms
{
    /// <summary>Room loot adapter that equips one imported Grip + Barrel assembly.</summary>
    public sealed class ModuleLootPickup : MonoBehaviour
    {
        private bool rare;
        private WeaponGripSO[] grips;
        private WeaponBarrelSO[] barrels;

        internal void Initialize(bool rareDrop)
        {
            rare = rareDrop;
            grips = Resources.LoadAll<WeaponGripSO>("ImportedModules");
            barrels = Resources.LoadAll<WeaponBarrelSO>("ImportedModules");
            var renderer = gameObject.AddComponent<SpriteRenderer>();
            renderer.sprite = RuntimeSpriteFactory.CreateDoor();
            renderer.color = rare ? new Color(0.75f, 0.35f, 1f, 1f) : new Color(0.25f, 0.8f, 1f, 1f);
            renderer.sortingOrder = 12;
            var collider = gameObject.AddComponent<CircleCollider2D>();
            collider.isTrigger = true;
            collider.radius = 0.3f;
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (!other.CompareTag("Player") && other.GetComponent<PlayerMovement>() == null) return;
            var combat = other.GetComponent<PlayerCombat>();
            if (combat == null || grips == null || barrels == null || grips.Length == 0 || barrels.Length == 0) return;

            var gripCount = MetaProgression.ModulePackUnlocked ? grips.Length : Mathf.Min(1, grips.Length);
            var barrelCount = MetaProgression.ModulePackUnlocked ? barrels.Length : Mathf.Min(1, barrels.Length);
            var grip = grips[Random.Range(0, gripCount)];
            var barrel = barrels[Random.Range(0, barrelCount)];
            var damage = Mathf.Max(1, Mathf.RoundToInt(barrel.baseDamage * grip.damageMultiplier * (rare ? 1.2f : 1f)));
            combat.ApplyFusion(damage, 1f / Mathf.Max(0.05f, grip.fireRate), barrel.projectileSpeed);
            var renderer = other.GetComponent<SpriteRenderer>();
            if (renderer != null) renderer.color = Color.Lerp(grip.gripColor, barrel.barrelColor, 0.5f);
            GameAudio.Play("Bonus", 0.8f);
            Debug.Log("Module cache equipped: " + grip.gripName + " + " + barrel.barrelName);
            Destroy(gameObject);
        }
    }
}
