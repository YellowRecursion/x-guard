using System.Diagnostics;
using System.Security.Principal;

namespace XGuard
{
    internal static class Program
    {
        public static string AppName;
        public static string AppExePath;
        public static string StarterAppName;
        public static string StarterAppExePath;
        public static string CurrentDirectory;

        private static NSFWDetector _detector;
        private static FileSystemWatcher _terminationFileWatcher;
        private static bool _showOverlay;

        public static MainForm MainForm { get; private set; }
        public static OverlayForm OverlayForm { get; private set; }
        public static Logger Logger { get; private set; }
        public static UserConfig UserConfig { get; private set; }
        public static ProgramData Data { get; private set; }
        public static bool ShowOverlay
        {
            get => _showOverlay;
            set
            {
                if (_showOverlay == value) return;
                _showOverlay = value;
                TerminationModule.IsCritical = value;
                if (value)
                {
                    OverlayForm.WindowState = FormWindowState.Maximized;
                    OverlayForm.Show();
                }
                else
                {
                    OverlayForm.Hide();
                }
            }
        }
        internal static NSFWDetector Detector => _detector;


        [STAThread]
        static void Main()
        {
            // To customize application configuration such as set high DPI settings or default font,
            // see https://aka.ms/applicationconfiguration.
            ApplicationConfiguration.Initialize();

            AppName = AppDomain.CurrentDomain.FriendlyName;
            CurrentDirectory = AppDomain.CurrentDomain.BaseDirectory;
            AppExePath = Path.Combine(CurrentDirectory, $"{AppName}.exe");

            StarterAppName = "XGuardStarter";
            StarterAppExePath = Path.Combine(CurrentDirectory, $"{StarterAppName}.exe");

            MainForm = new MainForm();
            OverlayForm = new OverlayForm();
            Logger = new Logger(MainForm.LogsTextBox);

            Application.ApplicationExit += ApplicationExit;

            Run();

            Mainloop();

            Application.Run(MainForm);
        }
        static bool IsUserAdministrator()
        {
            WindowsIdentity identity = WindowsIdentity.GetCurrent();
            WindowsPrincipal principal = new WindowsPrincipal(identity);
            return principal.IsInRole(WindowsBuiltInRole.Administrator);
        }
        private static void Run()
        {
            try
            {
                if (IsUserAdministrator()) 
                {
                    Logger.Info("У программы есть права администратора.");
                }
                else
                {
                    Logger.Info("У программы нет прав администратора.");
                }

                if (IsProcessRunning(AppName))
                {
                    //Logger.Info("XGuard is already running, close new instance");
                    //Environment.Exit(0);
                    //Logger.Info("XGuard is already running, close old instance");
                    //ProcessObserver.StopProcessBesidesThat(AppName);
                    //ProcessObserver.StartProcessAsAdmin(AppExePath);
                    //return;
                }

                Logger.Info("------ XGuard launch ------");
                Logger.Info($"Current directory: {CurrentDirectory}");

                Data = new ProgramData();
                UserConfig = new UserConfig();

                if (Data.IsFileExists())
                {
                    Logger.Info("Load data");
                    Data.Load();
                }
                else
                {
                    Logger.Info("Load user config");

                    UserConfig.Load();

                    if (string.IsNullOrWhiteSpace(UserConfig.TerminationToken) ||
                        string.IsNullOrWhiteSpace(UserConfig.BotToken) ||
                        UserConfig.ModeratorUserIds.Count == 0)
                    {
                        Logger.Info($"Before start you must prepare '{UserConfig.Filename}'");
                        return;
                    }

                    Data.TerminationTokenHash = HashingUtility.ComputeSHA256Hash(UserConfig.TerminationToken);
                    Data.BotToken = UserConfig.BotToken;
                    Data.ModeratorUserIds = UserConfig.ModeratorUserIds;

                    Data.Save();
                }

                if (File.Exists(UserConfig.FilePath))
                {
                    File.Delete(UserConfig.FilePath);
                }

                if (TerminationModule.IsTerminationFileExists())
                {
                    OnTerminate();
                    Environment.Exit(0);
                    return;
                }

                _terminationFileWatcher = new FileSystemWatcher(CurrentDirectory, TerminationModule.TERMINATION_FILENAME);
                _terminationFileWatcher.EnableRaisingEvents = true;
                _terminationFileWatcher.Created += TerminationFileCreated;

                TaskSchedulerModule.Run();

                TerminationModule.SetUnterminatableMode(true);

                _detector = new NSFWDetector();
                //_detector.DetectionLoop();
                //_detector.LogicLoop();

                Bot.Run();

                FileSafetyManager.Init();

                Logger.Info("XGuard is running");
            }
            catch (Exception ex)
            {
                Logger.Error($"[!] Global exception handler: {ex}");
                TerminationModule.SetUnterminatableMode(false);
            }
        }

        private static async void Mainloop()
        {
            while (true)
            {
                try
                {
                    if (ShowOverlay)
                    {
                        OverlayForm.Activate();
                    }
                }
                catch (Exception ex)
                {
                    Logger.Error("Mainloop exception: " + ex);
                }
                await Task.Delay(200);
            }
        }

        private static void ApplicationExit(object sender, EventArgs e)
        {
            Logger.Info("Application exit");
        }

        private static void TerminationFileCreated(object sender, FileSystemEventArgs e)
        {
            if (TerminationModule.IsTerminationFileExists())
            {
                TerminationModule.SetUnterminatableMode(false);
                OnTerminate();
                Environment.Exit(0);
            }
            else
            {
                TerminationModule.RemoveTerminationFile();
            }
        }

        private static void OnTerminate()
        {
            TaskSchedulerModule.RemoveTask(AppName);
            TerminationModule.RemoveTerminationFile();
            Logger.Info("Terminated");
        }

        private static bool IsProcessRunning(string processName)
        {
            Process[] processes = Process.GetProcessesByName(processName);
            return processes.Length > 1;
        }
    }
}