namespace XGuard
{
    internal class Program
    {
        public const string TERMINATION_FILENAME = "termination.txt";

        public static string? CurrentDirectory;
        private static ProcessObserver? _processObserver1;
        private static ProcessObserver? _processObserver2;
        private static FileSystemWatcher? _terminationFileWatcher;

        public static ProgramData? Data { get; private set; }

        static void Main(string[] args)
        {
            CurrentDirectory = AppDomain.CurrentDomain.BaseDirectory;

            Data = new ProgramData();

            if (Data.IsFileExists())
            {
                Data.Load();
            }

            if (IsTerminationFileExists())
            {
                Environment.Exit(0);
            }

            Logger.Info("--- XGUARD STARTER RUN ---");

            _terminationFileWatcher = new FileSystemWatcher(AppDomain.CurrentDomain.BaseDirectory, TERMINATION_FILENAME);
            _terminationFileWatcher.EnableRaisingEvents = true;
            _terminationFileWatcher.Created += TerminationFileCreated;

            //try
            //{
            //    PrivilegeHelper.EnablePrivileges();
            //}
            //catch (Exception e)
            //{
            //    Logger.Error(e.ToString());
            //}

            _processObserver1 = new ProcessObserver("XGuard", 1, true);
            _processObserver1.Run();

            //_processObserver2 = new ProcessObserver("XGuardStarter", 2, false);
            //_processObserver2.Run();

            while (true)
            {
                Thread.Sleep(200);
            }
        }

        private static void TerminationFileCreated(object sender, FileSystemEventArgs e)
        {
            if (IsTerminationFileExists())
            {
                Environment.Exit(0);
            }
        }

        public static bool IsTerminationFileExists()
        {
            var path = Path.Combine(Program.CurrentDirectory, TERMINATION_FILENAME);

            if (File.Exists(path))
            {
                return HashingUtility.ComputeSHA256Hash(File.ReadAllText(path)) == Program.Data.TerminationTokenHash;
            }

            return false;
        }
    }
}
