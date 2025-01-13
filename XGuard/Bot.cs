using Telegram.Bot.Polling;
using Telegram.Bot;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;
using System.Text;

namespace XGuard
{
    internal static class Bot
    {
        // Это клиент для работы с Telegram Bot API, который позволяет отправлять сообщения, управлять ботом, подписываться на обновления и многое другое.
        private static ITelegramBotClient _botClient;
        // Это объект с настройками работы бота. Здесь мы будем указывать, какие типы Update мы будем получать, Timeout бота и так далее.
        private static ReceiverOptions _receiverOptions;

        private static bool _inited = false;

        public static async void Run()
        {
            while (true)
            {
                Init();
                await Task.Delay(2000);
            }
        }

        public static void Init()
        {
            if (_inited) return;

            try
            {
                _botClient = new TelegramBotClient(Program.Data.BotToken); // Присваиваем нашей переменной значение, в параметре передаем Token, полученный от BotFather
                _receiverOptions = new ReceiverOptions // Также присваем значение настройкам бота
                {
                    AllowedUpdates = new[] // Тут указываем типы получаемых Update`ов, о них подробнее расказано тут https://core.telegram.org/bots/api#update
                    {
                    UpdateType.Message, // Сообщения (текст, фото/видео, голосовые/видео сообщения и т.д.)
                    UpdateType.InlineQuery,
                    UpdateType.CallbackQuery,
                },
                    // Параметр, отвечающий за обработку сообщений, пришедших за то время, когда ваш бот был оффлайн
                    // DropPendingUpdates = false,
                    ThrowPendingUpdates = true,
                };

                using var cts = new CancellationTokenSource();
                
                // UpdateHander - обработчик приходящих Update`ов
                // ErrorHandler - обработчик ошибок, связанных с Bot API
                _botClient.StartReceiving(UpdateHandler, ErrorHandler, _receiverOptions, cts.Token); // Запускаем бота
             
                // var me = await _botClient.GetMe(); // Создаем переменную, в которую помещаем информацию о нашем боте.
                Logger.Info($"Bot run");

                _inited = true;
            }
            catch (Exception)
            {
                // Logger.Error("Error on init bot: " + ex.Message);
            }
        }

       
        private static async Task UpdateHandler(ITelegramBotClient client, Update update, CancellationToken token)
        {
            // Обязательно ставим блок try-catch, чтобы наш бот не "падал" в случае каких-либо ошибок
            try
            {
                // Сразу же ставим конструкцию switch, чтобы обрабатывать приходящие Update
                switch (update.Type)
                {
                    case UpdateType.Message:
                        {
                            var message = update.Message; // эта переменная будет содержать в себе все связанное с сообщениями
                            var user = message.From; // From - это от кого пришло сообщение (или любой другой Update)
                            var chat = message.Chat;   // Chat - содержит всю информацию о чате

                            if (message.Text == "/start")
                            {
                                Program.Data.ChatIds.Add(chat.Id);
                                Program.Data.Save();

                                if (Program.Data.ModeratorUserIds.Contains(user.Id))
                                {
                                    await client.SendTextMessageAsync(chat.Id, $"Привет модератор {user.Username}! Вы подписаны на уведомления от XGuard.");
                                }
                                else
                                {
                                    await client.SendTextMessageAsync(chat.Id, $"Привет пользователь {user.Username}! Вы подписаны на уведомления от XGuard.");
                                }
                            }

                            if (message.Text.StartsWith("/pause") && await IsModerator(client, chat, user))
                            {
                                var timer = int.MaxValue;

                                try
                                {
                                    timer = int.Parse(message.Text.Split(" ")[1]) * 60;
                                }
                                catch (Exception)
                                { }

                                Program.Detector.DisableTimer = timer;

                                if (timer < int.MaxValue)
                                {
                                    await client.SendTextMessageAsync(chat.Id, $"XGuard поставлен на паузу на {timer / 60} мин.");
                                }
                                else
                                {
                                    await client.SendTextMessageAsync(chat.Id, $"XGuard поставлен на паузу до перезагрузки ПК");
                                }
                            }

                            if (message.Text == "/continue")
                            {
                                Program.Detector.DisableTimer = 0;
                                await client.SendTextMessageAsync(chat.Id, $"XGuard возобновлён");
                            }

                            if (message.Text == "/status")
                            {
                                var sb = new StringBuilder();
                                sb.AppendLine($"Статус XGuard ({(Program.Detector.DisableTimer > 0 ? "приостановлен" : "работает")}):");
                                sb.AppendLine($"Таймер до возобновления работы: {Program.Detector.DisableTimer} с.");
                                sb.AppendLine($"Время без обнаружений NSFW: {Program.Detector.NoDetectionsTimer} с.");
                                sb.AppendLine($"Переодичность скриншотов: {Program.Detector.DetectionRate} мс.");

                                await client.SendTextMessageAsync(chat.Id, sb.ToString());
                            }

                            if (message.Text == "/help")
                            {
                                var helpText = new StringBuilder();
                                helpText.AppendLine("Доступные команды:");
                                helpText.AppendLine("/start - Подписаться на уведомления от XGuard.");
                                helpText.AppendLine("/pause [минуты] - Поставить XGuard на паузу на указанное количество минут (только для модераторов).");
                                helpText.AppendLine("/continue - Возобновить работу XGuard.");
                                helpText.AppendLine("/status - Узнать текущий статус XGuard.");
                                helpText.AppendLine("/help - Получить список доступных команд.");

                                await client.SendTextMessageAsync(chat.Id, helpText.ToString());
                            }
                            return;
                        }
                    case UpdateType.CallbackQuery:
                        {
                            var callbackQuery = update.CallbackQuery; // Переменная, которая будет содержать в себе всю информацию о кнопке, которую нажали
                            var user = callbackQuery.From;
                            var chat = callbackQuery.Message.Chat;

                            await client.AnswerCallbackQueryAsync(callbackQuery.Id);

                            if (callbackQuery.Data == "unblock" && await IsModerator(client, chat, user))
                            {
                                Program.Detector.NoDetectionsTimer = Program.Detector.NoDetectionsTimer < 0 ? 0 : Program.Detector.NoDetectionsTimer;
                                await client.SendTextMessageAsync(chat.Id, "XGuard разблокирован");
                            }
                        }
                        return;
                }
            }
            catch (Exception exception)
            {
                Logger.Error($"Bot UpdateHandler Exception: {exception.Message}");
            }
        }

        //private static async Task ErrorHandler(ITelegramBotClient client, Exception exception, HandleErrorSource source, CancellationToken token)
        //{
        //    Logger.Error($"Bot ErrorHandler Exception: {exception.Message}");
        //    await Task.CompletedTask;
        //}
        private static async Task ErrorHandler(ITelegramBotClient client, Exception exception, CancellationToken token)
        {
            Logger.Error($"Bot ErrorHandler Exception: {exception.Message}");
            await Task.CompletedTask;
        }


        public static async Task SendNsfsNotification(string screenshotPath)
        {
            await Task.Delay(1000);

            var chatIds = new List<long>(Program.Data.ChatIds);

            for (int i = 0; i < 2; i++)
            {
                try
                {
                    if (!_inited) throw new Exception("Bot is not inited");

                    Logger.Info($"Send NSFS notification to users (try {i + 1})");

                    var inlineKeyboard = new InlineKeyboardMarkup(InlineKeyboardButton.WithCallbackData("Разблокировать", "unblock"));
                    Telegram.Bot.Types.Message firstMessage = null;

                    using (Stream stream = System.IO.File.OpenRead(screenshotPath))
                    {
                        foreach (var chatId in chatIds)
                        {
                            var isModer = Program.Data.ModeratorUserIds.Contains(chatId);
                            var replyMarkup = isModer ? inlineKeyboard : null;
                            var caption = isModer ? "Обнаружен NSFS контент. Посмотрите на отправленный скриншот и примите решение на разблокировку" : "Обнаружен NSFS контент.";

                            if (firstMessage == null)
                            {
                                 firstMessage = await _botClient.SendPhotoAsync(chatId, photo: InputFile.FromStream(stream, "image.png"), hasSpoiler: true, caption: caption, replyMarkup: replyMarkup);
                            }
                            else
                            {
                                 await _botClient.SendPhotoAsync(chatId, photo: InputFile.FromFileId(firstMessage.Photo.First().FileId), hasSpoiler: true, caption: caption, replyMarkup: replyMarkup);
                            }

                            Logger.Info($"NSFS notification sent to chat {chatId}");
                        }
                    }

                    break;
                }
                catch (Exception ex)
                {
                    Logger.Error("Error on send nsfs telegram message: " + ex.Message);
                    await Task.Delay(3000);
                }
            }
        }

        private static async Task<bool> IsModerator(ITelegramBotClient client, Chat chat, User user)
        {
            if (Program.Data.ModeratorUserIds.Contains(user.Id))
            {
                return true;
            }
            else
            {
                await client.SendTextMessageAsync(chat.Id, $"У вас недостаточно прав, сообщите модератору, только он может выполнить данную команду");
                return false;
            }
        }
    }
}
