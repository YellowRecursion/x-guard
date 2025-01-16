using Microsoft.Win32.TaskScheduler;
using Task = Microsoft.Win32.TaskScheduler.Task;

namespace XGuardLibrary.Utilities
{
    public class TaskSchedulerUtilities
    {
        public static Task RegisterSystemTask(string taskPath, string appExePath)
        {
            TaskService.Instance.RootFolder.DeleteTask(taskPath, false);

            TaskDefinition td = TaskService.Instance.NewTask();

            var logonTrigger = new LogonTrigger();
            logonTrigger.UserId = null;
            td.Triggers.Add(logonTrigger);

            td.Actions.Add(appExePath);

            td.Settings.AllowHardTerminate = false;
            td.Settings.StopIfGoingOnBatteries = false;
            td.Settings.IdleSettings.StopOnIdleEnd = false;
            td.Settings.DisallowStartIfOnBatteries = false;
            td.Settings.MultipleInstances = TaskInstancesPolicy.IgnoreNew;
            td.Settings.DeleteExpiredTaskAfter = TimeSpan.Zero;
            td.Settings.Hidden = true;
            td.Settings.Enabled = true;
            td.Settings.RunOnlyIfIdle = false;
            td.Settings.WakeToRun = false;

            td.Principal.UserId = "SYSTEM";
            td.Principal.LogonType = TaskLogonType.ServiceAccount;
            td.Principal.RunLevel = TaskRunLevel.Highest;

            var registeredTask = TaskService.Instance.RootFolder.RegisterTaskDefinition(taskPath, td);

            return registeredTask;
        }

        public static bool IsTaskDeletedOrCorrupted(Task originalTask)
        {
            foreach (var task in TaskService.Instance.RootFolder.EnumerateTasks())
            {
                if (task.Xml == originalTask.Xml)
                {
                    if (!task.Enabled)
                    {
                        Logger.Error($"Task {task.Name} is disabled");
                        task.Enabled = true;
                        Logger.Error($"Task {task.Name} is enabled");
                    }
                    return false;
                }
            }
            return true;
        }

        public static bool HasTask(string taskName)
        {
            foreach (var task in TaskService.Instance.RootFolder.EnumerateTasks())
            {
                if (task.Name == taskName)
                {
                    return true;
                }
            }
            return false;
        }

        public static void RemoveTask(string taskPath)
        {
            TaskService.Instance.RootFolder.DeleteTask(taskPath, false);
        }
    }
}
