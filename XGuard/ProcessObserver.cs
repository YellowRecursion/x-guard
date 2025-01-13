using System;
using System.Diagnostics;

namespace XGuard
{
    public class ProcessObserver : IDisposable
    {
        private string _processName;
        private string _currentDirectory;
        private string _processPath;
        private bool _enabled = true;

        public ProcessObserver(string processNameWithoutExtencion)
        {
            _processName = processNameWithoutExtencion;
            _currentDirectory = AppDomain.CurrentDomain.BaseDirectory;
            _processPath = Path.Combine(_currentDirectory, _processName + ".exe");
        }

        public async void Run()
        {
            while (_enabled)
            {
                try
                {
                    if (!IsProcessRunning(_processName))
                    {
                        StartProcessAsAdmin(_processPath);
                    }
                }
                catch (Exception) { }

                await Task.Delay(200);
            }
        }

        private static bool IsProcessRunning(string processName)
        {
            return Process.GetProcessesByName(processName).Any();
        }

        public static void StartProcessAsAdmin(string filePath)
        {
            ProcessStartInfo startInfo = new ProcessStartInfo
            {
                FileName = filePath,
                Verb = "runas", // Указывает, что процесс должен быть запущен с правами администратора
                UseShellExecute = true // Необходимо для использования Verb
            };

            startInfo.CreateNoWindow = true; // Не создавать новое окно
            startInfo.WindowStyle = ProcessWindowStyle.Hidden; // Скрыть окно

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

        public static void StopProcess(string processName)
        {
            var processes = Process.GetProcessesByName(processName);

            for (int i = 0; i < processes.Length; i++)
            {
                processes[i].Kill();
            }
        }

        public static void StopProcessBesidesThat(string processName)
        {
            // Получаем текущий процесс
            var currentProcess = Process.GetCurrentProcess();

            // Получаем все процессы с заданным именем
            var processes = Process.GetProcessesByName(processName);

            foreach (var process in processes)
            {
                // Проверяем, не является ли это текущим процессом
                if (process.Id != currentProcess.Id)
                {
                    process.Kill();
                }
            }
        }

        public void Dispose()
        {
            _enabled = false;
            StopProcess(_processName);
        }
    }
}
