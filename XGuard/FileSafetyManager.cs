namespace XGuard
{
    public static class FileSafetyManager
    {
        public static readonly string BackupDirectoryName = "backup";

        private static readonly List<string> IgnoredFilesAndDirs = new List<string>
        {
            BackupDirectoryName,
            TerminationModule.TERMINATION_FILENAME,
            "logs",
            NSFWDetector.SCREENSHOT_FILENAME,
            NSFWDetector.SCREENSHOT_PLOT_FILENAME,
            "user-config.json",
        };

        private static readonly List<string> UnlockableFiles = new List<string>
        {
            "data.json",
        };

        private static FileSystemWatcher _fileSystemWatcher;
        private static readonly List<FileStream> _lockedFiles = new List<FileStream>();

        private static string WorkDirectory => AppDomain.CurrentDomain.BaseDirectory;

        private static string BackupDirectory => Path.Combine(WorkDirectory, BackupDirectoryName);

        public static void Init()
        {
            if (!Directory.Exists(BackupDirectory))
            {
                Directory.CreateDirectory(BackupDirectory);
            }

            MakeBackup();

            _fileSystemWatcher = new FileSystemWatcher(WorkDirectory)
            {
                IncludeSubdirectories = true,
                EnableRaisingEvents = true,
                SynchronizingObject = Program.MainForm
            };

            _fileSystemWatcher.Changed += FileChanged;
            _fileSystemWatcher.Deleted += FileDeleted;
            _fileSystemWatcher.Renamed += FileRenamed;
        }

        private static void MakeBackup()
        {
            Logger.Info($"Making backup");

            foreach (var file in Directory.GetFiles(WorkDirectory, "*", SearchOption.AllDirectories))
            {
                if (ShouldIgnore(file)) continue;
                BackupFileOrDir(file);
            }

            foreach (var dir in Directory.GetDirectories(WorkDirectory, "*", SearchOption.AllDirectories))
            {
                if (ShouldIgnore(dir)) continue;
                BackupFileOrDir(dir);
            }
        }

        private static void BackupFileOrDir(string path)
        {
            var relativePath = Path.GetRelativePath(WorkDirectory, path);
            var backupPath = Path.Combine(BackupDirectory, relativePath);

            UnlockFile(backupPath);

            if (File.Exists(path))
            {
                Directory.CreateDirectory(Path.GetDirectoryName(backupPath)!);
                File.Copy(path, backupPath, true);
                LockFile(backupPath);
            }
            else if (Directory.Exists(path))
            {
                Directory.CreateDirectory(backupPath);
            }
        }

        private static void RestoreFileOrDir(string path)
        {
            Logger.Info($"Restoring file: {path}");

            var relativePath = Path.GetRelativePath(WorkDirectory, path);
            var backupPath = Path.Combine(BackupDirectory, relativePath);

            UnlockFile(backupPath);

            if (File.Exists(backupPath))
            {
                Directory.CreateDirectory(Path.GetDirectoryName(path)!);
                File.Copy(backupPath, path, true);
                LockFile(backupPath);
            }
            else if (Directory.Exists(backupPath))
            {
                // Восстанавливаем директорию
                if (!Directory.Exists(path))
                {
                    Directory.CreateDirectory(path);
                }

                // Рекурсивно восстанавливаем содержимое директории
                foreach (var file in Directory.GetFiles(backupPath, "*", SearchOption.TopDirectoryOnly))
                {
                    var relativeFilePath = Path.GetRelativePath(BackupDirectory, file);
                    var originalFilePath = Path.Combine(WorkDirectory, relativeFilePath);
                    RestoreFileOrDir(originalFilePath);
                }

                foreach (var dir in Directory.GetDirectories(backupPath, "*", SearchOption.TopDirectoryOnly))
                {
                    var relativeDirPath = Path.GetRelativePath(BackupDirectory, dir);
                    var originalDirPath = Path.Combine(WorkDirectory, relativeDirPath);
                    RestoreFileOrDir(originalDirPath);
                }
            }
        }

        private static void LockFile(string path)
        {
            if (UnlockableFiles.Contains(Path.GetFileNameWithoutExtension(path)))
            {
                return;
            }

            try
            {
                var fs = new FileStream(path, FileMode.Open, FileAccess.ReadWrite, FileShare.None);
                _lockedFiles.Add(fs);
            }
            catch (Exception ex)
            {
                Logger.Error($"Failed to lock file {path}: {ex.Message}");
            }
        }

        private static void UnlockFile(string path)
        {
            _lockedFiles.RemoveAll(fs =>
            {
                if (fs.Name == path)
                {
                    fs.Close();
                    return true;
                }
                return false;
            });
        }

        public static void AddFileToIgnoreList(string path)
        {
            string filenameWithExtension = Path.GetFileName(path);
            if (!IgnoredFilesAndDirs.Contains(filenameWithExtension))
                IgnoredFilesAndDirs.Add(filenameWithExtension);
        }

        public static void RemoveFileFromIgnoreList(string path)
        {
            string filenameWithExtension = Path.GetFileName(path);
            IgnoredFilesAndDirs.Remove(filenameWithExtension);
            BackupFileOrDir(path);
        }

        private static bool ShouldIgnore(string path)
        {
            var relativePath = Path.GetRelativePath(WorkDirectory, path);
            return IgnoredFilesAndDirs.Exists(ignore =>
                relativePath.StartsWith(ignore, StringComparison.OrdinalIgnoreCase));
        }

        private static void FileRenamed(object sender, RenamedEventArgs e)
        {
            if (ShouldIgnore(e.OldFullPath) || ShouldIgnore(e.FullPath)) return;

            Logger.Info($"File renamed: {e.OldFullPath} -> {e.FullPath}");

            _fileSystemWatcher.EnableRaisingEvents = false;

            RestoreFileOrDir(e.FullPath);

            _fileSystemWatcher.EnableRaisingEvents = true;
        }

        private static void FileDeleted(object sender, FileSystemEventArgs e)
        {
            if (ShouldIgnore(e.FullPath)) return;

            Logger.Info($"File deleted: {e.FullPath}");

            _fileSystemWatcher.EnableRaisingEvents = false;

            RestoreFileOrDir(e.FullPath);

            _fileSystemWatcher.EnableRaisingEvents = true;
        }

        private static void FileChanged(object sender, FileSystemEventArgs e)
        {
            if (ShouldIgnore(e.FullPath)) return;

            Logger.Info($"File changed: {e.FullPath}");

            _fileSystemWatcher.EnableRaisingEvents = false;

            RestoreFileOrDir(e.FullPath);

            _fileSystemWatcher.EnableRaisingEvents = true;
        }
    }
}
