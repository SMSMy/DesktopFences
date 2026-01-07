namespace Desktop_Fences.Interfaces
{
    /// <summary>
    /// Interface for managing application settings.
    /// </summary>
    public interface ISettingsManager
    {
        /// <summary>
        /// Loads settings from the configuration file.
        /// </summary>
        void LoadSettings();

        /// <summary>
        /// Saves current settings to the configuration file.
        /// </summary>
        void SaveSettings();

        #region Properties

        /// <summary>
        /// Gets or sets whether snapping is enabled.
        /// </summary>
        bool IsSnapEnabled { get; set; }

        /// <summary>
        /// Gets or sets the tint level.
        /// </summary>
        int TintLevel { get; set; }

        /// <summary>
        /// Gets or sets the base color.
        /// </summary>
        string BaseColor { get; set; }

        /// <summary>
        /// Gets or sets whether logging is enabled.
        /// </summary>
        bool IsLoggingEnabled { get; set; }

        /// <summary>
        /// Gets or sets the launch effect.
        /// </summary>
        string LaunchEffect { get; set; }

        /// <summary>
        /// Gets or sets whether the application starts with Windows.
        /// </summary>
        bool StartWithWindows { get; set; }

        /// <summary>
        /// Gets or sets whether the tray icon is visible.
        /// </summary>
        bool ShowTrayIcon { get; set; }

        /// <summary>
        /// Gets or sets whether sounds are enabled.
        /// </summary>
        bool SoundsEnabled { get; set; }

        /// <summary>
        /// Gets or sets whether portal watermarks are shown.
        /// </summary>
        bool ShowPortalWatermark { get; set; }

        /// <summary>
        /// Gets or sets the icon size.
        /// </summary>
        int IconSize { get; set; }

        /// <summary>
        /// Gets or sets whether auto backup is enabled.
        /// </summary>
        bool AutoBackupEnabled { get; set; }

        #endregion
    }
}
