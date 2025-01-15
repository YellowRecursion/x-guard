using XGuardLibrary.Utilities;

namespace XGuardLibrary
{
    public static class TerminationService
    {
        public const string TERMINATION_FILENAME = "termination.txt";

        private static FileSystemWatcher _terminationFileWatcher;
        private static string _terminationTokenHash;

        public static event Action BeforeTermination;

        public static void Initialize(string terminationTokenHash)
        {
            _terminationTokenHash = terminationTokenHash;
            TerminationFileCreated(null, null);
            _terminationFileWatcher = new FileSystemWatcher(AppDomain.CurrentDomain.BaseDirectory, TERMINATION_FILENAME);
            _terminationFileWatcher.EnableRaisingEvents = true;
            _terminationFileWatcher.Created += TerminationFileCreated;
        }

        private static void TerminationFileCreated(object sender, FileSystemEventArgs e)
        {
            if (IsTerminationFileExists())
            {
                try
                {
                    BeforeTermination?.Invoke();
                }
                catch (Exception ex)
                {
                    Logger.Error("Termination Event throw exception: " + ex);
                }
                Environment.Exit(0);
            }
        }

        private static bool IsTerminationFileExists()
        {
            var path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, TERMINATION_FILENAME);

            if (File.Exists(path))
            {
                return HashingUtility.ComputeSHA256Hash(File.ReadAllText(path)) == _terminationTokenHash;
            }

            return false;
        }
    }
}
