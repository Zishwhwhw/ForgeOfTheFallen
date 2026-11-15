using UnityEngine;

namespace FrankenArms
{
    /// <summary>Safety budget for the vertical slice's transient projectile count.</summary>
    public static class ProjectileBudget
    {
        public const int MaxActive = 100;
        private static int active;

        public static bool TryAcquire()
        {
            if (active >= MaxActive) return false;
            active++;
            return true;
        }

        public static void Release() { active = Mathf.Max(0, active - 1); }
        public static int Active { get { return active; } }
    }
}
