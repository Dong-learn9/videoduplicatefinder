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

using Microsoft.JSInterop;
using Microsoft.AspNetCore.Components;

namespace VDF.Web.Localization;

/// <summary>
/// Scoped localization service — one instance per Blazor circuit.
/// Reads language from cookie / Accept-Language header on init.
/// Persists preference to cookie via JS interop on change.
/// </summary>
public class LocalizationService {
    private Dictionary<string, string> _translations = new();
    private string _currentLanguage = "en";

    public string CurrentLanguage => _currentLanguage;

    public static readonly (string Code, string NativeName)[] SupportedLanguages = {
        ("en", "English"),
        ("zh-Hans", "简体中文")
    };

    public LocalizationService(IHttpContextAccessor httpContextAccessor) {
        var ctx = httpContextAccessor.HttpContext;
        if (ctx != null) {
            // 1. Cookie takes priority
            var cookieLang = ctx.Request.Cookies["vdf-lang"];
            if (cookieLang == "zh-Hans" || cookieLang == "en") {
                _currentLanguage = cookieLang;
            }
            else {
                // 2. Fall back to Accept-Language header
                var acceptLang = ctx.Request.Headers["Accept-Language"].ToString();
                if (!string.IsNullOrEmpty(acceptLang) && acceptLang.Contains("zh"))
                    _currentLanguage = "zh-Hans";
            }
        }
        LoadTranslations();
    }

    void LoadTranslations() {
        _translations = _currentLanguage switch {
            "zh-Hans" => GetChineseTranslations(),
            _ => GetEnglishTranslations()
        };
    }

    public string Get(string key) =>
        _translations.TryGetValue(key, out var value) ? value : key;

    public string Get(string key, params object[] args) {
        var template = Get(key);
        try { return string.Format(template, args); }
        catch { return template; }
    }

    /// <summary>
    /// Change language, persist to cookie, and force-reload the page
    /// so all components pick up the new translations.
    /// </summary>
    public async Task SetLanguageAsync(string langCode, IJSRuntime js, NavigationManager nav) {
        _currentLanguage = langCode;
        LoadTranslations();
        try {
            await js.InvokeVoidAsync("vdf.setLanguage", langCode);
        }
        catch { }
        nav.NavigateTo(nav.Uri, forceLoad: true);
    }

    // ─── English ────────────────────────────────────────────────────────
    private static Dictionary<string, string> GetEnglishTranslations() => new() {
        // Nav
        { "Nav.Scan", "Scan" },
        { "Nav.Results", "Results" },
        { "Nav.Settings", "Settings" },

        // Login
        { "Login.Title", "Video Duplicate Finder" },
        { "Login.Hint", "Your password was printed in the console when the app started." },
        { "Login.DockerHint", "Run docker logs vdf to see it." },
        { "Login.Password", "Password" },
        { "Login.EnterPassword", "Enter password" },
        { "Login.RememberMe", "Remember me (30 days)" },
        { "Login.IncorrectPassword", "Incorrect password. Check your console output or docker logs." },
        { "Login.LogIn", "Log in" },

        // Scan page
        { "Scan.Title", "Scan" },
        { "Scan.CheckingFFmpeg", "Checking FFmpeg availability..." },
        { "Scan.FFmpegNotFound", "FFmpeg not found" },
        { "Scan.FFmpegDownloadFailed", "FFmpeg download failed" },
        { "Scan.Retry", "Retry" },
        { "Scan.Error", "Error" },
        { "Scan.Dismiss", "Dismiss" },
        { "Scan.HashingFiles", "Hashing files..." },
        { "Scan.ComparingFiles", "Comparing {0} hashed files for duplicates..." },
        { "Scan.ComparingFilesShort", "Comparing files for duplicates..." },
        { "Scan.ComparingNote", "This may take a while depending on library size." },
        { "Scan.RetrievingThumbnails", "Retrieving thumbnails..." },
        { "Scan.Pause", "Pause" },
        { "Scan.Resume", "Resume" },
        { "Scan.Stop", "Stop" },
        { "Scan.IncludePaths", "Include paths" },
        { "Scan.ExcludePaths", "Exclude paths" },
        { "Scan.Accessible", "Accessible" },
        { "Scan.NotAccessible", "Not found or not accessible" },
        { "Scan.NotFound", "Not found" },
        { "Scan.Add", "Add" },
        { "Scan.Browse", "Browse…" },
        { "Scan.Reset", "Reset" },
        { "Scan.SaveSettings", "Save settings" },
        { "Scan.StartScan", "Start Scan" },
        { "Scan.SettingsSaved", "Settings saved." },
        { "Scan.FailedToSave", "Failed to save settings." },
        { "Scan.ScanComplete", "Scan complete — view {0} result(s)" },
        { "Scan.ScanStopped", "Scan was stopped." },
        { "Scan.PathToScan", "/path/to/scan" },
        { "Scan.PathToExclude", "/path/to/exclude" },

        // Results page
        { "Results.Title", "Duplicate Groups" },
        { "Results.NoDuplicates", "No duplicates found." },
        { "Results.RunScan", "Run a scan" },
        { "Results.First", "first." },
        { "Results.FilterByPath", "Filter by path..." },
        { "Results.Groups", "group(s)" },
        { "Results.Files", "file(s)" },
        { "Results.Selected", "selected" },
        { "Results.Select", "Select" },
        { "Results.LowestQuality", "Lowest quality" },
        { "Results.SmallestFile", "Smallest file" },
        { "Results.Oldest", "Oldest (keep newest)" },
        { "Results.Newest", "Newest (keep oldest)" },
        { "Results.HundredPercentEqual", "100% equal groups" },
        { "Results.InvertSelection", "Invert selection" },
        { "Results.DeselectAll", "Deselect all" },
        { "Results.CollapseAll", "Collapse all" },
        { "Results.ExpandAll", "Expand all" },
        { "Results.RemoveFromList", "Remove from list" },
        { "Results.DeleteTrash", "Delete (trash)" },
        { "Results.DeletePermanently", "Delete permanently" },
        { "Results.MoveToFolder", "Move to folder…" },
        { "Results.DestinationFolder", "Destination folder:" },
        { "Results.MoveFiles", "Move {0} file(s)" },
        { "Results.Cancel", "Cancel" },
        { "Results.PermanentlyDelete", "Permanently delete" },
        { "Results.MoveToTrash", "Move to trash" },
        { "Results.FilesQuestion", "{0} file(s)?" },
        { "Results.CannotBeUndone", "This cannot be undone." },
        { "Results.Confirm", "Confirm" },
        { "Results.Similar", "similar" },
        { "Results.SortBy", "Sort by…" },
        { "Results.Similarity", "Similarity" },
        { "Results.Size", "Size" },
        { "Results.Duration", "Duration" },
        { "Results.Bitrate", "Bitrate" },
        { "Results.Resolution", "Resolution" },
        { "Results.Date", "Date" },
        { "Results.Name", "Name" },
        { "Results.Compare", "Compare" },
        { "Results.KeepBest", "Keep Best" },
        { "Results.NoPreview", "No preview" },
        { "Results.Res", "Res" },
        { "Results.Dur", "Dur" },
        { "Results.FPS", "FPS" },
        { "Results.Format", "Format" },
        { "Results.BestQuality", "Best quality" },
        { "Results.PartialClip", "partial clip" },
        { "Results.Hidden", "hidden — click to show all" },
        { "Results.SideBySide", "Side by Side" },
        { "Results.Swipe", "Swipe" },
        { "Results.OpenFile", "Open file" },
        { "Results.OpenFolder", "Open folder" },
        { "Results.CopyPath", "Copy path" },
        { "Results.Deselect", "Deselect" },
        { "Results.SelectItem", "Select" },
        { "Results.DeselectGroup", "Deselect group" },
        { "Results.SelectGroup", "Select group" },
        { "Results.HideCard", "Hide card" },
        { "Results.PermanentlyDeleted", "permanently deleted" },
        { "Results.MovedToTrash", "moved to trash" },
        { "Results.FilesMovedTo", "{0} file(s) moved to '{1}'." },
        { "Results.Failed", "failed" },
        { "Results.Kbps", "kbps" },

        // Settings page
        { "Settings.Title", "Settings" },
        { "Settings.SaveSettings", "Save Settings" },
        { "Settings.Saved", "Settings saved." },
        { "Settings.FailedToSave", "Failed to save settings." },
        { "Settings.Language", "Language" },
        { "Settings.Similarity", "Similarity" },
        { "Settings.Threshold", "Threshold (hash difference, 0–10, lower = stricter)" },
        { "Settings.MinSimilarity", "Minimum similarity %" },
        { "Settings.DurationTolerance", "Duration difference tolerance %" },
        { "Settings.Scanning", "Scanning" },
        { "Settings.ScanSubdirs", "Scan subdirectories" },
        { "Settings.IncludeImages", "Include images" },
        { "Settings.UsePHashing", "Use perceptual hashing" },
        { "Settings.CompareFlipped", "Compare horizontally flipped" },
        { "Settings.IgnoreBlack", "Ignore black pixels" },
        { "Settings.IgnoreWhite", "Ignore white pixels" },
        { "Settings.MaxParallelism", "Max degree of parallelism (-1 = all cores)" },
        { "Settings.ThumbnailCount", "Thumbnail count per file" },
        { "Settings.FFmpeg", "FFmpeg" },
        { "Settings.NativeBinding", "Use native FFmpeg binding" },
        { "Settings.HardwareAccel", "Hardware acceleration" },
        { "Settings.CustomFFArgs", "Custom FFmpeg arguments" },
        { "Settings.PartialClip", "Partial Clip Detection" },
        { "Settings.EnablePartialClip", "Enable partial clip detection (audio fingerprinting)" },
        { "Settings.MinClipRatio", "Min clip / source ratio %" },
        { "Settings.MinAudioSimilarity", "Min audio similarity %" },
        { "Settings.WebUIThumbs", "WebUI Thumbnails" },
        { "Settings.AutoLoadThumbs", "Automatically load thumbnails on results page" },
        { "Settings.ThumbWidth", "Thumbnail width (px) — lower = less memory, more pixelated" },
        { "Settings.JpegQuality", "JPEG quality (10–95) — lower = smaller files, more artifacts" },
        { "Settings.Database", "Database" },
        { "Settings.ScanNonExisting", "Scan against non-existing files (compare database entries whose files have been deleted or moved)" },
        { "Settings.CustomDbFolder", "Custom database folder" },
        { "Settings.CheckpointInterval", "Database checkpoint interval (minutes, 0 = disabled)" },
        { "Settings.Maintenance", "Maintenance" },
        { "Settings.CleanDatabase", "Clean Database" },
        { "Settings.ClearDatabase", "Clear Database" },
        { "Settings.ClearConfirm", "This will delete ALL stored data. Are you sure?" },
        { "Settings.YesClear", "Yes, clear" },
        { "Settings.CleanupComplete", "Cleanup complete — {0} entry(ies) removed." },
        { "Settings.CleanupFailed", "Cleanup failed: {0}" },
        { "Settings.DatabaseCleared", "Database cleared." },
        { "Settings.ClearFailed", "Clear failed: {0}" },

        // Folder browser
        { "Folder.SelectFolder", "Select Folder" },
        { "Folder.EnterPath", "Enter path..." },
        { "Folder.Go", "Go" },
        { "Folder.AccessDenied", "Access denied." },
        { "Folder.DirNotFound", "Directory not found." },
        { "Folder.NoSubdirs", "No subdirectories" },
        { "Folder.NoFolderSelected", "No folder selected" },
        { "Folder.Select", "Select" },
    };

    // ─── Chinese ────────────────────────────────────────────────────────
    private static Dictionary<string, string> GetChineseTranslations() => new() {
        // Nav
        { "Nav.Scan", "扫描" },
        { "Nav.Results", "结果" },
        { "Nav.Settings", "设置" },

        // Login
        { "Login.Title", "视频重复查找器" },
        { "Login.Hint", "密码在应用启动时已输出到控制台。" },
        { "Login.DockerHint", "运行 docker logs vdf 查看。" },
        { "Login.Password", "密码" },
        { "Login.EnterPassword", "输入密码" },
        { "Login.RememberMe", "记住我（30天）" },
        { "Login.IncorrectPassword", "密码错误。请查看控制台输出或 docker 日志。" },
        { "Login.LogIn", "登录" },

        // Scan page
        { "Scan.Title", "扫描" },
        { "Scan.CheckingFFmpeg", "正在检查 FFmpeg 可用性..." },
        { "Scan.FFmpegNotFound", "未找到 FFmpeg" },
        { "Scan.FFmpegDownloadFailed", "FFmpeg 下载失败" },
        { "Scan.Retry", "重试" },
        { "Scan.Error", "错误" },
        { "Scan.Dismiss", "关闭" },
        { "Scan.HashingFiles", "正在哈希文件..." },
        { "Scan.ComparingFiles", "正在比较 {0} 个已哈希文件查找重复..." },
        { "Scan.ComparingFilesShort", "正在比较文件查找重复..." },
        { "Scan.ComparingNote", "根据库的大小，这可能需要一段时间。" },
        { "Scan.RetrievingThumbnails", "正在获取缩略图..." },
        { "Scan.Pause", "暂停" },
        { "Scan.Resume", "继续" },
        { "Scan.Stop", "停止" },
        { "Scan.IncludePaths", "包含路径" },
        { "Scan.ExcludePaths", "排除路径" },
        { "Scan.Accessible", "可访问" },
        { "Scan.NotAccessible", "未找到或不可访问" },
        { "Scan.NotFound", "未找到" },
        { "Scan.Add", "添加" },
        { "Scan.Browse", "浏览…" },
        { "Scan.Reset", "重置" },
        { "Scan.SaveSettings", "保存设置" },
        { "Scan.StartScan", "开始扫描" },
        { "Scan.SettingsSaved", "设置已保存。" },
        { "Scan.FailedToSave", "保存设置失败。" },
        { "Scan.ScanComplete", "扫描完成 — 查看 {0} 个结果" },
        { "Scan.ScanStopped", "扫描已停止。" },
        { "Scan.PathToScan", "/扫描路径" },
        { "Scan.PathToExclude", "/排除路径" },

        // Results page
        { "Results.Title", "重复组" },
        { "Results.NoDuplicates", "未找到重复文件。" },
        { "Results.RunScan", "运行扫描" },
        { "Results.First", "。" },
        { "Results.FilterByPath", "按路径筛选..." },
        { "Results.Groups", "组" },
        { "Results.Files", "文件" },
        { "Results.Selected", "已选择" },
        { "Results.Select", "选择" },
        { "Results.LowestQuality", "最低质量" },
        { "Results.SmallestFile", "最小文件" },
        { "Results.Oldest", "最旧（保留最新）" },
        { "Results.Newest", "最新（保留最旧）" },
        { "Results.HundredPercentEqual", "100% 相同组" },
        { "Results.InvertSelection", "反选" },
        { "Results.DeselectAll", "取消全选" },
        { "Results.CollapseAll", "全部折叠" },
        { "Results.ExpandAll", "全部展开" },
        { "Results.RemoveFromList", "从列表移除" },
        { "Results.DeleteTrash", "删除（回收站）" },
        { "Results.DeletePermanently", "永久删除" },
        { "Results.MoveToFolder", "移动到文件夹…" },
        { "Results.DestinationFolder", "目标文件夹：" },
        { "Results.MoveFiles", "移动 {0} 个文件" },
        { "Results.Cancel", "取消" },
        { "Results.PermanentlyDelete", "永久删除" },
        { "Results.MoveToTrash", "移至回收站" },
        { "Results.FilesQuestion", "{0} 个文件？" },
        { "Results.CannotBeUndone", "此操作无法撤销。" },
        { "Results.Confirm", "确认" },
        { "Results.Similar", "相似" },
        { "Results.SortBy", "排序方式…" },
        { "Results.Similarity", "相似度" },
        { "Results.Size", "大小" },
        { "Results.Duration", "时长" },
        { "Results.Bitrate", "比特率" },
        { "Results.Resolution", "分辨率" },
        { "Results.Date", "日期" },
        { "Results.Name", "名称" },
        { "Results.Compare", "比较" },
        { "Results.KeepBest", "保留最佳" },
        { "Results.NoPreview", "无预览" },
        { "Results.Res", "分辨率" },
        { "Results.Dur", "时长" },
        { "Results.FPS", "帧率" },
        { "Results.Format", "格式" },
        { "Results.BestQuality", "最佳质量" },
        { "Results.PartialClip", "片段" },
        { "Results.Hidden", "已隐藏 — 点击显示全部" },
        { "Results.SideBySide", "并排" },
        { "Results.Swipe", "滑动" },
        { "Results.OpenFile", "打开文件" },
        { "Results.OpenFolder", "打开文件夹" },
        { "Results.CopyPath", "复制路径" },
        { "Results.Deselect", "取消选择" },
        { "Results.SelectItem", "选择" },
        { "Results.DeselectGroup", "取消选择组" },
        { "Results.SelectGroup", "选择组" },
        { "Results.HideCard", "隐藏卡片" },
        { "Results.PermanentlyDeleted", "已永久删除" },
        { "Results.MovedToTrash", "已移至回收站" },
        { "Results.FilesMovedTo", "{0} 个文件已移动到「{1}」。" },
        { "Results.Failed", "失败" },
        { "Results.Kbps", "kbps" },

        // Settings page
        { "Settings.Title", "设置" },
        { "Settings.SaveSettings", "保存设置" },
        { "Settings.Saved", "设置已保存。" },
        { "Settings.FailedToSave", "保存设置失败。" },
        { "Settings.Language", "语言" },
        { "Settings.Similarity", "相似度" },
        { "Settings.Threshold", "阈值（哈希差异，0-10，越小越严格）" },
        { "Settings.MinSimilarity", "最低相似度 %" },
        { "Settings.DurationTolerance", "时长差异容忍度 %" },
        { "Settings.Scanning", "扫描" },
        { "Settings.ScanSubdirs", "扫描子目录" },
        { "Settings.IncludeImages", "包含图片" },
        { "Settings.UsePHashing", "使用感知哈希" },
        { "Settings.CompareFlipped", "比较水平翻转" },
        { "Settings.IgnoreBlack", "忽略黑色像素" },
        { "Settings.IgnoreWhite", "忽略白色像素" },
        { "Settings.MaxParallelism", "最大并行度（-1 = 所有核心）" },
        { "Settings.ThumbnailCount", "每个文件的缩略图数量" },
        { "Settings.FFmpeg", "FFmpeg" },
        { "Settings.NativeBinding", "使用原生 FFmpeg 绑定" },
        { "Settings.HardwareAccel", "硬件加速" },
        { "Settings.CustomFFArgs", "自定义 FFmpeg 参数" },
        { "Settings.PartialClip", "片段检测" },
        { "Settings.EnablePartialClip", "启用片段检测（音频指纹）" },
        { "Settings.MinClipRatio", "最小片段/源比率 %" },
        { "Settings.MinAudioSimilarity", "最低音频相似度 %" },
        { "Settings.WebUIThumbs", "WebUI 缩略图" },
        { "Settings.AutoLoadThumbs", "自动加载结果页缩略图" },
        { "Settings.ThumbWidth", "缩略图宽度（像素）— 越小越省内存，越模糊" },
        { "Settings.JpegQuality", "JPEG 质量（10-95）— 越小文件越小，画质越差" },
        { "Settings.Database", "数据库" },
        { "Settings.ScanNonExisting", "扫描不存在的文件（比较已删除或移动的数据库条目）" },
        { "Settings.CustomDbFolder", "自定义数据库文件夹" },
        { "Settings.CheckpointInterval", "数据库检查点间隔（分钟，0 = 禁用）" },
        { "Settings.Maintenance", "维护" },
        { "Settings.CleanDatabase", "清理数据库" },
        { "Settings.ClearDatabase", "清空数据库" },
        { "Settings.ClearConfirm", "这将删除所有存储的数据。确定要继续吗？" },
        { "Settings.YesClear", "确定清空" },
        { "Settings.CleanupComplete", "清理完成 — 移除了 {0} 条记录。" },
        { "Settings.CleanupFailed", "清理失败：{0}" },
        { "Settings.DatabaseCleared", "数据库已清空。" },
        { "Settings.ClearFailed", "清空失败：{0}" },

        // Folder browser
        { "Folder.SelectFolder", "选择文件夹" },
        { "Folder.EnterPath", "输入路径..." },
        { "Folder.Go", "前往" },
        { "Folder.AccessDenied", "访问被拒绝。" },
        { "Folder.DirNotFound", "目录未找到。" },
        { "Folder.NoSubdirs", "无子目录" },
        { "Folder.NoFolderSelected", "未选择文件夹" },
        { "Folder.Select", "选择" },
    };
}
