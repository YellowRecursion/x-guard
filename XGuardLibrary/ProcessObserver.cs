using System.Diagnostics;
using XGuardLibrary.Utilities;

namespace XGuardLibrary
{
    public class ProcessObserver
    {
        private readonly string _processName;
        private readonly string _currentDirectory;
        private readonly string _processPath;
        private readonly int _count;
        private readonly bool _launchAppInUserSession;

        public bool Enabled { get; set; } = true;

        public ProcessObserver(string processNameWithoutExtencion, int count, bool launchAppInUserSession)
        {
            _processName = processNameWithoutExtencion;
            _currentDirectory = AppDomain.CurrentDomain.BaseDirectory;
            _processPath = Path.Combine(_currentDirectory, _processName + ".exe");
            _count = count;
            _launchAppInUserSession = launchAppInUserSession;
        }

        public async void Run()
        {
            while (true)
            {
                try
                {
                    if (Enabled)
                    {
                        if (_launchAppInUserSession)
                        {
                            // Получаем список процессов с указанным именем
                            var processes = Process.GetProcessesByName(_processName);

                            int countOfProcesses = int.MaxValue;

                            try
                            {
                                // Получаем идентификатор сессии пользователя через процесс explorer.exe
                                uint userSessionId = ProcessExtensions.GetCurrentSessionID();
                                // Считаем процессы в той же сессии
                                countOfProcesses = processes.Count(p => p.SessionId == userSessionId);
                            }
                            catch (Exception e)
                            {
                                Logger.Error(e);
                            }

                            // Проверяем, меньше ли число процессов лимита
                            if (countOfProcesses < _count && processes.Length < 16)
                            {
                                try
                                {
                                    Logger.Info("ProcessObserver StartProcess: " + _processPath);
                                    ProcessExtensions.StartProcessAsCurrentUser(_processPath, workDir: AppDomain.CurrentDomain.BaseDirectory);
                                }
                                catch (Exception e)
                                {
                                    Logger.Error("ProcessExtensions.StartProcessAsCurrentUser: " + e.GetType() + ": " + e.Message);
                                }
                            }
                        }
                        else
                        {
                            if (Process.GetProcessesByName(_processName).Length < _count)
                            {
                                try
                                {
                                    Logger.Info("ProcessObserver StartProcess: " + _processPath);
                                    ProcessExtensions.StartProcessAsAdmin(_processPath);
                                }
                                catch (Exception e)
                                {
                                    Logger.Error("ProcessExtensions.StartProcessAsAdmin: " + e.GetType() + e.Message);
                                }
                            }
                        }
                    }
                }
                catch (Exception) { }

                await Task.Delay(80);
            }
        }
    }
}
