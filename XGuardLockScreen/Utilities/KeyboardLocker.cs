﻿using System.Diagnostics;
using System.Runtime.InteropServices;

namespace XGuardLockScreen.Utilities
{
    public class KeyboardLocker : IDisposable
    {
        private const int WH_KEYBOARD_LL = 13;
        private IntPtr _hookID = IntPtr.Zero;
        private LowLevelKeyboardProc _proc;

        public KeyboardLocker()
        {
            _proc = HookCallback;
            _hookID = SetHook(_proc);
        }

        private IntPtr SetHook(LowLevelKeyboardProc proc)
        {
            using (var curProcess = Process.GetCurrentProcess())
            using (var curModule = curProcess.MainModule)
            {
                return SetWindowsHookEx(WH_KEYBOARD_LL, proc,
                    GetModuleHandle(curModule.ModuleName), 0);
            }
        }

        private IntPtr HookCallback(int nCode, IntPtr wParam, IntPtr lParam)
        {
            if (nCode >= 0)
            {
                return (IntPtr)1; // Блокируем все клавиши
            }
            return CallNextHookEx(_hookID, nCode, wParam, lParam);
        }

        public void Dispose()
        {
            UnhookWindowsHookEx(_hookID);
        }

        // Импорты и делегаты
        [DllImport("user32.dll")]
        private static extern IntPtr SetWindowsHookEx(int idHook, LowLevelKeyboardProc lpfn, IntPtr hMod, uint dwThreadId);

        [DllImport("user32.dll")]
        private static extern bool UnhookWindowsHookEx(IntPtr hhk);

        [DllImport("user32.dll")]
        private static extern IntPtr CallNextHookEx(IntPtr hhk, int nCode, IntPtr wParam, IntPtr lParam);

        [DllImport("kernel32.dll")]
        private static extern IntPtr GetModuleHandle(string lpModuleName);

        private delegate IntPtr LowLevelKeyboardProc(int nCode, IntPtr wParam, IntPtr lParam);
    }
}
