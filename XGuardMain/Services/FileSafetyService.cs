using XGuardLibrary;

namespace XGuard.Services
{
    public static class FileSafetyService
    {
        public static readonly string BackupDirectoryName = "backup";

        private static readonly List<string> IgnoredFilesAndDirs = new List<string>
        {
            BackupDirectoryName,
            TerminationService.TERMINATION_FILENAME,
            "logs",
            "screenshot-0.png",
            "screenshot-1.png",
            "screenshot-2.png",
            "screenshot-3.png",
            "user-config.json",
        };

        private static FileSystemWatcher _fileSystemWatcher;
        private static readonly List<FileStream> _lockedFiles = new List<FileStream>();

        private static string WorkDirectory => AppDomain.CurrentDomain.BaseDirectory;

        private static string BackupDirectory => Path.Combine(WorkDirectory, BackupDirectoryName);

        public static void Run()
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
            try
            {
                var fs = new FileStream(path, FileMode.Open, FileAccess.ReadWrite, FileShare.Read);
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

            //var backupedFilePath = Path.Combine(BackupDirectory, filenameWithExtension);

            //if (File.Exists(backupedFilePath))
            //{
            //    UnlockFile(backupedFilePath);
            //    File.Delete(backupedFilePath);
            //}
        }

        public static void RemoveFileFromIgnoreList(string path)
        {
            string filenameWithExtension = Path.GetFileName(path);
            IgnoredFilesAndDirs.Remove(filenameWithExtension);
            BackupFileOrDir(path);
        }

        private static bool ShouldIgnore(string path)
        {
            path = RelativePath(path);
            return IgnoredFilesAndDirs.Exists(ignoredPath =>
            {
                // Logger.Info($"Compare: {path} AND {RelativePath(ignoredPath)}");
                return path.StartsWith(RelativePath(ignoredPath));
            });
        }

        //public static void Test()
        //{
        //    Console.WriteLine($"WorkDirectory {WorkDirectory}");

        //    tt(1,
        //        "screenshot-0.png",
        //        "C:\\Projects\\XGuard\\XGuardMain\\bin\\Debug\\net8.0\\screenshot-0.png");

        //    tt(2,
        //        "backup",
        //        "C:\\Projects\\XGuard\\XGuardMain\\bin\\Debug\\net8.0\\backup\\screenshot-0.png");

        //    tt(3,
        //        "backup",
        //        "C:\\Projects\\XGuard\\XGuardMain\\bin\\Debug\\net8.0\\backup\\");

        //    tt(4,
        //        "C:\\Projects\\XGuard\\XGuardMain\\bin\\Debug\\net8.0\\backup\\",
        //        "C:\\Projects\\XGuard\\XGuardMain\\bin\\Debug\\net8.0/backup\\");
        //}

        //private static void tt(int testNum, string a, string b)
        //{
        //    Console.WriteLine($"Test {testNum}");
        //    Console.WriteLine($"A = {a}");
        //    Console.WriteLine($"B = {b}");
        //    Console.WriteLine($"NA = {RelativePath(a)}");
        //    Console.WriteLine($"NB = {RelativePath(b)}");
        //    Console.WriteLine($"EQUAL == {RelativePath(b).StartsWith(RelativePath(a))}");

        //    Console.WriteLine("___________________________");
        //}

        private static string RelativePath(string path)
        {
            path = path.Replace('/', '\\');
            path = path.Replace(WorkDirectory, "");
            path = path.Trim('\\');
            return path;
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
