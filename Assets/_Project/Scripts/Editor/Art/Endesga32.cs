using System;
using System.Collections.Generic;
using UnityEngine;

namespace FotF.EditorTools.Art
{
    /// <summary>Палитра Endesga 32 (art bible §3, https://lospec.com/palette-list/endesga-32).</summary>
    public static class Endesga32
    {
        static readonly string[] Hex =
        {
            "be4a2f", "d77643", "ead4aa", "e4a672", "b86f50", "733e39", "3e2731", "a22633",
            "e43b44", "f77622", "feae34", "fee761", "63c74d", "3e8948", "265c42", "193c3e",
            "124e89", "0099db", "2ce8f5", "ffffff", "c0cbdc", "8b9bb4", "5a6988", "3a4466",
            "262b44", "181425", "ff0044", "68386c", "b55088", "f6757a", "e8b796", "c28569",
        };

        public static readonly Color32[] Colors = Build();
        static readonly HashSet<int> Keys = BuildKeys();

        static Color32[] Build()
        {
            var result = new Color32[Hex.Length];
            for (int i = 0; i < Hex.Length; i++)
            {
                int v = Convert.ToInt32(Hex[i], 16);
                result[i] = new Color32((byte)(v >> 16), (byte)(v >> 8), (byte)v, 255);
            }
            return result;
        }

        static HashSet<int> BuildKeys()
        {
            var set = new HashSet<int>();
            foreach (var c in Colors) set.Add(Key(c));
            return set;
        }

        public static int Key(Color32 c) => (c.r << 16) | (c.g << 8) | c.b;

        public static bool Contains(Color32 c) => Keys.Contains(Key(c));

        /// <summary>Ближайший цвет палитры по евклидову расстоянию в RGB.</summary>
        public static Color32 Nearest(Color32 c)
        {
            Color32 best = Colors[0];
            int bestDist = int.MaxValue;
            foreach (var p in Colors)
            {
                int dr = p.r - c.r, dg = p.g - c.g, db = p.b - c.b;
                int d = dr * dr + dg * dg + db * db;
                if (d < bestDist) { bestDist = d; best = p; }
            }
            return best;
        }

        public static string ToHex(Color32 c) => $"#{c.r:x2}{c.g:x2}{c.b:x2}";
    }
}
