using UnityEngine;

namespace FrankenArms
{
    internal static class RuntimeSpriteFactory
    {
        private static Sprite authoredPlayer;
        private static Sprite[] authoredEnemies;

        private static Sprite LoadAuthoredPlayer()
        {
            if (authoredPlayer == null)
                authoredPlayer = Resources.Load<Sprite>("Art/Generated/hero-gunsmith-v1");
            return authoredPlayer;
        }

        private static Sprite LoadAuthoredEnemy(string spriteName)
        {
            if (authoredEnemies == null || authoredEnemies.Length < 3)
                authoredEnemies = Resources.LoadAll<Sprite>("Art/Generated/enemy-roster-v1");
            for (var i = 0; i < authoredEnemies.Length; i++)
                if (authoredEnemies[i].name == spriteName) return authoredEnemies[i];
            return null;
        }

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
            var authored = LoadAuthoredPlayer();
            if (authored != null) return authored;
            var palette = new[] { Color.clear, new Color(0.08f, 0.12f, 0.2f), new Color(0.25f, 0.85f, 0.95f), new Color(0.95f, 0.75f, 0.35f), Color.white };
            return CreatePixelArt(new[] { "001111100", "011222110", "012442210", "012222210", "001333100", "011333110", "012222210", "012002210", "011001110", "010000010", "010000010" }, palette, 16f);
        }

        internal static Sprite[] CreatePlayerFrames()
        {
            var authored = LoadAuthoredPlayer();
            if (authored != null) return new[] { authored, authored };
            var palette = new[] { Color.clear, new Color(0.08f, 0.12f, 0.2f), new Color(0.25f, 0.85f, 0.95f), new Color(0.95f, 0.75f, 0.35f), Color.white };
            return new[]
            {
                CreatePixelArt(new[] { "001111100", "011222110", "012442210", "012222210", "001333100", "011333110", "012222210", "012002210", "011001110", "010000010", "010000010" }, palette, 16f),
                CreatePixelArt(new[] { "001111100", "011222110", "012442210", "012222210", "001333100", "011333110", "012222210", "011002110", "010001010", "010000010", "010000010" }, palette, 16f)
            };
        }

        internal static Sprite CreateEnemy()
        {
            var authored = LoadAuthoredEnemy("enemy-slime");
            if (authored != null) return authored;
            var palette = new[] { Color.clear, new Color(0.35f, 0.05f, 0.12f), new Color(0.95f, 0.2f, 0.3f), new Color(1f, 0.65f, 0.7f), Color.white };
            return CreatePixelArt(new[] { "001111100", "012222210", "122442221", "122222221", "122222221", "012222210", "001111100", "010000010", "010000010" }, palette, 16f);
        }

        internal static Sprite CreateEnemyChaser()
        {
            return LoadAuthoredEnemy("enemy-chaser") ?? CreateEnemy();
        }

        internal static Sprite CreateEliteEnemy()
        {
            return LoadAuthoredEnemy("enemy-elite") ?? CreateEnemyChaser();
        }

        internal static Sprite[] CreateEnemyFrames(Sprite authoredSprite = null)
        {
            if (authoredSprite != null) return new[] { authoredSprite, authoredSprite };
            var palette = new[] { Color.clear, new Color(0.35f, 0.05f, 0.12f), new Color(0.95f, 0.2f, 0.3f), new Color(1f, 0.65f, 0.7f), Color.white };
            return new[]
            {
                CreatePixelArt(new[] { "001111100", "012222210", "122442221", "122222221", "122222221", "012222210", "001111100", "010000010", "010000010" }, palette, 16f),
                CreatePixelArt(new[] { "001111100", "012222210", "122442221", "122222221", "012222210", "012222210", "001111100", "010000010", "010000010" }, palette, 16f)
            };
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

        internal static Sprite CreateWall()
        {
            var palette = new[] { Color.clear, new Color(0.08f, 0.09f, 0.15f), new Color(0.18f, 0.2f, 0.3f), new Color(0.35f, 0.28f, 0.45f) };
            return CreatePixelArt(new[] { "12223321", "12333321", "12223321", "11222211", "12223321", "12333321", "12223321", "11222211" }, palette, 8f);
        }

        internal static Sprite CreateFloorTile()
        {
            var palette = new[] { new Color(0.05f, 0.07f, 0.12f), new Color(0.07f, 0.1f, 0.17f), new Color(0.1f, 0.13f, 0.21f) };
            return CreatePixelArt(new[] { "00110011", "01100110", "11001100", "10011001", "00110011", "01100110", "11001100", "10011001" }, palette, 8f);
        }
    }
}
