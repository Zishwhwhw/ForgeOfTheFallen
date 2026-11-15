using UnityEngine;

namespace FrankenArms
{
    [CreateAssetMenu(menuName = "FrankenArms/Weapon Fusion Recipe")]
    public sealed class WeaponFusionRecipe : ScriptableObject
    {
        public string displayName;
        public int damage = 1;
        public float shotsPerSecond = 5f;
        public float projectileSpeed = 9f;
        public Color tint = Color.white;
        public string role;
    }
}
