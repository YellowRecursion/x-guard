using XGuardLibrary;

namespace XGuardLockScreen
{
    internal static class Program
    {
        private static readonly List<LockScreen> _lockScreens = new List<LockScreen>();
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

            CreateLockScreensForAllMonitors();
            Mainloop();

            Application.Run();
        }

        private static void CreateLockScreensForAllMonitors()
        {
            foreach (var screen in Screen.AllScreens)
            {
                var lockScreen = new LockScreen
                {
                    StartPosition = FormStartPosition.Manual,
                    WindowState = FormWindowState.Maximized,
                    FormBorderStyle = FormBorderStyle.None,
                    TopMost = true,
                    Location = screen.Bounds.Location,
                    Size = screen.Bounds.Size
                };

                // ��������� ������� Closing ��� �������������� �������� ����
                lockScreen.FormClosing += (sender, e) =>
                {
                    e.Cancel = true; // �������� �������� �����
                    ((Form)sender).Hide(); // �������� �����
                };

                _lockScreens.Add(lockScreen);
                lockScreen.Show();
            }
        }

        private static async void Mainloop()
        {
            while (true)
            {
                try
                {
                    foreach (var lockScreen in _lockScreens)
                    {
                        // ���������, ���� ����� ���� ������ ��� �������, ��������������� ��
                        if (!lockScreen.Visible)
                        {
                            lockScreen.Show();
                        }

                        lockScreen.WindowState = FormWindowState.Maximized;
                        lockScreen.Activate();
                    }
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
