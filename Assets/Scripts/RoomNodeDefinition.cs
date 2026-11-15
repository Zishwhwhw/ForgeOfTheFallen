using UnityEngine;

namespace FrankenArms
{
    public enum RoomNodeType
    {
        Combat,
        Forge,
        Reward,
        Secret,
        Boss,
        Shop,
        Shrine
    }

    [CreateAssetMenu(menuName = "FrankenArms/Route Room Node")]
    public sealed class RoomNodeDefinition : ScriptableObject
    {
        public int floor;
        public string displayName;
        public RoomNodeType nodeType = RoomNodeType.Combat;
        [Min(0)] public int threatBudget = 3;
        [Min(0)] public int rewardCredits = 10;
        public bool optional;
        [TextArea] public string routeHint;
    }
}
