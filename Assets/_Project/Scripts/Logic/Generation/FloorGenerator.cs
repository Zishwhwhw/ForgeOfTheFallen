using System;
using System.Collections.Generic;

namespace FotF.Logic.Generation
{
    /// <summary>Тип комнаты этажа (GDD §8.3).</summary>
    public enum RoomKind { Start, Combat, Elite, MiniBoss, Forge, Merchant, Fountain, Shrine, Cage, Secret, Chest }

    /// <summary>Клетка сетки этажа (одна комната на клетку; решение владельца 28.11: сетка + коридоры).</summary>
    public readonly struct Cell : IEquatable<Cell>
    {
        public readonly int X, Y;
        public Cell(int x, int y) { X = x; Y = y; }
        public static readonly Cell[] Directions = { new Cell(0, 1), new Cell(1, 0), new Cell(0, -1), new Cell(-1, 0) };
        public static Cell operator +(Cell a, Cell b) => new Cell(a.X + b.X, a.Y + b.Y);
        public static Cell operator -(Cell a, Cell b) => new Cell(a.X - b.X, a.Y - b.Y);
        public bool IsAdjacent(Cell o) => Math.Abs(X - o.X) + Math.Abs(Y - o.Y) == 1;
        public bool Equals(Cell o) => X == o.X && Y == o.Y;
        public override bool Equals(object obj) => obj is Cell c && Equals(c);
        public override int GetHashCode() => X * 73856093 ^ Y * 19349663;
        public override string ToString() => $"({X},{Y})";
    }

    public sealed class FloorRoom
    {
        public int Index;
        public RoomKind Kind;
        public Cell Cell;
        /// <summary>На обязательном пути старт → боевые (ответвления — нет).</summary>
        public bool OnMainPath;
        /// <summary>Расстояние от старта по связям.</summary>
        public int Depth;
        public readonly List<int> Links = new List<int>();
        /// <summary>Секрет: за стеной, вход ломается бомбой (GDD п.27); на карте не виден.</summary>
        public bool Hidden => Kind == RoomKind.Secret;
        /// <summary>Портал этажа ждёт зачистки всех таких комнат (GDD §3.1). Элита и мини-босс — необязательные ответвления (§8.2).</summary>
        public bool RequiredForPortal => Kind == RoomKind.Combat;
        public bool HasFight => Kind == RoomKind.Combat || Kind == RoomKind.Elite || Kind == RoomKind.MiniBoss;
    }

    public sealed class FloorPlan
    {
        public readonly List<FloorRoom> Rooms = new List<FloorRoom>();
        public FloorRoom Start => Rooms[0];

        public FloorRoom At(Cell c)
        {
            foreach (var r in Rooms) if (r.Cell.Equals(c)) return r;
            return null;
        }

        public bool Linked(FloorRoom a, FloorRoom b) => a.Links.Contains(b.Index);

        public IEnumerable<(FloorRoom a, FloorRoom b)> Links()
        {
            foreach (var r in Rooms)
                foreach (var i in r.Links)
                    if (i > r.Index) yield return (r, Rooms[i]);
        }
    }

    /// <summary>Правила этажа (из FloorDefinition + BalanceConstants), без зависимости от Unity.</summary>
    public struct FloorRules
    {
        public int CombatMin, CombatMax, MaxBranches;
        public float Forge, Merchant, Fountain, Shrine, Cage, Secret, Elite, MiniBoss, ChestRoom;
    }

    /// <summary>
    /// Граф этажа по seed (ARCHITECTURE §4, GDD §8.1–8.2): обязательный путь старт → N боевых по сетке,
    /// спец-комнаты — тупиковые ответвления от пути. Без random walk по тайлам и BSP: сетка комнат + шаблоны.
    /// </summary>
    public static class FloorGenerator
    {
        /// <summary>Порядок важности ответвлений при лимите maxBranches.</summary>
        static readonly RoomKind[] BranchPriority =
            { RoomKind.Forge, RoomKind.Merchant, RoomKind.MiniBoss, RoomKind.Elite, RoomKind.Chest, RoomKind.Shrine, RoomKind.Fountain, RoomKind.Cage, RoomKind.Secret };

        public static FloorPlan Generate(FloorRules rules, IRandom rng)
        {
            for (int attempt = 0; attempt < 200; attempt++)
            {
                var plan = TryGenerate(rules, rng);
                if (plan != null && Validate(plan, rules, out _)) return plan;
            }
            throw new InvalidOperationException("FloorGenerator: не удалось построить этаж за 200 попыток");
        }

        static FloorPlan TryGenerate(FloorRules rules, IRandom rng)
        {
            var plan = new FloorPlan();
            int combat = rng.NextInt(rules.CombatMin, Math.Max(rules.CombatMin, rules.CombatMax) + 1);

            // Обязательный путь: самоизбегающая цепочка клеток.
            var start = Add(plan, RoomKind.Start, new Cell(0, 0), true);
            var prev = start;
            for (int i = 0; i < combat; i++)
            {
                var free = FreeNeighbours(plan, prev.Cell);
                if (free.Count == 0) return null;
                var room = Add(plan, RoomKind.Combat, free[rng.NextInt(0, free.Count)], true);
                Link(prev, room);
                prev = room;
            }

            // Ответвления: тупики, прикреплённые к обязательному пути.
            var wanted = new List<RoomKind>();
            foreach (var kind in BranchPriority)
                if (wanted.Count < rules.MaxBranches && rng.NextFloat() < Chance(rules, kind)) wanted.Add(kind);
            foreach (var kind in wanted)
            {
                var hosts = new List<FloorRoom>();
                foreach (var r in plan.Rooms)
                {
                    if (!r.OnMainPath || FreeNeighbours(plan, r.Cell).Count == 0) continue;
                    if ((kind == RoomKind.Elite || kind == RoomKind.MiniBoss) && r.Kind != RoomKind.Combat) continue; // вглубь этажа, не от старта
                    hosts.Add(r);
                }
                if (hosts.Count == 0) continue;                                   // места нет — ответвление пропускается
                var host = hosts[rng.NextInt(0, hosts.Count)];
                var cells = FreeNeighbours(plan, host.Cell);
                Link(host, Add(plan, kind, cells[rng.NextInt(0, cells.Count)], false));
            }

            ComputeDepth(plan);
            return plan;
        }

        static float Chance(FloorRules r, RoomKind k) => k switch
        {
            RoomKind.Forge => r.Forge, RoomKind.Merchant => r.Merchant, RoomKind.Fountain => r.Fountain,
            RoomKind.Shrine => r.Shrine, RoomKind.Cage => r.Cage, RoomKind.Secret => r.Secret,
            RoomKind.Elite => r.Elite, RoomKind.MiniBoss => r.MiniBoss, RoomKind.Chest => r.ChestRoom,
            _ => 0f,
        };

        static FloorRoom Add(FloorPlan plan, RoomKind kind, Cell cell, bool main)
        {
            var r = new FloorRoom { Index = plan.Rooms.Count, Kind = kind, Cell = cell, OnMainPath = main };
            plan.Rooms.Add(r);
            return r;
        }

        static void Link(FloorRoom a, FloorRoom b) { a.Links.Add(b.Index); b.Links.Add(a.Index); }

        static List<Cell> FreeNeighbours(FloorPlan plan, Cell c)
        {
            var list = new List<Cell>(4);
            foreach (var d in Cell.Directions) if (plan.At(c + d) == null) list.Add(c + d);
            return list;
        }

        static void ComputeDepth(FloorPlan plan)
        {
            foreach (var r in plan.Rooms) r.Depth = -1;
            var q = new Queue<FloorRoom>();
            plan.Start.Depth = 0; q.Enqueue(plan.Start);
            while (q.Count > 0)
            {
                var r = q.Dequeue();
                foreach (var i in r.Links)
                    if (plan.Rooms[i].Depth < 0) { plan.Rooms[i].Depth = r.Depth + 1; q.Enqueue(plan.Rooms[i]); }
            }
        }

        /// <summary>Гарантии GDD §8.2 + правила сетки. Используется генератором и тестами.</summary>
        public static bool Validate(FloorPlan plan, FloorRules rules, out string error)
        {
            error = null;
            if (plan.Rooms.Count == 0 || plan.Start.Kind != RoomKind.Start) { error = "нет старта"; return false; }
            var cells = new HashSet<Cell>();
            int combat = 0, starts = 0;
            foreach (var r in plan.Rooms)
            {
                if (!cells.Add(r.Cell)) { error = $"две комнаты в клетке {r.Cell}"; return false; }
                if (r.Kind == RoomKind.Start) starts++;
                if (r.Kind == RoomKind.Combat) combat++;
                foreach (var i in r.Links)
                {
                    var o = plan.Rooms[i];
                    if (!r.Cell.IsAdjacent(o.Cell)) { error = $"связь не по соседству {r.Cell}–{o.Cell}"; return false; }
                    if (!o.Links.Contains(r.Index)) { error = "связь в одну сторону"; return false; }
                }
                if (r.Depth < 0) { error = $"{r.Kind} {r.Cell} недостижима от старта"; return false; }   // путь существует
                if (r.OnMainPath != (r.Kind == RoomKind.Start || r.Kind == RoomKind.Combat)) { error = $"{r.Kind} на неверном месте"; return false; }
                if (!r.OnMainPath)
                {
                    // Сервис/секрет/элита — тупик: не перекрывают единственный маршрут, не обязательны.
                    if (r.Links.Count != 1) { error = $"ответвление {r.Kind} не тупик"; return false; }
                    if (!plan.Rooms[r.Links[0]].OnMainPath) { error = $"ответвление {r.Kind} не от основного пути"; return false; }
                }
            }
            if (starts != 1) { error = "стартов не один"; return false; }
            if (combat < rules.CombatMin || combat > Math.Max(rules.CombatMin, rules.CombatMax)) { error = $"боевых {combat}"; return false; }
            return true;
        }
    }
}
