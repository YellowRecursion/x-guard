using Newtonsoft.Json;
using XGuardLibrary;
using XGuardLibrary.Models.Pipes;
using XGuardLibrary.Utilities;
using XGuardUser.Services;
using XGuardUser.Utilities;

namespace XGuardUser
{
    internal static class Program
    {
        private static XGuardMain? _xGuardMain;

        public static ProgramData? Data { get; private set; }
        public static MainForm MainForm { get; private set; } = null!;

        [STAThread]
        static void Main()
        {
            Logger.Info("------ XGuard running ------");

            if (!ProcessExtensions.IsLowestStartTimeProcess())
            {
                Logger.Warn("Another process with a lower start time is running. Exiting current process.");
                Environment.Exit(0);
                return;
            }

            ApplicationConfiguration.Initialize();

            Data = new ProgramData();

            if (Data.IsFileExists())
            {
                Data.Load();
            }

            TerminationService.Initialize(Data.TerminationTokenHash);

            MainForm = new MainForm();
            MainForm.FormClosing += (sender, e) =>
            {
                e.Cancel = true;
                ((Form)sender).Hide();
            };

            _xGuardMain = new XGuardMain();
            _xGuardMain.OnTask += OnTask;
            _xGuardMain.Run();

            LockScreenService.Run();

            KeyboardLockingService.Run();

            CloseWindowsWithBadwordsService.Run();

            Application.ThreadException += new ThreadExceptionEventHandler(OnException);

            ProgramApplicationContext context = new(MainForm);
            Application.Run(context);
        }

        private static void OnException(object sender, ThreadExceptionEventArgs e)
        {
            Logger.Fatal(e.Exception);
        }

        private static Task<string?> OnTask(PipeMessage task)
        {
            if (task.Name == MessageNames.TakeScreenshots)
            {
                return Task.FromResult(JsonConvert.SerializeObject(Screenshoter.TakeScreenshotsAsByteArray()));
            }

            return Task.FromResult<string?>(null);
        }
    }
}
