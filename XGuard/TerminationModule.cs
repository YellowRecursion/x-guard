using System.Diagnostics;
using System.Runtime.InteropServices;

namespace XGuard
{
    internal class TerminationModule
    {
        public const string TERMINATION_FILENAME = "termination.txt";
        public const string TERMINATION_TOKEN = "RmIngeniChounitChMeARAdefiSabdEs";

        [DllImport("ntdll.dll", SetLastError = true)]
        private static extern int NtSetInformationProcess(IntPtr hProcess, int processInformationClass, ref int processInformation, int processInformationLength);

        //https://undocumented.ntinternals.net/index.html?page=UserMode%2FUndocumented%20Functions%2FNT%20Objects%2FProcess%2FPROCESS_INFORMATION_CLASS.html

        private static ProcessObserver _processObserver;
        private static bool _isCritical;

        public static bool IsTerminatable { get; private set; } = true;

        public static bool IsCritical
        {
            get => _isCritical;
            set
            {
                if (_isCritical == value) return;
                _isCritical = value;
                critical(value ? -1 : 0);
            }
        }

        private static void critical(int status)
        {
            int BreakOnTermi = 0x1D;  //breakoftermination value
            //https://undocumented.ntinternals.net/index.html?page=UserMode%2FUndocumented%20Functions%2FNT%20Objects%2FProcess%2FPROCESS_INFORMATION_CLASS.html
            Process.EnterDebugMode();
            NtSetInformationProcess(Process.GetCurrentProcess().Handle, BreakOnTermi, ref status, sizeof(int));
        }

        public static void SetUnterminatableMode(bool enabled)
        {
            if (enabled) Logger.Info("Unterminatable mode enabled");
            else Logger.Info("Unterminatable mode disabled");
            // critical(enabled ? -1 : 0);
            IsTerminatable = !enabled;

            //if (enabled)
            //{
            //    _processObserver = new ProcessObserver("XGuardStarter");
            //    _processObserver.Run();
            //}
            //else if (_processObserver != null)
            //{
            //    _processObserver.Dispose();
            //    _processObserver = null;
            //}
        }

        public static string GetTerminationFilePath()
        {
            return Path.Combine(Program.CurrentDirectory, TERMINATION_FILENAME);
        }

        public static void RemoveTerminationFile()
        {
            File.Delete(Path.Combine(Program.CurrentDirectory, TERMINATION_FILENAME));
            Logger.Info("Termination file removed");
        }

        public static bool IsTerminationFileExists()
        {
            var path = Path.Combine(Program.CurrentDirectory, TERMINATION_FILENAME);

            if (File.Exists(path))
            {
                return HashingUtility.ComputeSHA256Hash(File.ReadAllText(path)) == Program.Data.TerminationTokenHash;
            }

            return false;
        }
    }
}
