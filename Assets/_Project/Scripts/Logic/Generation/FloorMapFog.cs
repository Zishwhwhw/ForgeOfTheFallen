using System.Collections.Generic;

namespace FotF.Logic.Generation
{
    /// <summary>
    /// Туман мини-карты (GDD §3.2): посещённые комнаты видны полностью, их соседи — контуром;
    /// у соседа виден значок, только если это торговец, фонтан, мини-босс или святилище. Секреты не видны.
    /// </summary>
    public sealed class FloorMapFog
    {
        public enum Visibility { Hidden, Seen, Visited }

        readonly FloorPlan _plan;
        readonly HashSet<int> _visited = new HashSet<int>();
        public int Current { get; private set; } = -1;

        public FloorMapFog(FloorPlan plan) { _plan = plan; }

        /// <summary>Герой вошёл в комнату. true — что-то изменилось на карте.</summary>
        public bool Enter(int roomIndex)
        {
            if (roomIndex == Current) return false;
            Current = roomIndex;
            _visited.Add(roomIndex);
            return true;
        }

        public Visibility Of(FloorRoom room)
        {
            if (_visited.Contains(room.Index)) return Visibility.Visited;
            if (room.Hidden) return Visibility.Hidden;
            foreach (var i in room.Links) if (_visited.Contains(i)) return Visibility.Seen;
            return Visibility.Hidden;
        }

        /// <summary>Значок типа комнаты: у посещённой — всегда (кроме обычных), у увиденной — только у «вывесочных» типов.</summary>
        public bool ShowsIcon(FloorRoom room)
        {
            var v = Of(room);
            if (v == Visibility.Hidden) return false;
            if (v == Visibility.Visited) return room.Kind != RoomKind.Combat && room.Kind != RoomKind.Start;
            return IsSignposted(room.Kind);
        }

        public static bool IsSignposted(RoomKind k) =>
            k == RoomKind.Merchant || k == RoomKind.Fountain || k == RoomKind.MiniBoss || k == RoomKind.Shrine;
    }
}
