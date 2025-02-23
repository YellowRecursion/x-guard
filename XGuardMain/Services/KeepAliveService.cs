using XGuardLibrary;

namespace XGuard.Services
{
    public static class KeepAliveService
    {
        private static readonly ProcessObserver _processObserver;

        static KeepAliveService()
        {
            _processObserver = new ProcessObserver("XGuardKeepAlive", 3, false);
           
        }

        public static void Run()
        {
            _processObserver.Run();
        }
    }
}
