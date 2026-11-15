using UnityEngine;

namespace FrankenArms
{
    [CreateAssetMenu(menuName = "FrankenArms/Dungeon Route")]
    public sealed class DungeonRouteDefinition : ScriptableObject
    {
        public string routeName = "Main Route";
        [Tooltip("Ordered authored nodes. Index 0 is floor 1; no random walk is used.")]
        public RoomNodeDefinition[] nodes;

        public RoomNodeDefinition GetNode(int floor)
        {
            if (nodes == null || nodes.Length == 0) return null;
            var index = Mathf.Clamp(floor - 1, 0, nodes.Length - 1);
            return nodes[index];
        }
    }
}
