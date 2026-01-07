using System;

namespace Desktop_Fences.Interfaces
{
    /// <summary>
    /// Interface for managing the system tray icon.
    /// </summary>
    public interface ITrayManager : IDisposable
    {
        /// <summary>
        /// Initializes the tray icon.
        /// </summary>
        void InitializeTray();

        /// <summary>
        /// Updates the tray icon appearance.
        /// </summary>
        void UpdateTrayIcon();

        /// <summary>
        /// Updates the hidden fences menu.
        /// </summary>
        void UpdateHiddenFencesMenu();

        /// <summary>
        /// Shows or hides the tray icon.
        /// </summary>
        /// <param name="visible">True to show, false to hide.</param>
        void SetVisibility(bool visible);

        /// <summary>
        /// Registers a hidden fence with the tray manager.
        /// </summary>
        /// <param name="fenceId">The fence identifier.</param>
        /// <param name="fenceName">The fence display name.</param>
        void RegisterHiddenFence(string fenceId, string fenceName);

        /// <summary>
        /// Unregisters a hidden fence from the tray manager.
        /// </summary>
        /// <param name="fenceId">The fence identifier.</param>
        void UnregisterHiddenFence(string fenceId);
    }
}
