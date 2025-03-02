using XGuardLibrary;
using XGuardLibrary.Models.Pipes;

namespace XGuardUser.Services
{
    internal static class LockScreenService
    {
        private static readonly List<LockScreen> _lockScreens = new List<LockScreen>();
        private static bool _canClose;

        public static void Run()
        {
            XGuardMain.Instance.OnGlobalStateChanged += OnGlobalStateChanged;
            OnGlobalStateChanged(XGuardMain.Instance.GlobalState);

            Task.Run(Mainloop);
        }

        private static void OnGlobalStateChanged(GlobalState globalState)
        {
            if (globalState.Locked)
            {
                CreateLockScreensForAllMonitors();
            }
            else
            {
                CloseAllLockScreens();
            }

            for (int i = 0; i < _lockScreens.Count; i++)
            {
                _lockScreens[i].Invoke(() => _lockScreens[i].TimerLabel.Text = globalState.LockScreenTimer.ToString());
            }
        }

        private static void CreateLockScreensForAllMonitors()
        {
            if (_lockScreens.Count == Screen.AllScreens.Length) return;

            CloseAllLockScreens();

            for (int i = 0; i < Screen.AllScreens.Length; i++)
            {
                Screen screen = Screen.AllScreens[i];
                var lc = CreateLockScreen(screen);
            }
        }

        private static void CloseAllLockScreens()
        {
            _canClose = true;

            for (int i = 0; i < _lockScreens.Count; i++)
            {
                _lockScreens[i].Close();
            }

            _canClose = false;

            _lockScreens.Clear();
        }

        private static LockScreen CreateLockScreen(Screen screen)
        {
            LockScreen lockScreen = null!;

            Program.MainForm.Invoke(() =>
            {
                lockScreen = new LockScreen
                {
                    StartPosition = FormStartPosition.Manual,
                    WindowState = FormWindowState.Maximized,
                    FormBorderStyle = FormBorderStyle.None,
                    TopMost = true,
                    Location = screen.Bounds.Location,
                    Size = screen.Bounds.Size,
                };

                lockScreen.FormClosing += (sender, e) =>
                {
                    if (!_canClose)
                    {
                        e.Cancel = true;
                        ((Form)sender).Hide();
                    }
                };

                _lockScreens.Add(lockScreen);
                lockScreen.Show();
            });

            return lockScreen;
        }


        private static async void Mainloop()
        {
            while (true)
            {
                try
                {
                    if (XGuardMain.Instance.GlobalState.Locked)
                    {
                        foreach (var lockScreen in _lockScreens)
                        {
                            lockScreen.Invoke((MethodInvoker)(() =>
                            {
                                if (!lockScreen.Visible)
                                {
                                    lockScreen.Show();
                                }
                                lockScreen.WindowState = FormWindowState.Maximized;
                                lockScreen.Activate();
                            }));
                        }
                    }
                }
                catch (Exception ex)
                {
                    Logger.Error("Lock Screen Mainloop Exception: " + ex);
                }
                await Task.Delay(1200);
            }
        }
    }
}
