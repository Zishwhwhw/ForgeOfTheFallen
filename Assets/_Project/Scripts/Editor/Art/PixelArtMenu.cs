using System.IO;
using UnityEditor;
using UnityEngine;

namespace FotF.EditorTools.Art
{
    /// <summary>Меню FotF/Арт: привести выбранные спрайты к Endesga 32 и проверить весь арт.</summary>
    public static class PixelArtMenu
    {
        const string MenuRoot = "FotF/Арт/";

        [MenuItem(MenuRoot + "Привести выбранное к Endesga 32")]
        static void QuantizeSelected()
        {
            int done = 0;
            foreach (var obj in Selection.objects)
            {
                var path = AssetDatabase.GetAssetPath(obj);
                if (!path.EndsWith(".png", System.StringComparison.OrdinalIgnoreCase)) continue;
                if (QuantizeFile(path, askDownscale: true)) done++;
            }
            Debug.Log($"[FotF Арт] Обработано файлов: {done}");
        }

        [MenuItem(MenuRoot + "Привести выбранное к Endesga 32", true)]
        static bool QuantizeSelectedValidate() => Selection.objects.Length > 0;

        /// <summary>Переписывает PNG: (опционально) уменьшение апскейла, жёсткая альфа, Endesga 32.</summary>
        public static bool QuantizeFile(string assetPath, bool askDownscale)
        {
            var tex = Load(assetPath);
            if (tex == null) return false;

            int w = tex.width, h = tex.height;
            var pixels = tex.GetPixels32();
            Object.DestroyImmediate(tex);

            int k = PixelArtProcessor.DetectUpscale(pixels, w, h);
            if (k > 1 && (!askDownscale || EditorUtility.DisplayDialog(
                    "Апскейл", $"{assetPath}\nпохоже на апскейл ×{k} ({w}×{h}). Уменьшить до {w / k}×{h / k}?", "Уменьшить", "Оставить")))
            {
                pixels = PixelArtProcessor.Downscale(pixels, w, h, k, out w, out h);
            }

            pixels = PixelArtProcessor.Quantize(pixels);

            var output = new Texture2D(w, h, TextureFormat.RGBA32, false);
            output.SetPixels32(pixels);
            output.Apply();
            File.WriteAllBytes(assetPath, output.EncodeToPNG());
            Object.DestroyImmediate(output);

            AssetDatabase.ImportAsset(assetPath, ImportAssetOptions.ForceUpdate);
            Debug.Log($"[FotF Арт] {assetPath}: {w}×{h}, приведено к Endesga 32");
            return true;
        }

        [MenuItem(MenuRoot + "Проверить весь арт")]
        static void ValidateAll()
        {
            int bad = 0, total = 0;
            foreach (var path in FindArtPngs())
            {
                total++;
                var report = ValidateFile(path);
                foreach (var e in report.Errors) Debug.LogError($"[FotF Арт] {path}: {e}");
                foreach (var wrn in report.Warnings) Debug.LogWarning($"[FotF Арт] {path}: {wrn}");
                if (!report.Ok) bad++;
            }
            Debug.Log($"[FotF Арт] Проверено: {total}, с ошибками: {bad}");
        }

        public static string[] FindArtPngs()
        {
            var root = PixelArtImportPostprocessor.ArtRoot.TrimEnd('/');
            if (!AssetDatabase.IsValidFolder(root)) return new string[0];
            var guids = AssetDatabase.FindAssets("t:Texture2D", new[] { root });
            var list = new System.Collections.Generic.List<string>();
            foreach (var g in guids)
            {
                var p = AssetDatabase.GUIDToAssetPath(g);
                if (p.EndsWith(".png", System.StringComparison.OrdinalIgnoreCase)) list.Add(p);
            }
            return list.ToArray();
        }

        public static PixelArtProcessor.Report ValidateFile(string assetPath)
        {
            var tex = Load(assetPath);
            if (tex == null)
            {
                var r = new PixelArtProcessor.Report();
                r.Errors.Add("не удалось прочитать PNG");
                return r;
            }
            var report = PixelArtProcessor.Validate(tex.GetPixels32(), tex.width, tex.height);
            Object.DestroyImmediate(tex);
            return report;
        }

        // Читаем исходный PNG с диска, а не импортированную текстуру: без Read/Write и без сжатия.
        static Texture2D Load(string assetPath)
        {
            if (!File.Exists(assetPath)) return null;
            var tex = new Texture2D(2, 2, TextureFormat.RGBA32, false);
            if (tex.LoadImage(File.ReadAllBytes(assetPath))) return tex;
            Object.DestroyImmediate(tex);
            return null;
        }
    }
}
