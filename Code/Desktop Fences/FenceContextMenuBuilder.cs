using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using Newtonsoft.Json.Linq;

namespace Desktop_Fences
{
    /// <summary>
    /// Handles building and managing context menus for fences.
    /// Extracted from FenceManager for better code organization.
    /// </summary>
    public static class FenceContextMenuBuilder
    {
        /// <summary>
        /// Builds the heart (menu) ContextMenu for a fence with consistent items and dynamic state.
        /// </summary>
        /// <param name="fence">The fence to build the menu for.</param>
        /// <param name="showTabsOption">Whether to show the tabs option.</param>
        /// <returns>A configured ContextMenu.</returns>
        public static ContextMenu BuildHeartContextMenu(dynamic fence, bool showTabsOption = false)
        {
            LogManager.Log(LogManager.LogLevel.Info, LogManager.LogCategory.UI,
                $"Building heart context menu for fence '{fence.Title}'");

            var menu = new ContextMenu();

            // --- AUTO-CLOSE TIMER ---
            SetupAutoCloseTimer(menu);

            // About item
            AddMenuItem(menu, "About...", () => AboutFormManager.ShowAboutForm());

            // Options item
            AddMenuItem(menu, "Options...", () => OptionsFormManager.ShowOptionsForm());

            menu.Items.Add(new Separator());

            // New Fence items
            AddNewFenceMenuItems(menu);

            menu.Items.Add(new Separator());

            // Tabs Option (for Data fences)
            AddTabsOption(menu, fence);

            // Delete this fence
            AddDeleteFenceOption(menu, fence);

            menu.Items.Add(new Separator());

            // Export/Import Group
            AddExportImportOptions(menu, fence);

            menu.Items.Add(new Separator());

            // Exit item
            AddMenuItem(menu, "Exit", () => Application.Current.Shutdown());

            return menu;
        }

        /// <summary>
        /// Builds a basic context menu for icons.
        /// </summary>
        public static ContextMenu BuildBasicIconContextMenu(StackPanel sp, dynamic item, string filePath)
        {
            var menu = new ContextMenu();

            var removeItem = new MenuItem { Header = "Remove" };
            removeItem.Click += (s, e) => RemoveIconFromFence(sp, filePath);
            menu.Items.Add(removeItem);

            AddAlwaysRunAsAdminOption(menu, sp, item, filePath);

            return menu;
        }

        #region Private Helpers

        private static void SetupAutoCloseTimer(ContextMenu menu)
        {
            var menuTimer = new System.Windows.Threading.DispatcherTimer
            {
                Interval = TimeSpan.FromSeconds(4)
            };

            menuTimer.Tick += (s, e) =>
            {
                if (menu.IsOpen && !menu.IsMouseOver)
                {
                    menu.IsOpen = false;
                    LogManager.Log(LogManager.LogLevel.Debug, LogManager.LogCategory.UI, "Heart Menu auto-closed by timer");
                }
                menuTimer.Stop();
            };

            menu.Opened += (s, e) => menuTimer.Start();
            menu.Closed += (s, e) => menuTimer.Stop();
            menu.MouseEnter += (s, e) => menuTimer.Stop();
            menu.MouseLeave += (s, e) => { menuTimer.Stop(); menuTimer.Start(); };
        }

        private static void AddMenuItem(ContextMenu menu, string header, Action action)
        {
            var item = new MenuItem { Header = header };
            item.Click += (s, e) => action();
            menu.Items.Add(item);
        }

        private static void AddNewFenceMenuItems(ContextMenu menu)
        {
            var newFenceItem = new MenuItem { Header = "New Fence" };
            newFenceItem.Click += (s, e) =>
            {
                var mousePosition = System.Windows.Forms.Cursor.Position;
                FenceManager.CreateNewFence("", "Data", mousePosition.X, mousePosition.Y);
            };
            menu.Items.Add(newFenceItem);

            var newPortalFenceItem = new MenuItem { Header = "New Portal Fence" };
            newPortalFenceItem.Click += (s, e) =>
            {
                var mousePosition = System.Windows.Forms.Cursor.Position;
                FenceManager.CreateNewFence("New Portal Fence", "Portal", mousePosition.X, mousePosition.Y);
            };
            menu.Items.Add(newPortalFenceItem);

            var newNoteFenceItem = new MenuItem { Header = "New Note Fence" };
            newNoteFenceItem.Click += (s, e) =>
            {
                var mousePosition = System.Windows.Forms.Cursor.Position;
                FenceManager.CreateNewFence("", "Note", mousePosition.X, mousePosition.Y);
            };
            menu.Items.Add(newNoteFenceItem);
        }

        private static void AddTabsOption(ContextMenu menu, dynamic fence)
        {
            bool isDataFence = fence.ItemsType?.ToString() == "Data";
            if (!isDataFence) return;

            bool tabsEnabled = fence.TabsEnabled?.ToString().ToLower() == "true";

            var enableTabsItem = new MenuItem
            {
                Header = "Enable Tabs On This Fence",
                IsCheckable = true,
                IsChecked = tabsEnabled
            };

            enableTabsItem.Click += (s, e) => FenceManager.ToggleFenceTabs(fence);
            menu.Items.Add(enableTabsItem);
            menu.Items.Add(new Separator());
        }

        private static void AddDeleteFenceOption(ContextMenu menu, dynamic fence)
        {
            var deleteThisFence = new MenuItem { Header = "Delete this Fence" };
            deleteThisFence.Click += (s, e) =>
            {
                bool result = MessageBoxesManager.ShowCustomMessageBoxForm();
                if (result)
                {
                    if (SettingsManager.ExportShortcutsOnFenceDeletion && fence.ItemsType?.ToString() == "Data")
                    {
                        FenceIconHandler.ExportAllIconsToDesktop(fence, false);
                    }

                    BackupManager.BackupDeletedFence(fence);
                    FenceDataManager.FenceData.Remove(fence);
                    FenceDataManager.SaveFenceData();

                    // Close the window
                    var windows = Application.Current.Windows.OfType<NonActivatingWindow>();
                    var win = windows.FirstOrDefault(w => w.Tag?.ToString() == fence.Id?.ToString());
                    win?.Close();

                    FenceManager.UpdateAllHeartContextMenus();
                }
            };
            menu.Items.Add(deleteThisFence);
        }

        private static void AddExportImportOptions(ContextMenu menu, dynamic fence)
        {
            var exportItem = new MenuItem { Header = "Export this Fence" };
            exportItem.Click += (s, e) => BackupManager.ExportFence(fence);
            menu.Items.Add(exportItem);

            var importItem = new MenuItem { Header = "Import a Fence..." };
            importItem.Click += (s, e) => BackupManager.ImportFence();
            menu.Items.Add(importItem);

            var restoreItem = new MenuItem
            {
                Header = "Restore Last Deleted Fence",
                Visibility = BackupManager.IsRestoreAvailable ? Visibility.Visible : Visibility.Collapsed
            };
            restoreItem.Click += (s, e) => BackupManager.RestoreLastDeletedFence();
            menu.Items.Add(restoreItem);
        }

        private static void RemoveIconFromFence(StackPanel sp, string filePath)
        {
            try
            {
                var parentWin = VisualTreeHelperExtensions.FindVisualParent<NonActivatingWindow>(sp);
                if (parentWin == null) return;

                string fenceId = parentWin.Tag?.ToString();
                var fence = FenceDataManager.FenceData.FirstOrDefault(f => f.Id?.ToString() == fenceId);

                if (fence == null) return;

                var items = fence.Items as JArray;
                var itemToRemove = items?.FirstOrDefault(i => i["Filename"]?.ToString() == filePath);

                if (itemToRemove != null)
                {
                    items.Remove(itemToRemove);
                    FenceDataManager.SaveFenceData();

                    var wrapPanel = VisualTreeHelperExtensions.FindVisualParent<WrapPanel>(sp);
                    wrapPanel?.Children.Remove(sp);
                }
            }
            catch (Exception ex)
            {
                LogManager.Log(LogManager.LogLevel.Error, LogManager.LogCategory.FenceCreation,
                    $"Error removing icon: {ex.Message}");
            }
        }

        private static void AddAlwaysRunAsAdminOption(ContextMenu menu, StackPanel sp, dynamic item, string filePath)
        {
            bool alwaysAdmin = Convert.ToBoolean(item["AlwaysRunAsAdmin"] ?? false);

            var miAlwaysAdmin = new MenuItem
            {
                Header = "Always run as administrator",
                IsCheckable = true,
                IsChecked = alwaysAdmin
            };

            miAlwaysAdmin.Click += (s, e) =>
            {
                try
                {
                    var parentWindow = VisualTreeHelperExtensions.FindVisualParent<NonActivatingWindow>(sp);
                    if (parentWindow == null) return;

                    string fenceId = parentWindow.Tag?.ToString();
                    if (string.IsNullOrEmpty(fenceId)) return;

                    var currentFence = FenceDataManager.FenceData.FirstOrDefault(f => f.Id?.ToString() == fenceId);
                    if (currentFence == null || currentFence.ItemsType?.ToString() != "Data") return;

                    var items = GetActiveItems(currentFence);
                    var matchingItem = FindItemByPath(items, filePath);

                    if (matchingItem != null)
                    {
                        bool currentValue = Convert.ToBoolean(matchingItem["AlwaysRunAsAdmin"] ?? false);
                        bool newValue = !currentValue;
                        matchingItem["AlwaysRunAsAdmin"] = newValue;
                        miAlwaysAdmin.IsChecked = newValue;
                        FenceDataManager.SaveFenceData();

                        LogManager.Log(LogManager.LogLevel.Info, LogManager.LogCategory.UI,
                            $"Toggled AlwaysRunAsAdmin to {newValue} for: {filePath}");
                    }
                }
                catch (Exception ex)
                {
                    LogManager.Log(LogManager.LogLevel.Error, LogManager.LogCategory.UI,
                        $"Error toggling AlwaysRunAsAdmin: {ex.Message}");
                }
            };

            // Refresh IsChecked on menu open
            menu.Opened += (s, ev) =>
            {
                try
                {
                    var parentWindow = VisualTreeHelperExtensions.FindVisualParent<NonActivatingWindow>(sp);
                    if (parentWindow == null) return;

                    string fenceId = parentWindow.Tag?.ToString();
                    var currentFence = FenceDataManager.FenceData.FirstOrDefault(f => f.Id?.ToString() == fenceId);

                    if (currentFence == null) return;

                    var items = GetActiveItems(currentFence);
                    var matchingItem = FindItemByPath(items, filePath);

                    if (matchingItem != null)
                    {
                        miAlwaysAdmin.IsChecked = Convert.ToBoolean(matchingItem["AlwaysRunAsAdmin"] ?? false);
                    }
                }
                catch (Exception ex)
                {
                    LogManager.Log(LogManager.LogLevel.Warn, LogManager.LogCategory.UI,
                        $"Error refreshing AlwaysRunAsAdmin: {ex.Message}");
                }
            };

            menu.Items.Add(miAlwaysAdmin);
        }

        private static JArray GetActiveItems(dynamic fence)
        {
            var items = fence.Items as JArray ?? new JArray();
            bool tabsEnabled = fence.TabsEnabled?.ToString().ToLower() == "true";

            if (tabsEnabled)
            {
                var tabs = fence.Tabs as JArray ?? new JArray();
                int currentTabIndex = Convert.ToInt32(fence.CurrentTab?.ToString() ?? "0");

                if (currentTabIndex >= 0 && currentTabIndex < tabs.Count)
                {
                    var currentTab = tabs[currentTabIndex] as JObject;
                    items = currentTab?["Items"] as JArray ?? items;
                }
            }

            return items;
        }

        private static dynamic FindItemByPath(JArray items, string filePath)
        {
            return items.FirstOrDefault(i => string.Equals(
                System.IO.Path.GetFullPath(i["Filename"]?.ToString() ?? ""),
                System.IO.Path.GetFullPath(filePath),
                StringComparison.OrdinalIgnoreCase));
        }

        #endregion
    }

    /// <summary>
    /// Helper class for visual tree operations.
    /// </summary>
    public static class VisualTreeHelperExtensions
    {
        /// <summary>
        /// Finds a parent of the specified type in the visual tree.
        /// </summary>
        public static T FindVisualParent<T>(DependencyObject child) where T : DependencyObject
        {
            DependencyObject parentObject = System.Windows.Media.VisualTreeHelper.GetParent(child);

            if (parentObject == null) return null;
            if (parentObject is T parent) return parent;

            return FindVisualParent<T>(parentObject);
        }
    }
}
