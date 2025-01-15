using XGuardLibrary;

namespace XGuardKeepAlive
{
    internal class Program
    {
        private static ProcessObserver _mainObserver;
        private static ProcessObserver _keepAliveObserver; 

        public static ProgramData? Data { get; private set; }

        static void Main(string[] args)
        {
            Data = new ProgramData();

            if (Data.IsFileExists())
            {
                Data.Load();
            }

            TerminationService.Initialize(Data.TerminationTokenHash);

            _mainObserver = new ProcessObserver("XGuardMain", 1, false);
            _keepAliveObserver = new ProcessObserver("XGuardKeepAlive", 2, false);

            _mainObserver.Run();
            _keepAliveObserver.Run();

            while (true)
            {
                Thread.Sleep(1000);
            }
        }
    }
}
