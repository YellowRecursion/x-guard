using Task = Microsoft.Win32.TaskScheduler.Task;
using XGuardLibrary;
using XGuardLibrary.Utilities;

namespace XGuard.Services
{
    internal static class TaskSchedulerService
    {
        private static readonly string _appName = AppDomain.CurrentDomain.FriendlyName;
        private static readonly string _appPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, _appName + ".exe");

        public static Task ScheduledTask { get; private set; }

        public static async void Run()
        {
            ScheduledTask = TaskSchedulerUtilities.RegisterSystemTask(_appName, _appPath);

            while (true)
            {
                try
                {
                    if (TaskSchedulerUtilities.IsTaskDeletedOrCorrupted(ScheduledTask))
                    {
                        Logger.Error("Scheduled task is deleted or corrupted");
                        ScheduledTask = TaskSchedulerUtilities.RegisterSystemTask(_appName, _appPath);
                        Logger.Info("Scheduled task recreated");
                    }
                }
                catch (Exception ex)
                {
                    Logger.Error("Scheduled task update error: " + ex.Message);
                }

                await System.Threading.Tasks.Task.Delay(80);
            }
        }
    }
}
