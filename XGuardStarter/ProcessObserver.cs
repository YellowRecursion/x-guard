using System.Diagnostics;

namespace XGuard
{
    public class ProcessObserver
    {
        private string _processName;
        private string _currentDirectory;
        private string _processPath;
        private int _count;

        public ProcessObserver(string processNameWithoutExtencion, int count)
        {
            _processName = processNameWithoutExtencion;
            _currentDirectory = AppDomain.CurrentDomain.BaseDirectory;
            _processPath = Path.Combine(_currentDirectory, _processName + ".exe");
            _count = count;
        }

        public async void Run()
        {
            while (true)
            {
                try
                {
                    if (Process.GetProcessesByName(_processName).Length < _count)
                    {
                        StartProcessAsAdmin(_processPath);
                    }
                }
                catch (Exception) { }

                await Task.Delay(50);
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
