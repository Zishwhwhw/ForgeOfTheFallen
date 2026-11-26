using FotF.EditorTools.Art;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;

namespace FotF.Tests.Art
{
    public class PixelArtProcessorTests
    {
        static Color32[] Fill(int count, Color32 c)
        {
            var px = new Color32[count];
            for (int i = 0; i < count; i++) px[i] = c;
            return px;
        }

        [Test]
        public void Palette_Has32UniqueColors()
        {
            Assert.AreEqual(32, Endesga32.Colors.Length);
            var keys = new System.Collections.Generic.HashSet<int>();
            foreach (var c in Endesga32.Colors) keys.Add(Endesga32.Key(c));
            Assert.AreEqual(32, keys.Count);
        }

        [Test]
        public void Quantize_SnapsToPaletteAndHardAlpha()
        {
            var input = new[]
            {
                new Color32(250, 120, 30, 255), // рядом с #f77622
                new Color32(10, 10, 10, 100),   // полупрозрачный -> прозрачный
                new Color32(10, 10, 10, 200),   // почти непрозрачный -> #181425
            };
            var output = PixelArtProcessor.Quantize(input);

            Assert.AreEqual("#f77622", Endesga32.ToHex(output[0]));
            Assert.AreEqual(255, output[0].a);
            Assert.AreEqual(0, output[1].a);
            Assert.AreEqual("#181425", Endesga32.ToHex(output[2]));
            Assert.AreEqual(255, output[2].a);
        }

        [Test]
        public void DetectUpscale_FindsIntegerFactor()
        {
            // 2×2 «настоящих» пикселя, растянутые в 4 раза -> 8×8
            var small = new[] { Endesga32.Colors[0], Endesga32.Colors[1], Endesga32.Colors[2], Endesga32.Colors[3] };
            var big = new Color32[64];
            for (int y = 0; y < 8; y++)
            for (int x = 0; x < 8; x++)
                big[y * 8 + x] = small[(y / 4) * 2 + x / 4];

            Assert.AreEqual(4, PixelArtProcessor.DetectUpscale(big, 8, 8));

            var down = PixelArtProcessor.Downscale(big, 8, 8, 4, out int w, out int h);
            Assert.AreEqual(2, w);
            Assert.AreEqual(2, h);
            CollectionAssert.AreEqual(small, down);
        }

        [Test]
        public void DetectUpscale_ReturnsOneForNativeArt()
        {
            var px = new Color32[16];
            for (int i = 0; i < px.Length; i++) px[i] = Endesga32.Colors[i % 32];
            Assert.AreEqual(1, PixelArtProcessor.DetectUpscale(px, 4, 4));
        }

        [Test]
        public void Validate_AcceptsCleanSprite()
        {
            var px = PixelArtProcessor.Quantize(Fill(32 * 32, new Color32(200, 100, 50, 255)));
            px[0] = new Color32(0, 0, 0, 0);
            px[33] = Endesga32.Colors[5]; // ломает однородность блоков -> не апскейл
            var report = PixelArtProcessor.Validate(px, 32, 32);
            Assert.IsTrue(report.Ok, string.Join("; ", report.Errors));
            Assert.IsEmpty(report.Warnings);
        }

        [Test]
        public void Validate_RejectsWrongSizeOffPaletteAndSoftAlpha()
        {
            var px = Fill(30 * 30, new Color32(1, 2, 3, 255));
            px[0] = new Color32(0, 0, 0, 128);
            var report = PixelArtProcessor.Validate(px, 30, 30);
            Assert.AreEqual(3, report.Errors.Count, string.Join("; ", report.Errors));
        }
    }

    /// <summary>Весь арт в Assets/_Project/Art соответствует art bible §2–§4 и §7.</summary>
    public class ArtAssetsTests
    {
        [Test]
        public void AllArt_PassesArtBibleChecks()
        {
            var failures = new System.Text.StringBuilder();
            foreach (var path in PixelArtMenu.FindArtPngs())
            {
                var report = PixelArtMenu.ValidateFile(path);
                foreach (var e in report.Errors) failures.AppendLine($"{path}: {e}");
            }
            Assert.IsTrue(failures.Length == 0, failures.ToString());
        }

        [Test]
        public void AllArt_HasPixelArtImportSettings()
        {
            var failures = new System.Text.StringBuilder();
            foreach (var path in PixelArtMenu.FindArtPngs())
            {
                var ti = (TextureImporter)AssetImporter.GetAtPath(path);
                if (ti.filterMode != FilterMode.Point
                    || !Mathf.Approximately(ti.spritePixelsPerUnit, PixelArtImportPostprocessor.PixelsPerUnit)
                    || ti.textureCompression != TextureImporterCompression.Uncompressed
                    || ti.mipmapEnabled)
                    failures.AppendLine(path);
            }
            Assert.IsTrue(failures.Length == 0, "Неверные настройки импорта:\n" + failures);
        }
    }
}
