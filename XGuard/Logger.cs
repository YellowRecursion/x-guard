using System;
using System.IO;

namespace XGuard
{
    public class Logger
    {
        private readonly string logDirectory;
        private readonly string logFileName;
        private readonly string logFilePath;

        private readonly RichTextBox _logTextBox;

        public Logger(RichTextBox logTextBox)
        {
            _logTextBox = logTextBox;

            logDirectory = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "logs");
            logFileName = $"{AppDomain.CurrentDomain.FriendlyName}-logs.txt";
            logFilePath = Path.Combine(logDirectory, logFileName);
        }

        public static void Info(string message)
        {
            Program.Logger.Log("INFO", message);
        }

        public static void Error(string message)
        {
            Program.Logger.Log("ERROR", message);
        }

        private void Log(string level, string message)
        {
            if (!Directory.Exists(logDirectory))
            {
                Directory.CreateDirectory(logDirectory);
            }

            string logEntry = $"{DateTime.Now:yyyy-MM-dd HH:mm:ss} [{level}] {message}";

            // Выводим лог
            _logTextBox.Text += logEntry + Environment.NewLine;

            // Записываем лог в файл
            try
            {
                File.AppendAllText(logFilePath, logEntry + Environment.NewLine);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Не удалось записать лог в файл: {ex.Message}");
            }
        }
    }

}
