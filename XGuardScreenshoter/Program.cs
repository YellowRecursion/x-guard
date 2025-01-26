using System.Drawing.Imaging;
using System.Runtime.InteropServices;
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

            try
            {
                // Удаляем старые скриншоты
                var existingFiles = Directory.GetFiles(AppDomain.CurrentDomain.BaseDirectory, "screenshot-*.png");
                for (int i = 0; i < existingFiles.Length; i++)
                {
                    File.Delete(existingFiles[i]);
                }

                // Делаем скриншоты всех экранов
                var screens = Screen.AllScreens;
                for (int i = 0; i < screens.Length; i++)
                {
                    string screenshotFilename = Path.Combine(_basePath, $"screenshot-{i}.png");
                    CaptureScreen(screens[i], screenshotFilename);
                }
            }
            catch (Exception ex)
            {
                Logger.Error($"ScreenshotHelper Error: {ex.GetType()}: {ex.Message}");
            }
        }

        public static void CaptureScreen(Screen screen, string filePath)
        {
            if (screen == null)
                throw new ArgumentNullException(nameof(screen), "Screen cannot be null.");

            if (string.IsNullOrWhiteSpace(filePath))
                throw new ArgumentException("File path cannot be null or empty.", nameof(filePath));

            try
            {
                Rectangle bounds = screen.Bounds;
                float scaleFactor = GetScreenScalingFactor(screen);

                bounds = new Rectangle()
                {
                    X = (int)(screen.Bounds.X * scaleFactor),
                    Y = (int)(screen.Bounds.Y * scaleFactor),
                    Width = (int)(screen.Bounds.Width * scaleFactor),
                    Height = (int)(screen.Bounds.Height * scaleFactor),
                };

                using (Bitmap bitmap = new Bitmap(bounds.Width, bounds.Height))
                {
                    using (Graphics graphics = Graphics.FromImage(bitmap))
                    {
                        graphics.CopyFromScreen(bounds.X, bounds.Y, 0, 0, bounds.Size);
                    }

                    bitmap.Save(filePath, ImageFormat.Png);
                }
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("Failed to capture the screen.", ex);
            }
        }

        private static float GetScreenScalingFactor(Screen screen)
        {
            DEVMODE dm = new DEVMODE();
            dm.dmSize = (short)Marshal.SizeOf(typeof(DEVMODE));
            EnumDisplaySettings(screen.DeviceName, -1, ref dm);
            var scalingFactor = (float)dm.dmPelsWidth / screen.Bounds.Width;
            return scalingFactor;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct DEVMODE
        {
            private const int CCHDEVICENAME = 0x20;
            private const int CCHFORMNAME = 0x20;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 0x20)]
            public string dmDeviceName;
            public short dmSpecVersion;
            public short dmDriverVersion;
            public short dmSize;
            public short dmDriverExtra;
            public int dmFields;
            public int dmPositionX;
            public int dmPositionY;
            public ScreenOrientation dmDisplayOrientation;
            public int dmDisplayFixedOutput;
            public short dmColor;
            public short dmDuplex;
            public short dmYResolution;
            public short dmTTOption;
            public short dmCollate;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 0x20)]
            public string dmFormName;
            public short dmLogPixels;
            public int dmBitsPerPel;
            public int dmPelsWidth;
            public int dmPelsHeight;
            public int dmDisplayFlags;
            public int dmDisplayFrequency;
            public int dmICMMethod;
            public int dmICMIntent;
            public int dmMediaType;
            public int dmDitherType;
            public int dmReserved1;
            public int dmReserved2;
            public int dmPanningWidth;
            public int dmPanningHeight;
        }

        [DllImport("user32.dll")]
        public static extern bool EnumDisplaySettings(string lpszDeviceName, int iModeNum, ref DEVMODE lpDevMode);
    }
}