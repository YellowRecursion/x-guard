using System.Drawing.Imaging;
using XGuardLibrary;

namespace XGuardScreenshoter
{
    internal static class Program
    {
        private static string _basePath = AppDomain.CurrentDomain.BaseDirectory;

        public static ProgramData? Data { get; private set; }

        [STAThread]
        static void Main()
        {
            ApplicationConfiguration.Initialize();

            Data = new ProgramData();

            if (Data.IsFileExists())
            {
                Data.Load();
            }

            TerminationService.Initialize(Data.TerminationTokenHash);

            CaptureScreenshots();
        }

        public static void CaptureScreenshots()
        {
            while (true)
            {
                Thread.Sleep(200);

                try
                {
                    var existingFiles = Directory.GetFiles(_basePath, "screenshot-*.png");

                    if (existingFiles.Length == 0)
                    {
                        // Делаем скриншоты всех экранов
                        var screens = Screen.AllScreens;
                        for (int i = 0; i < screens.Length; i++)
                        {
                            string screenshotFilename = Path.Combine(_basePath, $"screenshot-{i}.png");
                            CaptureScreen(screens[i], screenshotFilename);
                        }
                    }
                }
                catch (Exception ex)
                {
                    Logger.Error($"ScreenshotHelper Error: {ex.GetType()}: {ex.Message}");
                }
            }
        }

        private static void CaptureScreen(Screen screen, string filePath)
        {
            Rectangle bounds = screen.Bounds;
            using Bitmap bitmap = new Bitmap(bounds.Width, bounds.Height);
            using Graphics g = Graphics.FromImage(bitmap);

            g.CopyFromScreen(bounds.Location, Point.Empty, bounds.Size);
            bitmap.Save(filePath, ImageFormat.Png);
        }
    }
}