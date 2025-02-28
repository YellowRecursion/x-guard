using System.Diagnostics;
using XGuardLibrary;

namespace XGuard.Services
{
    public static class CloseWindowsWithBadwordsService
    {
        private static readonly List<string> BadWords = new()
        {
            "porn", "erotic", "sex", "xxx",
            "секc", "порно", "эротика", "интим", "голые", "18+", "onlyfans",
            "erotico", "seks", "nudity", "naked",
            "érotique", "nude",
            "poruno", "poleuno", "зщкт", "gjhy", "порн",
            "curwabate", "chaturbate", "ポルノ", "포르노", "色情", "sèqíng"
        };

        public static async void Run()
        {
            while (true)
            {
                try
                {
                    KillMatchingProcesses();
                }
                catch (Exception ex)
                {
                    Logger.Error($"Error: {ex.Message}");
                }

                await Task.Delay(1100);
            }
        }

        private static void KillMatchingProcesses()
        {
            var processes = Process.GetProcesses();

            foreach (var process in processes)
            {
                try
                {
                    if (BadWords.Any(badWord => process.ProcessName.Contains(badWord, StringComparison.OrdinalIgnoreCase)))
                    {
                        Logger.Info($"Killing process: {process.ProcessName} (ID: {process.Id})");
                        process.Kill();
                    }
                }
                catch (Exception ex)
                {
                    Logger.Error($"Failed to kill process {process.ProcessName}: {ex.Message}");
                }
            }
        }
    }
}
