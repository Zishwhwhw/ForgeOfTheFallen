using System.IO;
using FotF.Data;
using Newtonsoft.Json.Linq;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;

namespace FotF.Tests.View
{
    /// <summary>Смерть врага (GDD §10.2): числа SO совпадают с балансом JSON, префаб врага проведён.</summary>
    public class EnemyDeathTests
    {
        const string FeelPath = "Assets/_Project/Data/GameFeel/gamefeel_default.asset";
        const string EnemyPrefab = "Assets/_Project/Prefabs/Enemies/enemy_skeleton_swordsman.prefab";

        static JObject Item(string file, string id)
        {
            var root = JObject.Parse(File.ReadAllText(Path.Combine("Assets/_Project/Data/Balance", file)));
            foreach (var it in root["items"]) if ((string)it["id"] == id) return (JObject)it;
            Assert.Fail($"{file}: нет {id}"); return null;
        }

        [Test]
        public void GameFeelAsset_MatchesBalanceJson()
        {
            var feel = AssetDatabase.LoadAssetAtPath<GameFeelDefinition>(FeelPath);
            Assert.IsNotNull(feel, FeelPath);
            var j = Item("gamefeel.json", feel.id);
            var d = (JObject)j["death"];
            Assert.AreEqual((float)j["enemyHitFlashSeconds"], feel.enemyHitFlashSeconds, 1e-5f);
            Assert.AreEqual((float)j["killMicroPauseSeconds"], feel.killMicroPauseSeconds, 1e-5f);
            Assert.AreEqual((float)d["flashSeconds"], feel.deathFlashSeconds, 1e-5f);
            Assert.AreEqual((float)d["squashSeconds"], feel.deathSquashSeconds, 1e-5f);
            Assert.AreEqual((float)d["squashScaleY"], feel.deathSquashScaleY, 1e-5f);
            Assert.AreEqual((float)d["squashScaleX"], feel.deathSquashScaleX, 1e-5f);
            Assert.AreEqual((float)d["corpseDarken"], feel.corpseDarken, 1e-5f);
            Assert.AreEqual((float)d["bossLieSeconds"], feel.bossLieSeconds, 1e-5f);
            Assert.AreEqual((float)d["bossDissolveSeconds"], feel.bossDissolveSeconds, 1e-5f);

            var constants = JObject.Parse(File.ReadAllText("Assets/_Project/Data/Balance/constants.json"));
            var limit = constants.SelectToken("$..corpseLimitPerFloor");
            Assert.IsNotNull(limit, "constants.json: нет corpseLimitPerFloor");
            Assert.AreEqual((int)limit, feel.corpseLimitPerFloor);
        }

        [Test]
        public void EnemyPrefab_HasWiredDeathView()
        {
            var root = AssetDatabase.LoadAssetAtPath<GameObject>(EnemyPrefab);
            var death = root.GetComponentInChildren<FotF.View.Enemies.EnemyDeathView>(true);
            Assert.IsNotNull(death, "нет EnemyDeathView");
            var so = new SerializedObject(death);
            foreach (var f in new[] { "feel", "body", "animator", "flash", "sortingGroup" })
                Assert.IsNotNull(so.FindProperty(f).objectReferenceValue, $"EnemyDeathView.{f} не назначен");
            var ctrl = new SerializedObject(root.GetComponent<FotF.View.Enemies.EnemyController>());
            Assert.AreEqual(death, ctrl.FindProperty("deathView").objectReferenceValue, "EnemyController.deathView не назначен");
        }
    }
}
