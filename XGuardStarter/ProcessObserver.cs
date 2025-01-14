using System.Diagnostics;
using murrayju.ProcessExtensions;

namespace XGuard
{
    public class ProcessObserver
    {
        private string _processName;
        private string _currentDirectory;
        private string _processPath;
        private int _count;
        private bool _launchAppInUserSession;

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
                    if (Process.GetProcessesByName(_processName).Length < _count)
                    {
                        Logger.Info("Start: " + _processPath);
                        if (_launchAppInUserSession)
                        {
                            try
                            {
                                //ProcessLauncher.LaunchInteractiveProcess(_processPath);
                                ProcessExtensions.StartProcessAsCurrentUser(_processPath, workDir: AppDomain.CurrentDomain.BaseDirectory);
                                //UserSessionLauncher.LaunchProcessInUserSession(_processPath);
                            }
                            catch (Exception e)
                            {
                                Logger.Error(e.GetType() + e.Message);
                            }
                            
                        }
                        else
                        {
                            StartProcessAsAdmin(_processPath);
                        }
                    }
                }
                catch (Exception) { }

                await Task.Delay(100);
            }
        }

        private void StartProcessAsAdmin(string filePath)
        {
            ProcessStartInfo startInfo = new ProcessStartInfo
            {
                FileName = filePath,
                Verb = "runas", // Указывает, что процесс должен быть запущен с правами администратора
                UseShellExecute = true // Необходимо для использования Verb
            };

            if (_count > 1)
            {
                startInfo.CreateNoWindow = true; // Не создавать новое окно
                startInfo.WindowStyle = ProcessWindowStyle.Hidden; // Скрыть окно
            }

            try
            {
                Process.Start(startInfo);
            }
            catch (System.ComponentModel.Win32Exception)
            {
                // Обработка случая, когда пользователь отменяет запрос на повышение прав
                Console.WriteLine("Запуск отменен пользователем.");
            }
            catch (Exception ex)
            {
                // Обработка других исключений
                Console.WriteLine($"Произошла ошибка: {ex.Message}");
            }
        }
    }
}
