using System.IO;
using FotF.View.Animation;
using UnityEditor;
using UnityEngine;

namespace FotF.EditorTools.Art
{
    /// <summary>
    /// Считает покачивание тела по кадрам (верхний непрозрачный ряд кадра относительно первого кадра idle того же направления)
    /// и записывает в SpriteAnimationSet: по нему оружие в руке двигается вместе с телом (Art Bible §5.1).
    /// </summary>
    public static class BodyBobBaker
    {
        [MenuItem("FotF/Art/Bake Body Bob (selected animation sets)")]
        static void BakeSelected()
        {
            foreach (var set in Selection.GetFiltered<SpriteAnimationSet>(SelectionMode.Assets)) Bake(set);
        }

        public static void Bake(SpriteAnimationSet set)
        {
            var idle = set.Find("idle");
            foreach (var clip in set.clips)
            {
                clip.bobDown = Compute(clip.down, idle?.down);
                clip.bobSide = Compute(clip.side, idle?.side);
                clip.bobUp = Compute(clip.up, idle?.up);
            }
            EditorUtility.SetDirty(set);
            AssetDatabase.SaveAssetIfDirty(set);
        }

        static int[] Compute(Sprite[] frames, Sprite[] idleFrames)
        {
            if (frames == null || frames.Length == 0) return System.Array.Empty<int>();
            int reference = idleFrames != null && idleFrames.Length > 0 ? TopRow(idleFrames[0]) : TopRow(frames[0]);
            var tops = new int[frames.Length];
            for (int i = 0; i < frames.Length; i++) tops[i] = TopRow(frames[i]);
            return BobFromTops(tops, reference);
        }

        /// <summary>Покачивание (+ вверх) = насколько верх силуэта выше опорного. Кадр без пикселей — 0.</summary>
        public static int[] BobFromTops(int[] topRowsFromTop, int referenceTopFromTop)
        {
            var bob = new int[topRowsFromTop.Length];
            for (int i = 0; i < bob.Length; i++) bob[i] = topRowsFromTop[i] < 0 ? 0 : referenceTopFromTop - topRowsFromTop[i];
            return bob;
        }

        /// <summary>Первый сверху ряд кадра с непрозрачным пикселем (читается из PNG, импорт не трогаем). -1 — пусто.</summary>
        static int TopRow(Sprite sprite)
        {
            if (sprite == null) return -1;
            var tex = new Texture2D(2, 2);
            tex.LoadImage(File.ReadAllBytes(AssetDatabase.GetAssetPath(sprite.texture)));
            var r = sprite.rect;
            try
            {
                for (int y = (int)r.yMax - 1; y >= (int)r.yMin; y--)
                    for (int x = (int)r.xMin; x < (int)r.xMax; x++)
                        if (tex.GetPixel(x, y).a > 0.5f) return (int)r.yMax - 1 - y;
                return -1;
            }
            finally { Object.DestroyImmediate(tex); }
        }
    }
}
