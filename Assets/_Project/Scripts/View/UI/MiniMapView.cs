using System.Collections.Generic;
using FotF.Logic.Generation;
using FotF.View.Heroes;
using FotF.View.World;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace FotF.View.UI
{
    /// <summary>
    /// Мини-карта этажа (GDD §3.2): туман, соседние комнаты видны контуром, значок типа — на «вывесочных»
    /// комнатах и на посещённых. Телепорта нет. Клетки и связи — готовые префабы, раскладка по сетке этажа.
    /// </summary>
    public sealed class MiniMapView : MonoBehaviour
    {
        [SerializeField] FloorRunner runner;
        [SerializeField] HeroController hero;
        [SerializeField] RectTransform container;
        [SerializeField] Image cellPrefab;
        [SerializeField] Image linkPrefab;
        [SerializeField] float cellSize = 9f;
        [SerializeField] float spacing = 13f;

        [Header("Цвета")]
        [SerializeField] Color seen = new Color(0.35f, 0.36f, 0.45f, 0.6f);
        [SerializeField] Color visited = new Color(0.55f, 0.57f, 0.68f, 0.95f);
        [SerializeField] Color current = new Color(1f, 0.85f, 0.45f, 1f);
        [SerializeField] Color portal = new Color(0.62f, 0.4f, 1f, 1f);

        FloorMapFog _fog;
        FloorRoom _portalShown;
        readonly Dictionary<FloorRoom, Image> _cells = new Dictionary<FloorRoom, Image>();
        readonly List<(FloorRoom a, FloorRoom b, Image img)> _links = new List<(FloorRoom, FloorRoom, Image)>();

        void OnEnable() { if (runner) runner.FloorBuilt += Rebuild; if (runner && runner.Plan != null) Rebuild(runner); }
        void OnDisable() { if (runner) runner.FloorBuilt -= Rebuild; }

        void Rebuild(FloorRunner r)
        {
            for (int i = container.childCount - 1; i >= 0; i--) Destroy(container.GetChild(i).gameObject);
            _cells.Clear(); _links.Clear();
            _fog = new FloorMapFog(r.Plan); _portalShown = null;
            foreach (var (a, b) in r.Plan.Links())
            {
                var img = Instantiate(linkPrefab, container);
                Vector2 pa = Pos(a), pb = Pos(b);
                img.rectTransform.anchoredPosition = (pa + pb) / 2f;
                bool horizontal = a.Cell.Y == b.Cell.Y;
                img.rectTransform.sizeDelta = horizontal ? new Vector2(spacing - cellSize, 2f) : new Vector2(2f, spacing - cellSize);
                _links.Add((a, b, img));
            }
            foreach (var room in r.Plan.Rooms)
            {
                var img = Instantiate(cellPrefab, container);
                img.rectTransform.anchoredPosition = Pos(room);
                img.rectTransform.sizeDelta = new Vector2(cellSize, cellSize);
                _cells[room] = img;
            }
            _fog.Enter(r.Plan.Start.Index);
            Refresh();
        }

        Vector2 Pos(FloorRoom r) => new Vector2(r.Cell.X * spacing, r.Cell.Y * spacing);

        void Update()
        {
            if (_fog == null || runner == null || hero == null) return;
            var room = runner.RoomAt(hero.transform.position);
            if (room != null && _fog.Enter(room.Index)) Refresh();
            else if (runner.PortalRoom != _portalShown) { _portalShown = runner.PortalRoom; Refresh(); }
        }

        void Refresh()
        {
            foreach (var kv in _cells)
            {
                var room = kv.Key; var img = kv.Value;
                var v = _fog.Of(room);
                img.gameObject.SetActive(v != FloorMapFog.Visibility.Hidden);
                bool isPortal = runner.PortalRoom == room;
                img.color = room.Index == _fog.Current ? current : isPortal ? portal : v == FloorMapFog.Visibility.Visited ? visited : seen;
                var label = img.GetComponentInChildren<TextMeshProUGUI>(true);
                if (label) label.text = isPortal ? "V" : _fog.ShowsIcon(room) ? Letter(room.Kind) : "";
            }
            foreach (var (a, b, img) in _links)
                img.gameObject.SetActive(_fog.Of(a) != FloorMapFog.Visibility.Hidden && _fog.Of(b) != FloorMapFog.Visibility.Hidden);
        }

        /// <summary>Буквы-заглушки до иконок карты (список для PixelLab в SESSION_CONTEXT).</summary>
        static string Letter(RoomKind k) => k switch
        {
            RoomKind.Merchant => "Т", RoomKind.Fountain => "Ф", RoomKind.MiniBoss => "Б", RoomKind.Shrine => "С",
            RoomKind.Forge => "К", RoomKind.Chest => "$", RoomKind.Elite => "Э", RoomKind.Cage => "П", RoomKind.Secret => "?",
            _ => "",
        };
    }
}
