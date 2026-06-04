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

namespace VDF.Web.Localization {
    public class LocalizationService {
        private static Dictionary<string, string> _translations = new();
        private static string _currentLanguage = "en";

        public static string CurrentLanguage {
            get => _currentLanguage;
            set {
                _currentLanguage = value;
                LoadLanguage(value);
            }
        }

        static LocalizationService() {
            LoadLanguage("en");
        }

        private static void LoadLanguage(string langCode) {
            _translations = langCode switch {
                "zh-Hans" => GetChineseTranslations(),
                "en" => GetEnglishTranslations(),
                _ => GetEnglishTranslations()
            };
        }

        public static string Get(string key) {
            return _translations.TryGetValue(key, out var value) ? value : key;
        }

        public static string Get(string key, params object[] args) {
            var template = Get(key);
            try {
                return string.Format(template, args);
            }
            catch {
                return template;
            }
        }

        private static Dictionary<string, string> GetEnglishTranslations() {
            return new Dictionary<string, string> {
                // Navigation
                { "Nav.Scan", "Scan" },
                { "Nav.Results", "Results" },
                { "Nav.Settings", "Settings" },
                { "Nav.About", "About" },
                
                // Scan page
                { "Scan.Title", "Video Duplicate Finder - Scan" },
                { "Scan.Directories", "Scan Directories" },
                { "Scan.ExcludeDirectories", "Exclude Directories" },
                { "Scan.IncludeImages", "Include Images" },
                { "Scan.SimilarityThreshold", "Similarity Threshold (%)" },
                { "Scan.StartScan", "Start Scan" },
                { "Scan.Scanning", "Scanning..." },
                { "Scan.ScanComplete", "Scan Complete" },
                
                // Results
                { "Results.Title", "Duplicate Results" },
                { "Results.Groups", "Duplicate Groups: {0}" },
                { "Results.Files", "Total Duplicate Files: {0}" },
                { "Results.SavedSpace", "Space Saved: {0}" },
                { "Results.NoResults", "No duplicate files found" },
                { "Results.ExportResults", "Export Results" },
                
                // Settings
                { "Settings.Title", "Settings" },
                { "Settings.Language", "Language" },
                { "Settings.Theme", "Theme" },
                { "Settings.Dark", "Dark" },
                { "Settings.Light", "Light" },
                { "Settings.FFmpegPath", "FFmpeg Path" },
                { \"Settings.Save\", \"Save\" },
                
                // Common
                { \"Common.Cancel\", \"Cancel\" },
                { \"Common.Delete\", \"Delete\" },
                { \"Common.Confirm\", \"Confirm\" },
                { \"Common.Error\", \"Error\" },
                { \"Common.Success\", \"Success\" }
            };
        }

        private static Dictionary<string, string> GetChineseTranslations() {
            return new Dictionary<string, string> {
                // Navigation
                { "Nav.Scan", "扫描" },
                { "Nav.Results", "结果" },
                { "Nav.Settings", "设置" },
                { "Nav.About", "关于" },
                
                // Scan page
                { "Scan.Title", "视频重复查找器 - 扫描" },
                { "Scan.Directories", "扫描目录" },
                { "Scan.ExcludeDirectories", "排除目录" },
                { "Scan.IncludeImages", "包含图像" },
                { "Scan.SimilarityThreshold", "相似度阈值 (%)" },
                { "Scan.StartScan", "开始扫描" },
                { "Scan.Scanning", "正在扫描..." },
                { "Scan.ScanComplete", "扫描完成" },
                
                // Results
                { "Results.Title", "重复文件结果" },
                { "Results.Groups", "重复组：{0}" },
                { "Results.Files", "总重复文件数：{0}" },
                { "Results.SavedSpace", "节省空间：{0}" },
                { "Results.NoResults", "未找到重复文件" },
                { "Results.ExportResults", "导出结果" },
                
                // Settings
                { "Settings.Title", "设置" },
                { "Settings.Language", "语言" },
                { "Settings.Theme", "主题" },
                { "Settings.Dark", "深色" },
                { "Settings.Light", "浅色" },
                { "Settings.FFmpegPath", "FFmpeg 路径" },
                { "Settings.Save", "保存" },
                
                // Common
                { "Common.Cancel", "取消" },
                { "Common.Delete", "删除" },
                { "Common.Confirm", "确认" },
                { "Common.Error", "错误" },
                { "Common.Success", "成功" }
            };
        }
    }
}
