using System;
using System.Runtime.InteropServices;

public static class PrivilegeHelper
{
    [DllImport("advapi32.dll", SetLastError = true)]
    private static extern bool OpenProcessToken(IntPtr ProcessHandle, uint DesiredAccess, out IntPtr TokenHandle);

    [DllImport("advapi32.dll", SetLastError = true, CharSet = CharSet.Auto)]
    private static extern bool LookupPrivilegeValue(string lpSystemName, string lpName, out LUID lpLuid);

    [DllImport("advapi32.dll", SetLastError = true)]
    private static extern bool AdjustTokenPrivileges(IntPtr TokenHandle, bool DisableAllPrivileges, ref TOKEN_PRIVILEGES NewState, uint BufferLength, IntPtr PreviousState, IntPtr ReturnLength);

    [DllImport("kernel32.dll", SetLastError = true)]
    private static extern bool CloseHandle(IntPtr hObject);

    [DllImport("kernel32.dll", SetLastError = true)]
    private static extern IntPtr GetCurrentProcess();

    private const uint TOKEN_ADJUST_PRIVILEGES = 0x0020;
    private const uint TOKEN_QUERY = 0x0008;

    private const string SE_ASSIGN_PRIMARY_TOKEN_NAME = "SeAssignPrimaryTokenPrivilege";
    private const string SE_IMPERSONATE_NAME = "SeImpersonatePrivilege";

    [StructLayout(LayoutKind.Sequential)]
    private struct LUID
    {
        public uint LowPart;
        public int HighPart;
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct TOKEN_PRIVILEGES
    {
        public int PrivilegeCount;
        public LUID Luid;
        public uint Attributes;
    }

    private const uint SE_PRIVILEGE_ENABLED = 0x00000002;

    public static void EnablePrivileges()
    {
        IntPtr tokenHandle = IntPtr.Zero;
        try
        {
            // Открываем токен текущего процесса
            if (!OpenProcessToken(GetCurrentProcess(), TOKEN_ADJUST_PRIVILEGES | TOKEN_QUERY, out tokenHandle))
            {
                throw new System.ComponentModel.Win32Exception(Marshal.GetLastWin32Error(), "Failed to open process token.");
            }

            // Включаем привилегии
            EnablePrivilege(tokenHandle, SE_ASSIGN_PRIMARY_TOKEN_NAME);
            EnablePrivilege(tokenHandle, SE_IMPERSONATE_NAME);
            Console.WriteLine("Privileges successfully enabled!");
        }
        finally
        {
            if (tokenHandle != IntPtr.Zero)
            {
                CloseHandle(tokenHandle);
            }
        }
    }

    private static void EnablePrivilege(IntPtr tokenHandle, string privilegeName)
    {
        if (!LookupPrivilegeValue(null, privilegeName, out LUID luid))
        {
            throw new System.ComponentModel.Win32Exception(Marshal.GetLastWin32Error(), $"Failed to lookup privilege: {privilegeName}");
        }

        TOKEN_PRIVILEGES tp = new TOKEN_PRIVILEGES
        {
            PrivilegeCount = 1,
            Luid = luid,
            Attributes = SE_PRIVILEGE_ENABLED
        };

        if (!AdjustTokenPrivileges(tokenHandle, false, ref tp, 0, IntPtr.Zero, IntPtr.Zero))
        {
            throw new System.ComponentModel.Win32Exception(Marshal.GetLastWin32Error(), $"Failed to adjust token privileges: {privilegeName}");
        }

        if (Marshal.GetLastWin32Error() == 0)
        {
            Console.WriteLine($"Privilege {privilegeName} successfully enabled.");
        }
        else
        {
            throw new System.ComponentModel.Win32Exception(Marshal.GetLastWin32Error(), $"Failed to enable privilege: {privilegeName}");
        }
    }
}
