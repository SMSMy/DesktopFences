using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using Newtonsoft.Json.Linq;

namespace Desktop_Fences
{
    /// <summary>
    /// Manages automatic organization rules for fences.
    /// Automatically sorts and organizes files based on type, name, or custom rules.
    /// A key feature from Stardock Fences 6.
    /// </summary>
    public static class AutoOrganizeManager
    {
        /// <summary>
        /// Rule types for auto-organization.
        /// </summary>
        public enum RuleType
        {
            FileType,
            NamePattern,
            DateCreated,
            DateModified,
            Size,
            Custom
        }

        /// <summary>
        /// Represents an auto-organize rule.
        /// </summary>
        public class OrganizeRule
        {
            public string Id { get; set; } = Guid.NewGuid().ToString();
            public string Name { get; set; }
            public RuleType Type { get; set; }
            public string Pattern { get; set; }
            public string TargetFenceId { get; set; }
            public bool IsEnabled { get; set; } = true;
            public int Priority { get; set; } = 0;
        }

        private static List<OrganizeRule> _rules = new List<OrganizeRule>();
        private static readonly string RulesFileName = "organize_rules.json";

        /// <summary>
        /// Predefined file type categories.
        /// </summary>
        public static readonly Dictionary<string, string[]> FileCategories = new Dictionary<string, string[]>
        {
            { "Images", new[] { ".jpg", ".jpeg", ".png", ".gif", ".bmp", ".webp", ".svg", ".ico" } },
            { "Documents", new[] { ".doc", ".docx", ".pdf", ".txt", ".rtf", ".xls", ".xlsx", ".ppt", ".pptx", ".odt" } },
            { "Videos", new[] { ".mp4", ".mkv", ".avi", ".mov", ".wmv", ".flv", ".webm" } },
            { "Audio", new[] { ".mp3", ".wav", ".flac", ".aac", ".ogg", ".wma", ".m4a" } },
            { "Archives", new[] { ".zip", ".rar", ".7z", ".tar", ".gz", ".bz2" } },
            { "Executables", new[] { ".exe", ".msi", ".bat", ".cmd", ".ps1", ".sh" } },
            { "Code", new[] { ".cs", ".js", ".ts", ".py", ".java", ".cpp", ".c", ".h", ".html", ".css" } },
            { "Shortcuts", new[] { ".lnk", ".url" } }
        };

        /// <summary>
        /// Loads rules from file.
        /// </summary>
        public static void LoadRules()
        {
            try
            {
                if (File.Exists(RulesFileName))
                {
                    string json = File.ReadAllText(RulesFileName);
                    _rules = Newtonsoft.Json.JsonConvert.DeserializeObject<List<OrganizeRule>>(json)
                        ?? new List<OrganizeRule>();

                    LogManager.Log(LogManager.LogLevel.Info, LogManager.LogCategory.Settings,
                        $"Loaded {_rules.Count} organize rules");
                }
            }
            catch (Exception ex)
            {
                LogManager.Log(LogManager.LogLevel.Error, LogManager.LogCategory.Settings,
                    $"Error loading organize rules: {ex.Message}");
            }
        }

        /// <summary>
        /// Saves rules to file.
        /// </summary>
        public static void SaveRules()
        {
            try
            {
                string json = Newtonsoft.Json.JsonConvert.SerializeObject(_rules,
                    Newtonsoft.Json.Formatting.Indented);
                File.WriteAllText(RulesFileName, json);

                LogManager.Log(LogManager.LogLevel.Info, LogManager.LogCategory.Settings,
                    $"Saved {_rules.Count} organize rules");
            }
            catch (Exception ex)
            {
                LogManager.Log(LogManager.LogLevel.Error, LogManager.LogCategory.Settings,
                    $"Error saving organize rules: {ex.Message}");
            }
        }

        /// <summary>
        /// Gets all rules.
        /// </summary>
        public static List<OrganizeRule> GetRules() => _rules.ToList();

        /// <summary>
        /// Adds a new rule.
        /// </summary>
        public static void AddRule(OrganizeRule rule)
        {
            _rules.Add(rule);
            SaveRules();
        }

        /// <summary>
        /// Removes a rule.
        /// </summary>
        public static void RemoveRule(string ruleId)
        {
            _rules.RemoveAll(r => r.Id == ruleId);
            SaveRules();
        }

        /// <summary>
        /// Finds matching fence for a file based on rules.
        /// </summary>
        public static string FindTargetFence(string filePath)
        {
            if (string.IsNullOrEmpty(filePath)) return null;

            var enabledRules = _rules.Where(r => r.IsEnabled)
                .OrderByDescending(r => r.Priority);

            foreach (var rule in enabledRules)
            {
                if (MatchesRule(filePath, rule))
                {
                    return rule.TargetFenceId;
                }
            }

            return null;
        }

        /// <summary>
        /// Checks if a file matches a rule.
        /// </summary>
        public static bool MatchesRule(string filePath, OrganizeRule rule)
        {
            try
            {
                switch (rule.Type)
                {
                    case RuleType.FileType:
                        return MatchesFileType(filePath, rule.Pattern);

                    case RuleType.NamePattern:
                        return MatchesNamePattern(filePath, rule.Pattern);

                    case RuleType.DateCreated:
                        return MatchesDateRule(filePath, rule.Pattern, true);

                    case RuleType.DateModified:
                        return MatchesDateRule(filePath, rule.Pattern, false);

                    case RuleType.Size:
                        return MatchesSizeRule(filePath, rule.Pattern);

                    case RuleType.Custom:
                        return MatchesCustomPattern(filePath, rule.Pattern);

                    default:
                        return false;
                }
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Gets the file category for a file.
        /// </summary>
        public static string GetFileCategory(string filePath)
        {
            string ext = Path.GetExtension(filePath)?.ToLower();
            if (string.IsNullOrEmpty(ext)) return "Other";

            foreach (var category in FileCategories)
            {
                if (category.Value.Contains(ext))
                {
                    return category.Key;
                }
            }

            return "Other";
        }

        /// <summary>
        /// Auto-organizes all items in a fence based on rules.
        /// </summary>
        public static int OrganizeFence(dynamic fence, List<dynamic> allFences)
        {
            int movedCount = 0;

            try
            {
                var items = fence.Items as JArray ?? new JArray();
                var itemsToRemove = new List<JToken>();

                foreach (var item in items)
                {
                    string filename = item["Filename"]?.ToString();
                    if (string.IsNullOrEmpty(filename)) continue;

                    string targetFenceId = FindTargetFence(filename);
                    if (!string.IsNullOrEmpty(targetFenceId) &&
                        targetFenceId != fence.Id?.ToString())
                    {
                        var targetFence = allFences.FirstOrDefault(f =>
                            f.Id?.ToString() == targetFenceId);

                        if (targetFence != null)
                        {
                            // Add to target fence
                            var targetItems = targetFence.Items as JArray ?? new JArray();
                            targetItems.Add(item.DeepClone());

                            itemsToRemove.Add(item);
                            movedCount++;
                        }
                    }
                }

                // Remove moved items
                foreach (var item in itemsToRemove)
                {
                    items.Remove(item);
                }

                if (movedCount > 0)
                {
                    FenceDataManager.SaveFenceData();
                    LogManager.Log(LogManager.LogLevel.Info, LogManager.LogCategory.FenceUpdate,
                        $"Auto-organized {movedCount} items from fence '{fence.Title}'");
                }
            }
            catch (Exception ex)
            {
                LogManager.Log(LogManager.LogLevel.Error, LogManager.LogCategory.FenceUpdate,
                    $"Error auto-organizing fence: {ex.Message}");
            }

            return movedCount;
        }

        /// <summary>
        /// Creates default rules based on common file categories.
        /// </summary>
        public static void CreateDefaultRules(List<dynamic> fences)
        {
            // Create a rule for each fence that has a category-like name
            foreach (var fence in fences)
            {
                string title = fence.Title?.ToString()?.ToLower();
                if (string.IsNullOrEmpty(title)) continue;

                foreach (var category in FileCategories)
                {
                    if (title.Contains(category.Key.ToLower()))
                    {
                        var rule = new OrganizeRule
                        {
                            Name = $"Auto-sort {category.Key}",
                            Type = RuleType.FileType,
                            Pattern = string.Join(",", category.Value),
                            TargetFenceId = fence.Id?.ToString(),
                            IsEnabled = false // Disabled by default
                        };

                        if (!_rules.Any(r => r.Name == rule.Name))
                        {
                            _rules.Add(rule);
                        }
                    }
                }
            }

            SaveRules();
        }

        #region Private Helpers

        private static bool MatchesFileType(string filePath, string pattern)
        {
            string ext = Path.GetExtension(filePath)?.ToLower();
            if (string.IsNullOrEmpty(ext)) return false;

            var extensions = pattern.Split(new[] { ',', ';', ' ' },
                StringSplitOptions.RemoveEmptyEntries);

            return extensions.Any(e =>
                e.Trim().TrimStart('*').Equals(ext, StringComparison.OrdinalIgnoreCase));
        }

        private static bool MatchesNamePattern(string filePath, string pattern)
        {
            string filename = Path.GetFileName(filePath);
            return Regex.IsMatch(filename, WildcardToRegex(pattern),
                RegexOptions.IgnoreCase);
        }

        private static bool MatchesDateRule(string filePath, string pattern, bool useCreated)
        {
            try
            {
                var fileInfo = new FileInfo(filePath);
                var date = useCreated ? fileInfo.CreationTime : fileInfo.LastWriteTime;

                // Pattern format: "today", "yesterday", "thisweek", "thismonth", "days:7"
                switch (pattern.ToLower())
                {
                    case "today":
                        return date.Date == DateTime.Today;
                    case "yesterday":
                        return date.Date == DateTime.Today.AddDays(-1);
                    case "thisweek":
                        return date >= DateTime.Today.AddDays(-7);
                    case "thismonth":
                        return date.Month == DateTime.Today.Month && date.Year == DateTime.Today.Year;
                    default:
                        if (pattern.StartsWith("days:", StringComparison.OrdinalIgnoreCase))
                        {
                            int days = int.Parse(pattern.Substring(5));
                            return date >= DateTime.Today.AddDays(-days);
                        }
                        return false;
                }
            }
            catch
            {
                return false;
            }
        }

        private static bool MatchesSizeRule(string filePath, string pattern)
        {
            try
            {
                var fileInfo = new FileInfo(filePath);
                long size = fileInfo.Length;

                // Pattern format: ">1MB", "<100KB", "1MB-10MB"
                if (pattern.Contains("-"))
                {
                    var parts = pattern.Split('-');
                    long min = ParseSize(parts[0]);
                    long max = ParseSize(parts[1]);
                    return size >= min && size <= max;
                }
                else if (pattern.StartsWith(">"))
                {
                    return size > ParseSize(pattern.Substring(1));
                }
                else if (pattern.StartsWith("<"))
                {
                    return size < ParseSize(pattern.Substring(1));
                }

                return false;
            }
            catch
            {
                return false;
            }
        }

        private static bool MatchesCustomPattern(string filePath, string pattern)
        {
            try
            {
                return Regex.IsMatch(filePath, pattern, RegexOptions.IgnoreCase);
            }
            catch
            {
                return false;
            }
        }

        private static string WildcardToRegex(string pattern)
        {
            return "^" + Regex.Escape(pattern)
                .Replace("\\*", ".*")
                .Replace("\\?", ".") + "$";
        }

        private static long ParseSize(string size)
        {
            size = size.Trim().ToUpper();

            if (size.EndsWith("GB"))
                return long.Parse(size.TrimEnd('G', 'B')) * 1024 * 1024 * 1024;
            if (size.EndsWith("MB"))
                return long.Parse(size.TrimEnd('M', 'B')) * 1024 * 1024;
            if (size.EndsWith("KB"))
                return long.Parse(size.TrimEnd('K', 'B')) * 1024;

            return long.Parse(size);
        }

        #endregion
    }
}
