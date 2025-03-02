using XGuardLibrary.Models.Pipes;
using XGuardUser.Utilities;

namespace XGuardUser.Services
{
    internal static class KeyboardLockingService
    {
        private static KeyboardLocker? _locker;

        public static void Run()
        {
            XGuardMain.Instance.OnGlobalStateChanged += OnGlobalStateChanged;
            OnGlobalStateChanged(XGuardMain.Instance.GlobalState);
        }

        private static void OnGlobalStateChanged(GlobalState globalState)
        {
            if (Application.OpenForms.Count == 0 || !Application.OpenForms[0]!.IsHandleCreated)
            {
                return;
            }

            Program.MainForm.Invoke(() =>
            {
                if (globalState.Locked)
                {
                    _locker ??= new KeyboardLocker();
                }
                else if (_locker != null)
                {
                    _locker.Dispose();
                    _locker = null;
                }
            });
        }
    }
}
