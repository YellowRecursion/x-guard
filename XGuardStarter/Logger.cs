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

    public static void Info(string message)
    {
        Logger.Log("INFO", message);
    }

    public static void Error(string message)
    {
        Logger.Log("ERROR", message);
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