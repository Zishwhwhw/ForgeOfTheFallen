using UnityEngine;

namespace FrankenArms
{
    internal static class RuntimeSpriteFactory
    {
        internal static Sprite CreateSquare(int size = 16)
        {
            var texture = new Texture2D(size, size, TextureFormat.RGBA32, false)
            {
                filterMode = FilterMode.Point,
                wrapMode = TextureWrapMode.Clamp
            };
            var pixels = new Color[size * size];
            for (var i = 0; i < pixels.Length; i++) pixels[i] = Color.white;
            texture.SetPixels(pixels);
            texture.Apply();
            return Sprite.Create(texture, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), size);
        }

        internal static Sprite CreatePixelArt(string[] rows, Color[] palette, float pixelsPerUnit)
        {
            var height = rows.Length;
            var width = rows[0].Length;
            var texture = new Texture2D(width, height, TextureFormat.RGBA32, false)
            {
                filterMode = FilterMode.Point,
                wrapMode = TextureWrapMode.Clamp
            };
            var pixels = new Color[width * height];
            for (var y = 0; y < height; y++)
            {
                for (var x = 0; x < width; x++)
                {
                    var symbol = rows[height - 1 - y][x];
                    var index = symbol - '0';
                    pixels[y * width + x] = index >= 0 && index < palette.Length ? palette[index] : Color.clear;
                }
            }
            texture.SetPixels(pixels);
            texture.Apply();
            return Sprite.Create(texture, new Rect(0, 0, width, height), new Vector2(0.5f, 0.5f), pixelsPerUnit);
        }

        internal static Sprite CreatePlayer()
        {
            var palette = new[] { Color.clear, new Color(0.08f, 0.12f, 0.2f), new Color(0.25f, 0.85f, 0.95f), new Color(0.95f, 0.75f, 0.35f), Color.white };
            return CreatePixelArt(new[] { "001111100", "011222110", "012442210", "012222210", "001333100", "011333110", "012222210", "012002210", "011001110", "010000010", "010000010" }, palette, 16f);
        }

        internal static Sprite CreateEnemy()
        {
            var palette = new[] { Color.clear, new Color(0.35f, 0.05f, 0.12f), new Color(0.95f, 0.2f, 0.3f), new Color(1f, 0.65f, 0.7f), Color.white };
            return CreatePixelArt(new[] { "001111100", "012222210", "122442221", "122222221", "122222221", "012222210", "001111100", "010000010", "010000010" }, palette, 16f);
        }

        internal static Sprite CreateDoor()
        {
            var palette = new[] { Color.clear, new Color(0.18f, 0.08f, 0.28f), new Color(0.7f, 0.25f, 0.95f), new Color(1f, 0.75f, 0.25f) };
            return CreatePixelArt(new[] { "01110", "01210", "01210", "01210", "01210", "01210", "01210", "01210", "01210", "01210", "01210", "01110" }, palette, 8f);
        }

        internal static Sprite CreateProjectile()
        {
            var palette = new[] { Color.clear, new Color(1f, 0.35f, 0.05f), new Color(1f, 0.9f, 0.35f), Color.white };
            return CreatePixelArt(new[] { "010", "123", "010" }, palette, 16f);
        }

        internal static Sprite CreateCoin()
        {
            var palette = new[] { Color.clear, new Color(0.75f, 0.35f, 0.05f), new Color(1f, 0.85f, 0.2f), Color.white };
            return CreatePixelArt(new[] { "0110", "1231", "1231", "0110" }, palette, 12f);
        }
    }
}
