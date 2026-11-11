using UnityEngine;

namespace FrankenArms
{
    public enum WeaponPartKind
    {
        Grip,
        Barrel
    }

    public sealed class WeaponPartDefinition : ScriptableObject
    {
        public string displayName;
        public WeaponPartKind kind;
        public int damage = 1;
        public float shotsPerSecond = 5f;
        public float projectileSpeed = 9f;
        public Color tint = Color.white;
    }
}
