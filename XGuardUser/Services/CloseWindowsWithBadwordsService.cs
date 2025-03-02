using System.Runtime.InteropServices;
using System.Text;
using XGuardLibrary;

namespace XGuardUser.Services
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
            "curwabate", "chaturbate", "ポルノ", "포르노", "色情", "sèqíng", "bongacams",
            "сиськи", "голая"
        };

        [DllImport("user32.dll")]
        private static extern bool EnumWindows(EnumWindowsProc lpEnumFunc, IntPtr lParam);

        [DllImport("user32.dll")]
        private static extern int GetWindowText(IntPtr hWnd, StringBuilder lpString, int nMaxCount);

        [DllImport("user32.dll")]
        private static extern bool IsWindowVisible(IntPtr hWnd);

        [DllImport("user32.dll")]
        private static extern bool PostMessage(IntPtr hWnd, uint Msg, IntPtr wParam, IntPtr lParam);

        private const uint WM_CLOSE = 0x0010;

        private delegate bool EnumWindowsProc(IntPtr hWnd, IntPtr lParam);

        public static async void Run()
        {
            while (true)
            {
                try
                {
                    CloseMatchingWindows();
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error: {ex.Message}");
                }

                await Task.Delay(1100);
            }
        }

        private static void CloseMatchingWindows()
        {
            EnumWindows((hWnd, lParam) =>
            {
                if (IsWindowVisible(hWnd))
                {
                    StringBuilder windowText = new StringBuilder(256);
                    GetWindowText(hWnd, windowText, windowText.Capacity);
                    string title = windowText.ToString();

                    if (BadWords.Any(badWord => title.Contains(badWord, StringComparison.OrdinalIgnoreCase)))
                    {
                        Logger.Info($"Closing window: {title}");
                        PostMessage(hWnd, WM_CLOSE, IntPtr.Zero, IntPtr.Zero);
                    }
                }
                return true;
            }, IntPtr.Zero);
        }
    }
}
