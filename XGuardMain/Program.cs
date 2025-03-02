using System.Diagnostics;
using XGuard.Services;
using XGuard.Utilities;
using XGuardLibrary;
using XGuardLibrary.Utilities;

namespace XGuard
{
    public class Program
    {
        private static XGuardUser _xGuardUser;

        public static NsfwDetectionService DetectionService { get; private set; }

        public static ProgramData Data { get; private set; }

        static void Main(string[] args)
        {
            Run();
            while (true)
            {
                Thread.Sleep(1000);
            }
        }

        private static async void Run()
        {
            try
            {
                Logger.Info("------ XGuard running ------");

                if (!ProcessExtensions.IsLowestStartTimeProcess())
                {
                    Logger.Warn("Another process with a lower start time is running. Exiting current process.");
                    Environment.Exit(0);
                    return;
                }

                PrepareConfigs();

                TerminationService.BeforeTermination += OnTermination;
                TerminationService.Initialize(Data.TerminationTokenHash);

                FileSafetyService.Run();

                TaskSchedulerService.Run();

                KeepAliveService.Run();

                _xGuardUser = new XGuardUser();
                _xGuardUser.Run();
                AppDomain.CurrentDomain.ProcessExit += async (sender, e) =>
                {
                    await _xGuardUser.DisposeAsync();
                };

                DetectionService = new NsfwDetectionService(); 
                DetectionService.DetectionLoop();
                DetectionService.LogicLoop();
                DetectionService.OnLock += OnLockOrUnlock;
                DetectionService.OnUnlock += OnLockOrUnlock;
                DetectionService.OnTimer += OnLockTimerChanged;

                BotService.Run();

                Logger.Info("XGuard is running");

                await Task.Delay(-1);
            }
            catch (Exception e)
            {
                Logger.Fatal($"Global exception handler: {e}");
            }
        }

        private static async void OnLockTimerChanged()
        {
            XGuardUser.Instance.GlobalState.LockScreenTimer = -DetectionService.NoDetectionsTimer;
            await XGuardUser.Instance.SyncState();
        }

        private static async void OnLockOrUnlock()
        {
            XGuardUser.Instance.GlobalState.LockScreenTimer = -DetectionService.NoDetectionsTimer;
            await XGuardUser.Instance.SyncState();
            SystemVolumeUtilities.Mute = DetectionService.Locked;
        }

        private static void OnTermination()
        {
            TaskSchedulerUtilities.RemoveTask(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, AppDomain.CurrentDomain.FriendlyName + ".exe"));
            Logger.Info("Terminated");
        }

        private static void PrepareConfigs()
        {
            Data = new ProgramData();

            if (Data.IsFileExists())
            {
                Logger.Info("Load data");
                Data.Load();
            }
            else
            {
                Logger.Info("Data file is not found");
                Environment.Exit(0);
            }
        }
    }
}
