using H.Pipes;
using Newtonsoft.Json;
using XGuardLibrary;
using XGuardLibrary.Models.Pipes;

namespace XGuardUser
{
    internal sealed class XGuardMain : IAsyncDisposable
    {
        private PipeClient<PipeMessage> _client = new PipeClient<PipeMessage>("XGuard");

        public static XGuardMain Instance { get; private set; } = null!;
        public event Func<PipeMessage, Task<string?>>? OnTask;
        public event Action<GlobalState>? OnGlobalStateChanged;
        public GlobalState GlobalState { get; private set; } = new GlobalState();

        public XGuardMain()
        {
            Instance = this;

            _client.Connected += (o, args) => Logger.Info($"Connected to Main program");
            _client.Disconnected += (o, args) => Logger.Warn($"Disconnected from Main program");
            _client.MessageReceived += MessageReceived;
            _client.ExceptionOccurred += (o, args) => Logger.Error("PipeClient ExceptionOccurred: " + args.Exception);

            OnTask += OnNewTask;
        }

        public async ValueTask DisposeAsync()
        {
            await _client.DisposeAsync();
        }

        public async void Run()
        {
            await _client.ConnectAsync();
        }

        private async void MessageReceived(object? sender, H.Pipes.Args.ConnectionMessageEventArgs<PipeMessage?> e)
        {
            var message = e.Message;
            if (message == null) return;
            // Logger.Info($"{message} received");
            if (OnTask == null) return;
            var response = await OnTask.Invoke(message);
            if (response != null) await Reply(message, response);
        }

        public async Task Send(string messageId, string? data = null)
        {
            var message = new PipeMessage()
            {
                Name = messageId,
                Data = data,
            };

            await _client.WriteAsync(message);

            // Logger.Info($"{message} sent");
        }

        public async Task Reply(PipeMessage message, string response)
        {
            message = new PipeMessage(message.Id)
            {
                Name = message.Name,
                Data = response,
            };

            await _client.WriteAsync(message);

            // Logger.Info($"Reply to {message} sent");
        }

        private Task<string?> OnNewTask(PipeMessage task)
        {
            if (task.Name == MessageNames.SyncState)
            {
                var gs = JsonConvert.DeserializeObject<GlobalState>(task.Data!);

                if (gs != null)
                {
                    GlobalState = gs;
                    OnGlobalStateChanged?.Invoke(GlobalState);
                }
            }

            return Task.FromResult<string?>(null);
        }
    }
}
