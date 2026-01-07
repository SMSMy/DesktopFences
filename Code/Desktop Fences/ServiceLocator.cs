using System;
using System.Collections.Generic;
using Desktop_Fences.Interfaces;

namespace Desktop_Fences
{
    /// <summary>
    /// Simple service locator for dependency injection.
    /// Provides centralized access to application services.
    /// </summary>
    public static class ServiceLocator
    {
        private static readonly Dictionary<Type, object> _services = new Dictionary<Type, object>();
        private static readonly Dictionary<Type, Func<object>> _factories = new Dictionary<Type, Func<object>>();
        private static readonly object _lock = new object();

        /// <summary>
        /// Registers a service instance.
        /// </summary>
        public static void Register<T>(T instance) where T : class
        {
            lock (_lock)
            {
                _services[typeof(T)] = instance;
                LogManager.Log(LogManager.LogLevel.Debug, LogManager.LogCategory.Settings,
                    $"Registered service: {typeof(T).Name}");
            }
        }

        /// <summary>
        /// Registers a service factory for lazy instantiation.
        /// </summary>
        public static void RegisterFactory<T>(Func<T> factory) where T : class
        {
            lock (_lock)
            {
                _factories[typeof(T)] = () => factory();
            }
        }

        /// <summary>
        /// Gets a registered service.
        /// </summary>
        public static T Get<T>() where T : class
        {
            lock (_lock)
            {
                var type = typeof(T);

                // Check for existing instance
                if (_services.TryGetValue(type, out var service))
                {
                    return (T)service;
                }

                // Check for factory
                if (_factories.TryGetValue(type, out var factory))
                {
                    var instance = (T)factory();
                    _services[type] = instance;
                    return instance;
                }

                throw new InvalidOperationException($"Service not registered: {type.Name}");
            }
        }

        /// <summary>
        /// Tries to get a registered service.
        /// </summary>
        public static bool TryGet<T>(out T service) where T : class
        {
            lock (_lock)
            {
                var type = typeof(T);

                if (_services.TryGetValue(type, out var obj))
                {
                    service = (T)obj;
                    return true;
                }

                if (_factories.TryGetValue(type, out var factory))
                {
                    service = (T)factory();
                    _services[type] = service;
                    return true;
                }

                service = null;
                return false;
            }
        }

        /// <summary>
        /// Checks if a service is registered.
        /// </summary>
        public static bool IsRegistered<T>() where T : class
        {
            lock (_lock)
            {
                return _services.ContainsKey(typeof(T)) || _factories.ContainsKey(typeof(T));
            }
        }

        /// <summary>
        /// Clears all registered services.
        /// </summary>
        public static void Clear()
        {
            lock (_lock)
            {
                // Dispose disposable services
                foreach (var service in _services.Values)
                {
                    if (service is IDisposable disposable)
                    {
                        try { disposable.Dispose(); } catch { }
                    }
                }

                _services.Clear();
                _factories.Clear();
            }
        }

        /// <summary>
        /// Initializes all core services.
        /// </summary>
        public static void InitializeServices()
        {
            // Register core managers as services
            Register<ISettingsManager>(new SettingsManagerWrapper());
            Register<IFenceManager>(new FenceManagerWrapper());
            Register<IBackupManager>(new BackupManagerWrapper());

            // Note: ITrayManager is registered separately when TrayManager is instantiated
            // since it requires specific initialization timing

            LogManager.Log(LogManager.LogLevel.Info, LogManager.LogCategory.Settings,
                "Service locator initialized with all core services");
        }

        /// <summary>
        /// Registers the TrayManager instance as ITrayManager service.
        /// Called from TrayManager constructor after initialization.
        /// </summary>
        public static void RegisterTrayManager(ITrayManager trayManager)
        {
            Register<ITrayManager>(trayManager);
        }

    /// <summary>
    /// Wrapper to make SettingsManager compatible with ISettingsManager interface.
    /// </summary>
    internal class SettingsManagerWrapper : ISettingsManager
    {
        public bool IsSnapEnabled
        {
            get => SettingsManager.IsSnapEnabled;
            set => SettingsManager.IsSnapEnabled = value;
        }

        public int TintLevel
        {
            get => SettingsManager.TintValue;
            set => SettingsManager.TintValue = value;
        }

        public string BaseColor
        {
            get => SettingsManager.SelectedColor;
            set => SettingsManager.SelectedColor = value;
        }

        public bool IsLoggingEnabled
        {
            get => SettingsManager.IsLogEnabled;
            set => SettingsManager.IsLogEnabled = value;
        }

        public string LaunchEffect
        {
            get => SettingsManager.LaunchEffect.ToString();
            set
            {
                if (Enum.TryParse<LaunchEffectsManager.LaunchEffect>(value, out var effect))
                    SettingsManager.LaunchEffect = effect;
            }
        }

        public bool StartWithWindows
        {
            get => RegistryHelper.IsStartupEnabled();
            set => RegistryHelper.SetStartup(value);
        }

        public bool ShowTrayIcon
        {
            get => SettingsManager.ShowInTray;
            set => SettingsManager.ShowInTray = value;
        }

        public bool SoundsEnabled
        {
            get => SettingsManager.EnableSounds;
            set => SettingsManager.EnableSounds = value;
        }

        public bool ShowPortalWatermark
        {
            get => SettingsManager.ShowBackgroundImageOnPortalFences;
            set => SettingsManager.ShowBackgroundImageOnPortalFences = value;
        }

        public int IconSize
        {
            get => SettingsManager.MaxDisplayNameLength;
            set => SettingsManager.MaxDisplayNameLength = value;
        }

        public bool AutoBackupEnabled
        {
            get => SettingsManager.EnableAutoBackup;
            set => SettingsManager.EnableAutoBackup = value;
        }

        public void LoadSettings() => SettingsManager.LoadSettings();
        public void SaveSettings() => SettingsManager.SaveSettings();
    }

    /// <summary>
    /// Wrapper to make static FenceManager compatible with IFenceManager interface.
    /// </summary>
    internal class FenceManagerWrapper : IFenceManager
    {
        /// <summary>
        /// Reloads all fences from configuration.
        /// </summary>
        public void ReloadFences() => FenceManager.ReloadFences();

        /// <summary>
        /// Gets the fence data list.
        /// </summary>
        public List<dynamic> GetFenceData() => FenceDataManager.FenceData;

        /// <summary>
        /// Creates a new fence at the specified location.
        /// </summary>
        public void CreateNewFence(string name, string type, int x, int y)
            => FenceManager.CreateNewFence(name, type, x, y);

        /// <summary>
        /// Gets the portal fences dictionary.
        /// </summary>
        public Dictionary<dynamic, PortalFenceManager> GetPortalFences()
            => FenceManager.GetPortalFences();
    }

    /// <summary>
    /// Wrapper to make static BackupManager compatible with IBackupManager interface.
    /// </summary>
    internal class BackupManagerWrapper : IBackupManager
    {
        /// <summary>
        /// Gets whether a restore operation is available.
        /// </summary>
        public bool IsRestoreAvailable => BackupManager.IsRestoreAvailable;

        /// <summary>
        /// Creates a backup of the current fence data.
        /// </summary>
        public void BackupData() => BackupManager.BackupData();

        /// <summary>
        /// Restores the last deleted fence.
        /// </summary>
        public void RestoreLastDeletedFence() => BackupManager.RestoreLastDeletedFence();

        /// <summary>
        /// Exports a fence to a file.
        /// </summary>
        public void ExportFence(dynamic fence) => BackupManager.ExportFence(fence);

        /// <summary>
        /// Imports a fence from a file.
        /// </summary>
        public void ImportFence() => BackupManager.ImportFence();

        /// <summary>
        /// Creates a backup of a deleted fence for potential restoration.
        /// </summary>
        public void BackupDeletedFence(dynamic fence) => BackupManager.BackupDeletedFence(fence);

        /// <summary>
        /// Restores data from a backup.
        /// </summary>
        public void RestoreFromBackup(string backupPath, bool restoreSettings)
            => BackupManager.RestoreFromBackup(backupPath);

        /// <summary>
        /// Initializes the automatic backup system.
        /// </summary>
        public void InitializeAutoBackup() => BackupManager.InitializeAutoBackup();
    }
    }
}

