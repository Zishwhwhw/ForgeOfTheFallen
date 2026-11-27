using System.Collections;
using FotF.Data;
using FotF.Logic;
using FotF.Logic.Fusion;
using FotF.State;
using FotF.View.Heroes;
using TMPro;
using UnityEngine;

namespace FotF.View.World
{
    /// <summary>
    /// Кузница (GDD §6.1, §6.4): две плиты на наковальне и рычаг. Без меню.
    /// Плита: E — положить оружие из рук (или забрать обратно). Рычаг: E — ритуал 3–4 с (E — пропустить) → гибрид падает на пол.
    /// Правила результата — FusionService (Logic).
    /// </summary>
    public sealed class ForgeView : MonoBehaviour
    {
        [SerializeField] FusionRulesDefinition rules;
        [SerializeField] WeaponCatalog catalog;
        [SerializeField] SpriteRenderer plateA, plateB;
        [SerializeField] Transform dropPoint;
        [SerializeField] WeaponPickupView pickupPrefab;
        [SerializeField] SpriteRenderer flash;
        [SerializeField] TextMeshPro speech;
        [SerializeField, Range(0f, 1f)] float flashAlpha = 0.5f;
        [SerializeField] int seed = 777;

        public WeaponInstance OnA { get; private set; }
        public WeaponInstance OnB { get; private set; }
        public bool Busy { get; private set; }
        public FusionResult? LastResult { get; private set; }

        IRandom _rng;
        bool _skip;

        void Awake() { _rng = new SeededRandom(seed); if (flash) flash.enabled = false; if (speech) speech.text = ""; }

        public bool CanPlace(WeaponInstance w) => !Busy && w != null && !w.IsHybrid;

        public void UsePlate(bool a, HeroController hero)
        {
            if (Busy) return;
            var current = a ? OnA : OnB;
            if (current == null)
            {
                var w = hero.State.ActiveWeapon.Instance;
                if (!CanPlace(w)) { Say(w != null && w.IsHybrid ? "…Гибрид уже не перековать." : "…Нечего класть."); return; }
                hero.TakeActiveWeapon();
                Set(a, w);
            }
            else
            {
                if (!hero.State.Weapons[0].IsEmpty && !hero.State.Weapons[1].IsEmpty) { Say("…Руки заняты."); return; }
                hero.PickUp(current); Set(a, null);
            }
        }

        public void PullLever(HeroController hero)
        {
            if (Busy) { _skip = true; return; }                         // повторное E — пропустить ритуал (GDD §6.4)
            if (OnA == null || OnB == null) { Say("…Две вещи. Не одну."); return; }
            StartCoroutine(Ritual());
        }

        IEnumerator Ritual()
        {
            Busy = true; _skip = false;
            var fr = rules ? new FusionRules(rules.dominantWeight, rules.jackpotChance, rules.floorOfBestParent) : FusionRules.Default;
            var result = FusionService.Fuse(OnA, OnB, _rng, fr);
            LastResult = result;
            float total = rules ? rules.ritualSeconds : 3.2f, t = 0f;
            Vector3 a0 = plateA.transform.localPosition, b0 = plateB.transform.localPosition;
            while (t < total && !_skip)
            {
                t += Time.deltaTime; float k = t / total;
                plateA.transform.localPosition = a0 + Vector3.up * Mathf.Min(1f, k * 2f) * 0.8f;        // оружия взлетают
                plateB.transform.localPosition = b0 + Vector3.up * Mathf.Min(1f, k * 2f) * 0.8f;
                if (flash)
                {
                    flash.enabled = k > 0.6f;
                    var c = result.Jackpot ? Color.Lerp(new Color32(0xf7, 0x76, 0x22, 0xff), new Color32(0x2c, 0xe8, 0xf5, 0xff), Mathf.PingPong(t * 3f, 1f)) : (Color)new Color32(0xf7, 0x76, 0x22, 0xff);
                    c.a = flashAlpha * Mathf.PingPong(t * 6f, 1f); flash.color = c;          // пульсирующая полупрозрачная вспышка
                }
                yield return null;
            }
            plateA.transform.localPosition = a0; plateB.transform.localPosition = b0;
            if (flash) flash.enabled = false;
            Set(true, null); Set(false, null);
            var p = Instantiate(pickupPrefab, dropPoint ? dropPoint.position : transform.position, Quaternion.identity);
            p.Init(result.Weapon);
            var lines = rules ? rules.frankenLines : null;
            Say((result.Jackpot ? "ИДЕАЛЬНЫЙ СПЛАВ! " : "") + (lines != null && lines.Length > 0 ? lines[_rng.NextInt(0, lines.Length)] : ""));
            Busy = false;
        }

        void Set(bool a, WeaponInstance w)
        {
            if (a) OnA = w; else OnB = w;
            var r = a ? plateA : plateB;
            var def = w != null && catalog ? catalog.Find(w.DefinitionId) : null;
            if (r) { r.sprite = def ? def.sprite : null; r.transform.localRotation = Quaternion.Euler(0, 0, def ? -def.spriteForwardAngle : 0f); }
        }

        void Say(string text) { if (speech) { speech.text = text; CancelInvoke(nameof(Clear)); Invoke(nameof(Clear), 3f); } }
        void Clear() { if (speech) speech.text = ""; }
    }
}


