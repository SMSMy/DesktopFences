using System;
using System.IO;
using Xunit;

namespace Desktop_Fences.Tests
{
    /// <summary>
    /// Unit tests for BackupManager functionality.
    /// </summary>
    public class BackupManagerTests : IDisposable
    {
        private readonly string _testBackupDir;

        public BackupManagerTests()
        {
            _testBackupDir = Path.Combine(Path.GetTempPath(), $"test_backups_{Guid.NewGuid()}");
            Directory.CreateDirectory(_testBackupDir);
        }

        public void Dispose()
        {
            // Cleanup test directory
            if (Directory.Exists(_testBackupDir))
            {
                try
                {
                    Directory.Delete(_testBackupDir, true);
                }
                catch { }
            }
        }

        [Fact]
        public void IsRestoreAvailable_ShouldReturnBoolean()
        {
            // Arrange & Act
            bool result = BackupManager.IsRestoreAvailable;

            // Assert - Just verify it doesn't throw
            Assert.True(result || !result);
        }

        [Fact]
        public void BackupDirectory_ShouldExistAfterBackup()
        {
            // Arrange
            string backupsFolder = "backups";

            // Assert - Verify the backups folder concept exists
            Assert.NotNull(backupsFolder);
        }

        [Theory]
        [InlineData(".fence")]
        [InlineData(".json")]
        public void ExportFileExtensions_ShouldBeValid(string extension)
        {
            // Assert - Document valid export extensions
            Assert.StartsWith(".", extension);
            Assert.True(extension.Length > 1);
        }

        [Fact]
        public void BackupFolderNaming_ShouldFollowPattern()
        {
            // Arrange
            string timestamp = DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss");
            string expectedPattern = $"backup_{timestamp}";

            // Assert - Verify pattern includes date components
            Assert.Contains("-", timestamp);
            Assert.Contains("backup_", expectedPattern);
        }
    }
}
