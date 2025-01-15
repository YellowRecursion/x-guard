using XGuardLibrary;

namespace XGuardLockScreen
{
    internal static class Program
    {
        private static LockScreen _lockScreen;

        public static ProgramData? Data { get; private set; }

        [STAThread]
        static void Main()
        {
            // To customize application configuration such as set high DPI settings or default font,
            // see https://aka.ms/applicationconfiguration.
            ApplicationConfiguration.Initialize();

            Data = new ProgramData();

            if (Data.IsFileExists())
            {
                Data.Load();
            }

            TerminationService.Initialize(Data.TerminationTokenHash);

            _lockScreen = new LockScreen();
            Mainloop();

            Application.Run(_lockScreen);
        }

        private static async void Mainloop()
        {
            while (true)
            {
                try
                {
                    _lockScreen.WindowState = FormWindowState.Maximized;
                    _lockScreen.Activate();
                }
                catch (Exception ex)
                {
                    Logger.Error("Lock Screen Mainloop Exception: " + ex);
                }
                await Task.Delay(200);
            }
        }
    }
}