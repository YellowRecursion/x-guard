namespace XGuardLibrary
{
    public static class Logger
    {
        private readonly static string logDirectory;
        private readonly static string logFileName;
        private readonly static string logFilePath;

        static Logger()
        {
            logDirectory = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "logs");
            logFileName = $"{AppDomain.CurrentDomain.FriendlyName}-logs.txt";
            logFilePath = Path.Combine(logDirectory, logFileName);
        }

        public static void Info(object message)
        {
            Log("INFO", message.ToString());
        }

        public static void Warn(object message)
        {
            Log("WARN", message.ToString());
        }

        public static void Error(object message)
        {
            Log("ERROR", message.ToString());
        }

        public static void Fatal(object message)
        {
            Log("!!! FATAL !!!", message.ToString());
        }

        private static void Log(string level, string message)
        {
            if (!Directory.Exists(logDirectory))
            {
                Directory.CreateDirectory(logDirectory);
            }

            string logEntry = $"{DateTime.Now:yyyy-MM-dd HH:mm:ss} [{level}] {message}";

            // Выводим лог
            Console.WriteLine(logEntry);

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