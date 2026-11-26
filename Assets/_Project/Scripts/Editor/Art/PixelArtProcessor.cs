using System.Collections.Generic;
using UnityEngine;

namespace FotF.EditorTools.Art
{
    /// <summary>
    /// Чистая логика пост-обработки и проверки спрайтов по art bible §2–§4, §8.
    /// Пиксели — в порядке Unity (снизу вверх), как отдаёт Texture2D.GetPixels32.
    /// </summary>
    public static class PixelArtProcessor
    {
        /// <summary>Пиксели с альфой ниже порога становятся прозрачными, остальные — непрозрачными.</summary>
        public const int AlphaCut = 128;

        /// <summary>Все канвасы art bible кратны 8 (8, 16, 32, 48, 64, 96, 128, 160).</summary>
        public const int CanvasStep = 8;

        const int MaxUpscaleCheck = 8;

        /// <summary>Жёсткая альфа и привод каждого цвета к ближайшему из Endesga 32.</summary>
        public static Color32[] Quantize(Color32[] pixels)
        {
            var result = new Color32[pixels.Length];
            for (int i = 0; i < pixels.Length; i++)
            {
                var c = pixels[i];
                if (c.a < AlphaCut) { result[i] = new Color32(0, 0, 0, 0); continue; }
                var p = Endesga32.Nearest(c);
                result[i] = new Color32(p.r, p.g, p.b, 255);
            }
            return result;
        }

        /// <summary>
        /// Наибольший множитель k (2..8), при котором картинка — ровный апскейл
        /// (каждый блок k×k одного цвета). 1 — не апскейл.
        /// </summary>
        public static int DetectUpscale(Color32[] pixels, int width, int height)
        {
            for (int k = MaxUpscaleCheck; k >= 2; k--)
            {
                if (width % k != 0 || height % k != 0) continue;
                if (BlocksUniform(pixels, width, height, k)) return k;
            }
            return 1;
        }

        static bool BlocksUniform(Color32[] pixels, int width, int height, int k)
        {
            for (int y = 0; y < height; y++)
            for (int x = 0; x < width; x++)
            {
                var a = pixels[y * width + x];
                var b = pixels[(y - y % k) * width + (x - x % k)];
                if (a.r != b.r || a.g != b.g || a.b != b.b || a.a != b.a) return false;
            }
            return true;
        }

        /// <summary>Уменьшение в k раз по ближайшему соседу (берём левый нижний пиксель блока).</summary>
        public static Color32[] Downscale(Color32[] pixels, int width, int height, int k, out int newWidth, out int newHeight)
        {
            newWidth = width / k;
            newHeight = height / k;
            var result = new Color32[newWidth * newHeight];
            for (int y = 0; y < newHeight; y++)
            for (int x = 0; x < newWidth; x++)
                result[y * newWidth + x] = pixels[y * k * width + x * k];
            return result;
        }

        public sealed class Report
        {
            public readonly List<string> Errors = new List<string>();
            public readonly List<string> Warnings = new List<string>();
            public bool Ok => Errors.Count == 0;
        }

        /// <summary>Проверка art bible §8: размер канваса, палитра, жёсткая альфа, апскейл.</summary>
        public static Report Validate(Color32[] pixels, int width, int height)
        {
            var report = new Report();

            if (width % CanvasStep != 0 || height % CanvasStep != 0)
                report.Errors.Add($"размер {width}×{height} не кратен {CanvasStep} (канвасы art bible: 8/16/32/48/64/96/128/160)");

            int semiTransparent = 0, offPalette = 0, opaque = 0;
            string firstOff = null;
            foreach (var c in pixels)
            {
                if (c.a == 0) continue;
                if (c.a != 255) { semiTransparent++; continue; }
                opaque++;
                if (!Endesga32.Contains(c))
                {
                    offPalette++;
                    if (firstOff == null) firstOff = Endesga32.ToHex(c);
                }
            }

            if (semiTransparent > 0)
                report.Errors.Add($"{semiTransparent} полупрозрачных пикселей (нужна альфа 0 или 255)");
            if (offPalette > 0)
                report.Errors.Add($"{offPalette} пикселей вне Endesga 32 (например {firstOff})");

            if (opaque > 0)
            {
                int k = DetectUpscale(pixels, width, height);
                if (k > 1)
                    report.Warnings.Add($"похоже на апскейл ×{k}: пиксели не одного размера (если это не простой плейсхолдер — уменьшить)");
            }

            return report;
        }
    }
}
