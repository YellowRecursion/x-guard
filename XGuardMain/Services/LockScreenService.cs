using XGuardLibrary;
using XGuardLibrary.Utilities;

namespace XGuard.Services
{
    public static class LockScreenService
    {
        private static readonly ProcessObserver _processObserver;
        private static bool _show;

        static LockScreenService()
        {
            _processObserver = new ProcessObserver("XGuardLockScreen", 1, true);
            _processObserver.Enabled = false;
            _processObserver.Run();
        }

        public static bool ShowLockScreen
        {
            get => _show;
            set
            {
                if (_show == value) return;

                _show = value;
                _processObserver.Enabled = _show;

                if (!_show)
                {
                    ProcessExtensions.KillProcesses("XGuardLockScreen");
                }
            }
        }
    }
}
