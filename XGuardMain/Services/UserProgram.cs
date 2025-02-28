using H.Pipes;
using XGuardLibrary;
using XGuardLibrary.Models.Pipes;
using XGuardLibrary.Utilities;

namespace XGuard.Services
{
    public class UserProgram
    {
        private PipeServer<PipeMessage> _server;
        private ProcessObserver _userProgramObserver = new ProcessObserver("XGuardUser", 1, true);
        private List<PipeMessage> _pipeTasks = new List<PipeMessage>();
        private List<PipeMessage> _pipeResponses = new List<PipeMessage>();

        public static UserProgram Instance { get; private set; }
        public GlobalState GlobalState { get; private set; } = new GlobalState();

        ~UserProgram()
        {
            _server.DisposeAsync().GetAwaiter().GetResult();
        }

        public async void Run()
        {
            Instance = this;

            _server = new PipeServer<PipeMessage>("XGuard");

            _server.ClientConnected += async (o, args) =>
            {
                Logger.Info($"Client {args.Connection.PipeName} is now connected!");
                await SyncState();
                foreach (var task in _pipeTasks)
                {
                    await _server.WriteAsync(task);
                }
            };

            _server.ClientDisconnected += (o, args) =>
            {
                Logger.Warn($"Client {args.Connection.PipeName} disconnected");
            };

            _server.MessageReceived += MessageReceived;

            _server.ExceptionOccurred += (o, args) => Logger.Error("PipeServer ExceptionOccurred: " + args.Exception);

            await _server.StartAsync();
        }

        public async Task Send(string messageId, object data = null)
        {
            var message = new PipeMessage()
            {
                Name = messageId,
                Data = data
            };

            await _server.WriteAsync(message);
        }

        public async Task<object> SendAndWaitForResponse(string messageId, object data = null)
        {
            var message = new PipeMessage()
            {
                Name = messageId,
                Data = data
            };

            _pipeTasks.Add(message);

            await _server.WriteAsync(message);

            while (!_pipeResponses.Any(m => m.Id == message.Id))
            {
                await Task.Delay(50);
            }

            var response = _pipeResponses.First(m => m.Id == message.Id);

            _pipeResponses.Remove(response);

            return response.Data;
        }

        private void MessageReceived(object? sender, H.Pipes.Args.ConnectionMessageEventArgs<PipeMessage?> e)
        {
            var message = e.Message;

            for (int i = 0; i < _pipeTasks.Count; i++)
            {
                if (_pipeTasks[i].Id == message.Id)
                {
                    _pipeTasks.Remove(_pipeTasks[i]);
                    _pipeResponses.Add(message);
                    break;
                }
            }
        }

        public async Task SyncState()
        {
            await Send(MessageNames.SyncState, data: GlobalState);
        }

        public async Task<SixLabors.ImageSharp.Image[]> TakeScreenshots()
        {
            var data = await SendAndWaitForResponse(MessageNames.TakeScreenshots);
            var imagesInByteFormat = (byte[][])data;
            return imagesInByteFormat.Select(img => img.ToImage()).ToArray();
        }
    }
}
