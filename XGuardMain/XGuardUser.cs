using H.Pipes;
using H.Pipes.AccessControl;
using Newtonsoft.Json;
using System.IO.Pipes;
using System.Security.AccessControl;
using System.Security.Principal;
using Telegram.Bot.Types;
using XGuardLibrary;
using XGuardLibrary.Models.Pipes;
using XGuardLibrary.Utilities;

namespace XGuard
{
    internal sealed class XGuardUser : IAsyncDisposable
    {
        private PipeServer<PipeMessage> _server = new PipeServer<PipeMessage>("XGuard");
        private ProcessObserver _userProgramObserver = new ProcessObserver("XGuardUser", 1, true);
        private List<PipeMessage> _pipeTasks = new List<PipeMessage>();
        private List<PipeMessage> _pipeResponses = new List<PipeMessage>();

        public static XGuardUser Instance { get; private set; } = null!;
        public GlobalState GlobalState { get; private set; } = new GlobalState();

        public XGuardUser()
        {
            Instance = this;
        }

        public async ValueTask DisposeAsync()
        {
            await _server.DisposeAsync();
        }

        public async void Run()
        {
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

#pragma warning disable CA1416 // Validate platform compatibility
            var pipeSecurity = new PipeSecurity();
            pipeSecurity.AddAccessRule(new PipeAccessRule(new SecurityIdentifier(WellKnownSidType.BuiltinUsersSid, null),
                PipeAccessRights.ReadWrite, AccessControlType.Allow));
            _server.SetPipeSecurity(pipeSecurity);
#pragma warning restore CA1416 // Validate platform compatibility

            await _server.StartAsync();

            _userProgramObserver.Run();
        }

        public async Task Send(string messageId, string? data = null)
        {
            var message = new PipeMessage()
            {
                Name = messageId,
                Data = data
            };

            await _server.WriteAsync(message);

            // Logger.Info($"{message} sent");
        }

        public async Task<string?> SendAndWaitForResponse(string messageId, string? data = null)
        {
            var message = new PipeMessage()
            {
                Name = messageId,
                Data = data
            };

            _pipeTasks.Add(message);

            await _server.WriteAsync(message);

            // Logger.Info($"{message} sent. Waiting for result...");

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

            if (message == null) return;

            // Logger.Info($"{message} received");

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
            await Send(MessageNames.SyncState, data: JsonConvert.SerializeObject(GlobalState));
        }

        public async Task<SixLabors.ImageSharp.Image[]> TakeScreenshots()
        {
            var data = await SendAndWaitForResponse(MessageNames.TakeScreenshots);
            var imagesInByteFormat = JsonConvert.DeserializeObject<byte[][]>(data)!;
            return imagesInByteFormat.Select(img => img.ToImage()).ToArray();
        }
    }
}
