using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;

public class ProcessLauncher
{
    [DllImport("advapi32.dll", SetLastError = true)]
    private static extern bool OpenProcessToken(IntPtr ProcessHandle, uint DesiredAccess, out IntPtr TokenHandle);

    [DllImport("kernel32.dll", SetLastError = true)]
    private static extern bool CloseHandle(IntPtr hObject);

    [DllImport("advapi32.dll", SetLastError = true)]
    private static extern bool DuplicateTokenEx(
        IntPtr hExistingToken,
        uint dwDesiredAccess,
        IntPtr lpTokenAttributes,
        SECURITY_IMPERSONATION_LEVEL ImpersonationLevel,
        TOKEN_TYPE TokenType,
        out IntPtr phNewToken);

    [DllImport("advapi32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
    private static extern bool CreateProcessAsUser(
        IntPtr hToken,
        string lpApplicationName,
        string lpCommandLine,
        IntPtr lpProcessAttributes,
        IntPtr lpThreadAttributes,
        bool bInheritHandles,
        uint dwCreationFlags,
        IntPtr lpEnvironment,
        string lpCurrentDirectory,
        ref STARTUPINFO lpStartupInfo,
        out PROCESS_INFORMATION lpProcessInformation);

    [StructLayout(LayoutKind.Sequential)]
    private struct STARTUPINFO
    {
        public uint cb;
        public string lpReserved;
        public string lpDesktop;
        public string lpTitle;
        public uint dwX;
        public uint dwY;
        public uint dwXSize;
        public uint dwYSize;
        public uint dwXCountChars;
        public uint dwYCountChars;
        public uint dwFillAttribute;
        public uint dwFlags;
        public short wShowWindow;
        public short cbReserved2;
        public IntPtr lpReserved2;
        public IntPtr hStdInput;
        public IntPtr hStdOutput;
        public IntPtr hStdError;
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct PROCESS_INFORMATION
    {
        public IntPtr hProcess;
        public IntPtr hThread;
        public uint dwProcessId;
        public uint dwThreadId; 
    }

    private enum SECURITY_IMPERSONATION_LEVEL
    {
        SecurityAnonymous,
        SecurityIdentification,
        SecurityImpersonation,
        SecurityDelegation
    }

    private enum TOKEN_TYPE
    {
        TokenPrimary = 1,
        TokenImpersonation
    }

    private const uint TOKEN_QUERY = 0x0008;
    private const uint TOKEN_DUPLICATE = 0x0002;
    private const uint TOKEN_ASSIGN_PRIMARY = 0x0001;
    private const uint STARTF_USESHOWWINDOW = 0x00000001;
    private const uint CREATE_UNICODE_ENVIRONMENT = 0x00000400;
    private const uint CREATE_NEW_CONSOLE = 0x00000010;

    public static void LaunchInteractiveProcess(string appPath)
    {
        try
        {
            // Находим процесс Explorer
            var explorer = Process.GetProcessesByName("explorer").FirstOrDefault();
            if (explorer == null)
            {
                Logger.Error("Explorer process not found. Ensure a user is logged in.");
                return;
            }

            // Открываем токен процесса
            if (!OpenProcessToken(explorer.Handle, TOKEN_DUPLICATE | TOKEN_ASSIGN_PRIMARY | TOKEN_QUERY, out IntPtr token))
            {
                Logger.Error($"Failed to open process token. Error: {Marshal.GetLastWin32Error()}");
                return;
            }

            try
            {
                // Дублируем токен
                if (!DuplicateTokenEx(token, TOKEN_ASSIGN_PRIMARY | TOKEN_QUERY | TOKEN_DUPLICATE, IntPtr.Zero,
                    SECURITY_IMPERSONATION_LEVEL.SecurityImpersonation, TOKEN_TYPE.TokenPrimary, out IntPtr duplicatedToken))
                {
                    Logger.Error($"Failed to duplicate token. Error: {Marshal.GetLastWin32Error()}");
                    return;
                }

                try
                {
                    // Настраиваем запуск нового процесса
                    var startupInfo = new STARTUPINFO
                    {
                        cb = (uint)Marshal.SizeOf(typeof(STARTUPINFO)),
                        lpDesktop = "winsta0\\default"
                    };

                    var processInfo = new PROCESS_INFORMATION();

                    // Запускаем процесс
                    if (!CreateProcessAsUser(
                        duplicatedToken,
                        null,
                        appPath,
                        IntPtr.Zero,
                        IntPtr.Zero,
                        false,
                        CREATE_UNICODE_ENVIRONMENT | CREATE_NEW_CONSOLE,
                        IntPtr.Zero,
                        null,
                        ref startupInfo,
                        out processInfo))
                    {
                        Logger.Error($"Failed to create process. Error: {Marshal.GetLastWin32Error()}");
                        return;
                    }

                    Logger.Info($"Process started successfully! PID: {processInfo.dwProcessId}");
                }
                finally
                {
                    CloseHandle(duplicatedToken);
                }
            }
            finally
            {
                CloseHandle(token);
            }
        }
        catch (Exception ex)
        {
            Logger.Error($"Unexpected error: {ex.Message}");
        }
    }

}






public class UserSessionLauncher
{
    [DllImport("advapi32.dll", SetLastError = true)]
    private static extern bool OpenProcessToken(IntPtr ProcessHandle, uint DesiredAccess, out IntPtr TokenHandle);

    [DllImport("kernel32.dll", SetLastError = true)]
    private static extern IntPtr OpenProcess(uint processAccess, bool bInheritHandle, int processId);

    [DllImport("advapi32.dll", SetLastError = true)]
    private static extern bool DuplicateTokenEx(
        IntPtr hExistingToken,
        uint dwDesiredAccess,
        IntPtr lpTokenAttributes,
        int ImpersonationLevel,
        int TokenType,
        out IntPtr phNewToken);

    [DllImport("userenv.dll", SetLastError = true)]
    private static extern bool CreateEnvironmentBlock(
        out IntPtr lpEnvironment,
        IntPtr hToken,
        bool bInherit);

    [DllImport("advapi32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
    private static extern bool CreateProcessAsUser(
        IntPtr hToken,
        string lpApplicationName,
        string lpCommandLine,
        IntPtr lpProcessAttributes,
        IntPtr lpThreadAttributes,
        bool bInheritHandles,
        uint dwCreationFlags,
        IntPtr lpEnvironment,
        string lpCurrentDirectory,
        ref STARTUPINFO lpStartupInfo,
        out PROCESS_INFORMATION lpProcessInformation);

    [DllImport("kernel32.dll", SetLastError = true)]
    private static extern bool CloseHandle(IntPtr hObject);

    [StructLayout(LayoutKind.Sequential)]
    private struct STARTUPINFO
    {
        public uint cb;
        public string lpReserved;
        public string lpDesktop;
        public string lpTitle;
        public uint dwX;
        public uint dwY;
        public uint dwXSize;
        public uint dwYSize;
        public uint dwXCountChars;
        public uint dwYCountChars;
        public uint dwFillAttribute;
        public uint dwFlags;
        public short wShowWindow;
        public short cbReserved2;
        public IntPtr lpReserved2;
        public IntPtr hStdInput;
        public IntPtr hStdOutput;
        public IntPtr hStdError;
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct PROCESS_INFORMATION
    {
        public IntPtr hProcess;
        public IntPtr hThread;
        public uint dwProcessId;
        public uint dwThreadId;
    }

    public static void LaunchProcessInUserSession(string appPath)
    {
        const uint TOKEN_DUPLICATE = 0x0002;
        const uint TOKEN_QUERY = 0x0008;
        const uint TOKEN_ASSIGN_PRIMARY = 0x0001;
        const uint TOKEN_ADJUST_PRIVILEGES = 0x0020;
        const uint TOKEN_ADJUST_SESSIONID = 0x0100;

        const uint CREATE_UNICODE_ENVIRONMENT = 0x00000400;

        const int SECURITY_IMPERSONATION = 2;
        const int TOKEN_TYPE_PRIMARY = 1;

        // Получаем процесс Explorer
        Process explorer = Process.GetProcessesByName("explorer")[0];

        // Открываем токен процесса Explorer
        IntPtr processHandle = OpenProcess(0x001F0FFF, false, explorer.Id);
        if (processHandle == IntPtr.Zero)
        {
            throw new Exception("Failed to open process Explorer. Error: " + Marshal.GetLastWin32Error());
        }

        IntPtr tokenHandle;
        if (!OpenProcessToken(processHandle, TOKEN_DUPLICATE | TOKEN_QUERY, out tokenHandle))
        {
            CloseHandle(processHandle);
            throw new Exception("Failed to get process token. Error: " + Marshal.GetLastWin32Error());
        }

        // Дублируем токен
        IntPtr duplicatedToken;
        if (!DuplicateTokenEx(
            tokenHandle,
            TOKEN_ASSIGN_PRIMARY | TOKEN_QUERY | TOKEN_DUPLICATE | TOKEN_ADJUST_SESSIONID | TOKEN_ADJUST_PRIVILEGES,
            IntPtr.Zero,
            SECURITY_IMPERSONATION,
            TOKEN_TYPE_PRIMARY,
            out duplicatedToken))
        {
            CloseHandle(tokenHandle);
            CloseHandle(processHandle);
            throw new Exception("Failed to duplicate token. Error: " + Marshal.GetLastWin32Error());
        }

        // Создаем окружение
        IntPtr environmentBlock;
        if (!CreateEnvironmentBlock(out environmentBlock, duplicatedToken, true))
        {
            CloseHandle(duplicatedToken);
            CloseHandle(tokenHandle);
            CloseHandle(processHandle);
            throw new Exception("Failed to create environment block. Error: " + Marshal.GetLastWin32Error());
        }

        // Настраиваем STARTUPINFO
        STARTUPINFO si = new STARTUPINFO();
        si.cb = (uint)Marshal.SizeOf(typeof(STARTUPINFO));
        si.lpDesktop = "winsta0\\default";

        // Запускаем процесс
        PROCESS_INFORMATION pi;
        if (!CreateProcessAsUser(
            duplicatedToken,
            null,
            appPath,
            IntPtr.Zero,
            IntPtr.Zero,
            false,
            CREATE_UNICODE_ENVIRONMENT,
            environmentBlock,
            null,
            ref si,
            out pi))
        {
            CloseHandle(duplicatedToken);
            CloseHandle(tokenHandle);
            CloseHandle(processHandle);
            throw new Exception("Failed to create process. Error: " + Marshal.GetLastWin32Error());
        }

        // Закрываем дескрипторы
        CloseHandle(pi.hProcess);
        CloseHandle(pi.hThread);
        CloseHandle(duplicatedToken);
        CloseHandle(tokenHandle);
        CloseHandle(processHandle);
    }
}

