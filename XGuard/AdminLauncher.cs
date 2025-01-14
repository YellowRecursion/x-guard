using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using XGuard;

public static class AdminLauncher
{
    [DllImport("user32.dll")]
    private static extern bool SetForegroundWindow(IntPtr hWnd);

    public static async Task LaunchAsAdministratorWithFocus(string applicationPath, string arguments = "")
    {
        try
        {
            var startInfo = new ProcessStartInfo
            {
                FileName = applicationPath,
                Arguments = arguments,
                UseShellExecute = true,
                Verb = "runas"
            };

            var uacTask = Task.Run(() =>
            {
                // Периодически проверяем, появилось ли окно UAC, чтобы установить его на передний план
                while (true)
                {
                    IntPtr uacWindow = FindUACWindow();
                    if (uacWindow != IntPtr.Zero)
                    {
                        SetForegroundWindow(uacWindow);
                        break;
                    }
                    Thread.Sleep(100);
                }
            });

            Process.Start(startInfo);

            await uacTask;
        }
        catch (System.ComponentModel.Win32Exception ex) when (ex.NativeErrorCode == 1223)
        {
            Logger.Info("Запуск отменен пользователем.");
        }
        catch (Exception ex)
        {
            Logger.Error($"Ошибка запуска: {ex.Message}");
        }
    }

    private static IntPtr FindUACWindow()
    {
        foreach (Process process in Process.GetProcesses())
        {
            if (process.MainWindowTitle.Contains("User Account Control", StringComparison.OrdinalIgnoreCase))
            {
                return process.MainWindowHandle;
            }
        }
        return IntPtr.Zero;
    }
}

