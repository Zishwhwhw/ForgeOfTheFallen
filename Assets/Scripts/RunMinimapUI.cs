using UnityEngine;

namespace FrankenArms
{
    /// <summary>Compact authored-route minimap for the top-right safe area.</summary>
    public sealed class RunMinimapUI : MonoBehaviour
    {
        private DungeonRouteDefinition route;
        private CombatRoom room;

        private void Start()
        {
            route = Resources.Load<DungeonRouteDefinition>("Content/Routes/FrankenArmsMainRoute");
            room = FindAnyObjectByType<CombatRoom>();
        }

        private void OnGUI()
        {
            if (route == null || route.nodes == null || route.nodes.Length == 0 || room == null) return;
            const float width = 270f;
            const float height = 118f;
            var panel = new Rect(Screen.width - width - 18f, 70f, width, height);
            GUI.Box(panel, "ROUTE // 24 FLOORS");
            GUI.Label(new Rect(panel.x + 12f, panel.y + 24f, width - 24f, 18f),
                string.Format("CURRENT FLOOR  {0:00}   {1}", room.RoomIndex, room.RoomKind));

            for (var i = 0; i < Mathf.Min(24, route.nodes.Length); i++)
            {
                var x = panel.x + 14f + (i % 12) * 20f;
                var y = panel.y + 50f + (i / 12) * 25f;
                var floor = i + 1;
                var node = route.nodes[i];
                var color = floor == room.RoomIndex
                    ? new Color(1f, 0.75f, 0.2f)
                    : floor < room.RoomIndex ? NodeColor(node.nodeType) : new Color(0.28f, 0.28f, 0.34f);
                var oldColor = GUI.color;
                GUI.color = color;
                GUI.Box(new Rect(x, y, 17f, 17f), floor.ToString("00"));
                GUI.color = oldColor;
            }
        }

        private static Color NodeColor(RoomNodeType type)
        {
            switch (type)
            {
                case RoomNodeType.Boss: return new Color(0.95f, 0.28f, 0.35f);
                case RoomNodeType.Forge: return new Color(1f, 0.45f, 0.18f);
                case RoomNodeType.Reward: return new Color(0.95f, 0.75f, 0.2f);
                case RoomNodeType.Secret: return new Color(0.72f, 0.4f, 1f);
                case RoomNodeType.Shop: return new Color(0.95f, 0.65f, 0.2f);
                case RoomNodeType.Shrine: return new Color(0.55f, 0.9f, 0.75f);
                default: return new Color(0.35f, 0.75f, 0.85f);
            }
        }
    }
}
