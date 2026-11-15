using System.Collections.Generic;
using UnityEngine;

namespace FrankenArms
{
    public static class LocalizationService
    {
        private const string LanguageKey = "FrankenArms.Language";
        private static readonly string[] Languages = { "EN", "RU", "UA", "PL", "DE", "ES" };
        private static readonly Dictionary<string, string[]> Texts = new()
        {
            { "room", new[] { "ROOM", "КОМНАТА", "КІМНАТА", "POKÓJ", "RAUM", "SALA" } },
            { "threats", new[] { "THREATS", "УГРОЗЫ", "ЗАГРОЗИ", "ZAGROŻENIA", "GEFAHREN", "AMENAZAS" } },
            { "credits", new[] { "CREDITS", "КРЕДИТЫ", "КРЕДИТИ", "KREDYTY", "KREDITE", "CRÉDITOS" } },
            { "mana", new[] { "MANA", "МАНА", "МАНА", "MANA", "MANA", "MANÁ" } },
            { "dash_ready", new[] { "DASH READY", "РЫВОК ГОТОВ", "РИВОК ГОТОВИЙ", "DASH GOTOWY", "DASH BEREIT", "DASH LISTO" } },
            { "best_room", new[] { "BEST ROOM", "ЛУЧШАЯ КОМНАТА", "НАЙКРАЩА КІМНАТА", "NAJLEPSZY POKÓJ", "BESTE RAUM", "MEJOR SALA" } },
            { "language", new[] { "LANGUAGE", "ЯЗЫК", "МОВА", "JĘZYK", "SPRACHE", "IDIOMA" } }
        };

        public static string CurrentLanguage { get { return PlayerPrefs.GetString(LanguageKey, "EN"); } }

        public static string T(string key)
        {
            if (!Texts.TryGetValue(key, out var values)) return key;
            var index = System.Array.IndexOf(Languages, CurrentLanguage);
            return values[Mathf.Max(0, index)];
        }

        public static void CycleLanguage()
        {
            var index = System.Array.IndexOf(Languages, CurrentLanguage);
            PlayerPrefs.SetString(LanguageKey, Languages[(index + 1) % Languages.Length]);
            PlayerPrefs.Save();
        }
    }
}
