using System.IO;
using FrankenArms;
using UnityEditor;
using UnityEngine;

public static class CreateRouteAssets
{
    public static void Main()
    {
        var nodeFolder = "Assets/Resources/Content/Routes/Nodes";
        var routeFolder = "Assets/Resources/Content/Routes";
        Directory.CreateDirectory(nodeFolder);

        var specs = new[]
        {
            Spec(1, "Warmup", RoomNodeType.Combat, 3, 10, false, "Teach movement and the free base weapon."),
            Spec(2, "Pursuit", RoomNodeType.Combat, 4, 10, false, "First chaser pressure; keep a flank open."),
            Spec(3, "Crossfire", RoomNodeType.Combat, 5, 10, false, "Read projectile telegraphs and Dash through gaps."),
            Spec(4, "Gauntlet", RoomNodeType.Combat, 5, 15, false, "Pressure spike before the first boss gate."),
            Spec(5, "Elite Gate", RoomNodeType.Combat, 6, 20, false, "Optional elite-style build check."),
            Spec(6, "Boss Gate", RoomNodeType.Boss, 1, 40, false, "Fixed arena, boss reward, and recovery."),
            Spec(7, "Relief", RoomNodeType.Reward, 0, 20, false, "Safe recovery node after the boss."),
            Spec(8, "Forge Branch", RoomNodeType.Forge, 0, 0, true, "Choose a hybrid without a combat wave."),
            Spec(9, "Secret Archive", RoomNodeType.Secret, 0, 25, true, "Hidden optional room; never required for the route."),
            Spec(10, "Biome Boss", RoomNodeType.Boss, 1, 50, false, "Second boss gate in the full route."),
            Spec(11, "Relic Vault", RoomNodeType.Reward, 0, 25, false, "Guaranteed reward choice."),
            Spec(12, "Risk Arena", RoomNodeType.Combat, 6, 20, false, "Ranged pressure and elite threat."),
            Spec(13, "Forge Branch", RoomNodeType.Forge, 0, 0, true, "A safe but slower power route."),
            Spec(14, "Trial", RoomNodeType.Combat, 7, 25, true, "Optional high-value challenge."),
            Spec(15, "Biome Boss", RoomNodeType.Boss, 1, 60, false, "Third boss gate."),
            Spec(16, "Relief", RoomNodeType.Reward, 0, 25, false, "Recovery after the third boss."),
            Spec(17, "Secret Workshop", RoomNodeType.Secret, 0, 35, true, "Hidden blueprint opportunity."),
            Spec(18, "Crossfire", RoomNodeType.Combat, 7, 25, false, "Mixed shooter and chaser room."),
            Spec(19, "Elite Gate", RoomNodeType.Combat, 8, 30, false, "Late-biome build check."),
            Spec(20, "Biome Boss", RoomNodeType.Boss, 1, 70, false, "Fourth boss gate."),
            Spec(21, "Final Preparation", RoomNodeType.Reward, 0, 30, false, "Preparation before the final sequence."),
            Spec(22, "Final Forge", RoomNodeType.Forge, 0, 0, false, "Final irreversible hybrid decision."),
            Spec(23, "Final Trial", RoomNodeType.Combat, 8, 35, false, "Short combat test before the final boss."),
            Spec(24, "Franken Archon", RoomNodeType.Boss, 1, 100, false, "Final five-point decision sequence and end boss.")
        };

        var nodes = new RoomNodeDefinition[specs.Length];
        for (var i = 0; i < specs.Length; i++)
        {
            var path = nodeFolder + "/Floor" + specs[i].floor.ToString("00") + ".asset";
            var node = AssetDatabase.LoadAssetAtPath<RoomNodeDefinition>(path);
            if (node == null)
            {
                node = ScriptableObject.CreateInstance<RoomNodeDefinition>();
                AssetDatabase.CreateAsset(node, path);
            }
            node.floor = specs[i].floor;
            node.displayName = specs[i].displayName;
            node.nodeType = specs[i].nodeType;
            node.threatBudget = specs[i].threatBudget;
            node.rewardCredits = specs[i].rewardCredits;
            node.optional = specs[i].optional;
            node.routeHint = specs[i].routeHint;
            EditorUtility.SetDirty(node);
            nodes[i] = node;
        }

        var routePath = routeFolder + "/FrankenArmsMainRoute.asset";
        var route = AssetDatabase.LoadAssetAtPath<DungeonRouteDefinition>(routePath);
        if (route == null)
        {
            route = ScriptableObject.CreateInstance<DungeonRouteDefinition>();
            AssetDatabase.CreateAsset(route, routePath);
        }
        route.routeName = "FrankenArms Main Route // Authored 24 Floors";
        route.nodes = nodes;
        EditorUtility.SetDirty(route);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh(ImportAssetOptions.ForceUpdate);
    }

    private static NodeSpec Spec(int floor, string name, RoomNodeType type, int threats, int credits, bool optional, string hint)
    {
        return new NodeSpec { floor = floor, displayName = name, nodeType = type, threatBudget = threats, rewardCredits = credits, optional = optional, routeHint = hint };
    }

    private struct NodeSpec
    {
        public int floor;
        public string displayName;
        public RoomNodeType nodeType;
        public int threatBudget;
        public int rewardCredits;
        public bool optional;
        public string routeHint;
    }
}
