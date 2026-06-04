// /*
//     Copyright (C) 2026 0x90d
//     This file is part of VideoDuplicateFinder
//     VideoDuplicateFinder is free software: you can redistribute it and/or modify
//     it under the terms of the GNU Affero General Public License as published by
//     the Free Software Foundation, either version 3 of the License, or
//     (at your option) any later version.
//     VideoDuplicateFinder is distributed in the hope that it will be useful,
//     but WITHOUT ANY WARRANTY without even the implied warranty of
//     MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
//     GNU Affero General Public License for more details.
//     You should have received a copy of the GNU Affero General Public License
//     along with VideoDuplicateFinder.  If not, see <http://www.gnu.org/licenses/>.
// */
//

using System.Text.Json;
using System.Reflection;
using System.Globalization;

namespace VDF.CLI.Localization {
    public class LocalizationService {
        private static Dictionary<string, string> _translations = new();
        private static string _currentLanguage = "en";

        static LocalizationService() {
            // Auto-detect system language
            string systemLang = CultureInfo.CurrentCulture.Name.Replace("-", "-");
            if (systemLang.StartsWith("zh", StringComparison.OrdinalIgnoreCase)) {
                _currentLanguage = "zh-Hans";
            }
            LoadLanguage(_currentLanguage);
        }

        public static void SetLanguage(string langCode) {
            _currentLanguage = langCode;
            LoadLanguage(langCode);
        }

        private static void LoadLanguage(string langCode) {
            try {
                var assembly = Assembly.GetExecutingAssembly();
                var resourceName = $"VDF.CLI.Localization.{langCode}.json";
                using var stream = assembly.GetManifestResourceStream(resourceName);
                if (stream == null) {
                    // Fall back to English
                    if (langCode != "en") {
                        LoadLanguage("en");
                        return;
                    }
                    _translations = new();
                    return;
                }
                using var reader = new StreamReader(stream);
                var json = reader.ReadToEnd();
                _translations = JsonSerializer.Deserialize<Dictionary<string, string>>(json) ?? new();
            }
            catch {
                _translations = new();
            }
        }

        public static string Get(string key) {
            return _translations.TryGetValue(key, out var value) ? value : key;
        }

        public static string Get(string key, params object[] args) {
            var template = Get(key);
            return string.Format(template, args);
        }
    }
}
