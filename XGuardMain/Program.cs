using System.Diagnostics;
using XGuard.Services;
using XGuard.Utilities;
using XGuardLibrary;
using XGuardLibrary.Utilities;

namespace XGuard
{
    public class Program
    {
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

                if (!IsLowestStartTimeProcess())
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

                DetectionService = new NsfwDetectionService(); 
                DetectionService.DetectionLoop();
                DetectionService.LogicLoop();
                DetectionService.OnLock += OnLockOrUnlock;
                DetectionService.OnUnlock += OnLockOrUnlock;

                BotService.Run();

                Logger.Info("XGuard is running");

                await Task.Delay(-1);
            }
            catch (Exception e)
            {
                Logger.Fatal($"Global exception handler: {e}");
            }
        }

        private static void OnLockOrUnlock()
        {
            LockScreenService.ShowLockScreen = DetectionService.Locked;
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

        private static bool IsLowestStartTimeProcess()
        {
            string processName = Process.GetCurrentProcess().ProcessName;

            // Получаем все процессы с тем же именем
            var processes = Process.GetProcessesByName(processName);

            // Находим процесс с минимальным StartTime
            var processWithMinPid = processes.OrderBy(p => p.StartTime).FirstOrDefault();

            // Если текущий процесс совпадает с процессом с минимальным PID, вернуть true
            return processWithMinPid != null && processWithMinPid.Id == Process.GetCurrentProcess().Id;
        }
    }
}
