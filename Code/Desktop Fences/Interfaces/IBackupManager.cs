namespace Desktop_Fences.Interfaces
{
    /// <summary>
    /// Interface for managing backups and restore operations.
    /// </summary>
    public interface IBackupManager
    {
        /// <summary>
        /// Gets whether a restore operation is available.
        /// </summary>
        bool IsRestoreAvailable { get; }

        /// <summary>
        /// Creates a backup of the current fence data.
        /// </summary>
        void BackupData();

        /// <summary>
        /// Restores the last deleted fence.
        /// </summary>
        void RestoreLastDeletedFence();

        /// <summary>
        /// Exports a fence to a file.
        /// </summary>
        /// <param name="fence">The fence to export.</param>
        void ExportFence(dynamic fence);

        /// <summary>
        /// Imports a fence from a file.
        /// </summary>
        void ImportFence();

        /// <summary>
        /// Creates a backup of a deleted fence for potential restoration.
        /// </summary>
        /// <param name="fence">The fence being deleted.</param>
        void BackupDeletedFence(dynamic fence);

        /// <summary>
        /// Restores data from a backup.
        /// </summary>
        /// <param name="backupPath">Path to the backup.</param>
        /// <param name="restoreSettings">Whether to restore settings as well.</param>
        void RestoreFromBackup(string backupPath, bool restoreSettings);

        /// <summary>
        /// Initializes the automatic backup system.
        /// </summary>
        void InitializeAutoBackup();
    }
}
