using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Desktop_Fences
{
    /// <summary>
    /// Handles all fence data persistence operations (loading, saving, migration).
    /// Centralized data management extracted from FenceManager.
    /// </summary>
    public static class FenceDataPersistence
    {
        private const string FencesFileName = "fences.json";
        private const string BackupFolderName = "backups";
        private static readonly object _saveLock = new object();

        /// <summary>
        /// Loads fence data from the JSON file.
        /// </summary>
        /// <returns>List of fence objects or empty list if file doesn't exist.</returns>
        public static List<dynamic> LoadFenceData()
        {
            try
            {
                if (!File.Exists(FencesFileName))
                {
                    LogManager.Log(LogManager.LogLevel.Info, LogManager.LogCategory.General,
                        "No fences.json found, will create default fence");
                    return new List<dynamic>();
                }

                string json = File.ReadAllText(FencesFileName);

                if (string.IsNullOrWhiteSpace(json))
                {
                    LogManager.Log(LogManager.LogLevel.Warn, LogManager.LogCategory.General,
                        "fences.json is empty");
                    return new List<dynamic>();
                }

                var fences = JsonConvert.DeserializeObject<List<dynamic>>(json);

                LogManager.Log(LogManager.LogLevel.Info, LogManager.LogCategory.General,
                    $"Loaded {fences?.Count ?? 0} fences from {FencesFileName}");

                return fences ?? new List<dynamic>();
            }
            catch (JsonException ex)
            {
                LogManager.Log(LogManager.LogLevel.Error, LogManager.LogCategory.Error,
                    $"JSON parsing error in fences.json: {ex.Message}");
                CreateCorruptedFileBackup();
                return new List<dynamic>();
            }
            catch (Exception ex)
            {
                LogManager.Log(LogManager.LogLevel.Error, LogManager.LogCategory.Error,
                    $"Error loading fences: {ex.Message}");
                return new List<dynamic>();
            }
        }

        /// <summary>
        /// Saves fence data to the JSON file.
        /// </summary>
        /// <param name="fenceData">The fence data to save.</param>
        public static void SaveFenceData(List<dynamic> fenceData)
        {
            lock (_saveLock)
            {
                try
                {
                    string json = JsonConvert.SerializeObject(fenceData, Formatting.Indented);
                    File.WriteAllText(FencesFileName, json);

                    LogManager.Log(LogManager.LogLevel.Debug, LogManager.LogCategory.FenceUpdate,
                        $"Saved {fenceData.Count} fences to {FencesFileName}");
                }
                catch (Exception ex)
                {
                    LogManager.Log(LogManager.LogLevel.Error, LogManager.LogCategory.Error,
                        $"Error saving fences: {ex.Message}");
                }
            }
        }

        /// <summary>
        /// Migrates legacy JSON format to current format.
        /// </summary>
        public static void MigrateLegacyFormat(List<dynamic> fenceData)
        {
            try
            {
                bool needsSave = false;

                foreach (var fence in fenceData)
                {
                    IDictionary<string, object> fenceDict = fence is IDictionary<string, object> dict
                        ? dict
                        : ((JObject)fence).ToObject<IDictionary<string, object>>();

                    // Ensure required fields exist
                    if (!fenceDict.ContainsKey("Id"))
                    {
                        fenceDict["Id"] = Guid.NewGuid().ToString();
                        needsSave = true;
                    }

                    if (!fenceDict.ContainsKey("ItemsType"))
                    {
                        fenceDict["ItemsType"] = "Data";
                        needsSave = true;
                    }

                    if (!fenceDict.ContainsKey("TabsEnabled"))
                    {
                        fenceDict["TabsEnabled"] = false;
                        needsSave = true;
                    }

                    if (!fenceDict.ContainsKey("IsHidden"))
                    {
                        fenceDict["IsHidden"] = false;
                        needsSave = true;
                    }

                    if (!fenceDict.ContainsKey("IsLocked"))
                    {
                        fenceDict["IsLocked"] = false;
                        needsSave = true;
                    }

                    // New Fences 6 features
                    if (!fenceDict.ContainsKey("IconTintEnabled"))
                    {
                        fenceDict["IconTintEnabled"] = false;
                        needsSave = true;
                    }

                    if (!fenceDict.ContainsKey("IconTintColor"))
                    {
                        fenceDict["IconTintColor"] = "#FFFFFF";
                        needsSave = true;
                    }

                    if (!fenceDict.ContainsKey("ClickToOpenRolled"))
                    {
                        fenceDict["ClickToOpenRolled"] = false;
                        needsSave = true;
                    }
                }

                if (needsSave)
                {
                    SaveFenceData(fenceData);
                    LogManager.Log(LogManager.LogLevel.Info, LogManager.LogCategory.General,
                        "Migrated fence data to new format");
                }
            }
            catch (Exception ex)
            {
                LogManager.Log(LogManager.LogLevel.Error, LogManager.LogCategory.Error,
                    $"Error during migration: {ex.Message}");
            }
        }

        /// <summary>
        /// Creates a backup of the corrupted fences.json file.
        /// </summary>
        public static void CreateCorruptedFileBackup()
        {
            try
            {
                if (!File.Exists(FencesFileName)) return;

                string backupPath = $"fences_corrupted_{DateTime.Now:yyyyMMdd_HHmmss}.json";
                File.Copy(FencesFileName, backupPath, true);

                LogManager.Log(LogManager.LogLevel.Warn, LogManager.LogCategory.General,
                    $"Created backup of corrupted file: {backupPath}");
            }
            catch (Exception ex)
            {
                LogManager.Log(LogManager.LogLevel.Error, LogManager.LogCategory.Error,
                    $"Error creating backup: {ex.Message}");
            }
        }

        /// <summary>
        /// Gets a fence by its ID.
        /// </summary>
        public static dynamic GetFenceById(List<dynamic> fenceData, string fenceId)
        {
            return fenceData.FirstOrDefault(f => f.Id?.ToString() == fenceId);
        }

        /// <summary>
        /// Updates a specific property of a fence.
        /// </summary>
        public static void UpdateFenceProperty(List<dynamic> fenceData, string fenceId,
            string propertyName, object value)
        {
            var fence = GetFenceById(fenceData, fenceId);
            if (fence == null) return;

            try
            {
                if (fence is JObject jObj)
                {
                    jObj[propertyName] = JToken.FromObject(value);
                }
                else if (fence is IDictionary<string, object> dict)
                {
                    dict[propertyName] = value;
                }

                SaveFenceData(fenceData);

                LogManager.Log(LogManager.LogLevel.Debug, LogManager.LogCategory.FenceUpdate,
                    $"Updated {propertyName} for fence {fenceId}");
            }
            catch (Exception ex)
            {
                LogManager.Log(LogManager.LogLevel.Error, LogManager.LogCategory.FenceUpdate,
                    $"Error updating fence property: {ex.Message}");
            }
        }

        /// <summary>
        /// Gets items from a fence, handling tabs if enabled.
        /// </summary>
        public static JArray GetFenceItems(dynamic fence)
        {
            try
            {
                bool tabsEnabled = fence.TabsEnabled?.ToString().ToLower() == "true";

                if (tabsEnabled)
                {
                    var tabs = fence.Tabs as JArray ?? new JArray();
                    int currentTab = Convert.ToInt32(fence.CurrentTab?.ToString() ?? "0");

                    if (currentTab >= 0 && currentTab < tabs.Count)
                    {
                        var activeTab = tabs[currentTab] as JObject;
                        return activeTab?["Items"] as JArray ?? new JArray();
                    }
                }

                return fence.Items as JArray ?? new JArray();
            }
            catch
            {
                return new JArray();
            }
        }

        /// <summary>
        /// Validates fence data integrity.
        /// </summary>
        public static bool ValidateFenceData(List<dynamic> fenceData)
        {
            if (fenceData == null) return false;

            foreach (var fence in fenceData)
            {
                try
                {
                    string id = fence.Id?.ToString();
                    string title = fence.Title?.ToString();

                    if (string.IsNullOrEmpty(id))
                    {
                        LogManager.Log(LogManager.LogLevel.Warn, LogManager.LogCategory.General,
                            $"Fence missing ID: {title}");
                        return false;
                    }
                }
                catch
                {
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        /// Creates the default initial fence.
        /// </summary>
        public static dynamic CreateDefaultFence(int x = 50, int y = 50)
        {
            return new
            {
                Id = Guid.NewGuid().ToString(),
                Title = "Welcome",
                X = x,
                Y = y,
                Width = 300,
                Height = 200,
                ItemsType = "Data",
                Items = new JArray(),
                Color = "#1E1E1E",
                Tint = 80,
                IsHidden = false,
                IsLocked = false,
                TabsEnabled = false,
                IconTintEnabled = false,
                IconTintColor = "#FFFFFF",
                ClickToOpenRolled = false
            };
        }
    }
}
