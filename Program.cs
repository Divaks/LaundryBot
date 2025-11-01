using Telegram.Bot; 
using Telegram.Bot.Types;
using Telegram.Bot.Polling;
using System.Text;
using System.Linq;
using System;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;
using Telegram.Bot.Extensions; 
using Telegram.Bot.Types.Enums;

namespace LaundryBot
{
    public class Program
    {
        private static readonly string BotToken = ""; 
        
        private static readonly ITelegramBotClient Bot = new TelegramBotClient(BotToken);
        private static readonly MachineScraper Scraper = new MachineScraper();

        public static async Task Main()
        {
            Console.WriteLine($"Бот запущено: {await Bot.GetMeAsync()}");

            var receiverOptions = new ReceiverOptions { AllowedUpdates = { } };
            var cts = new CancellationTokenSource();

            Bot.StartReceiving(HandleUpdateAsync, HandleErrorAsync, receiverOptions: receiverOptions, cancellationToken: cts.Token);

            Console.WriteLine("Натисніть Enter для виходу");
            Console.ReadLine(); 
            cts.Cancel();
        }

        async static Task HandleUpdateAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
        {
            if (update.Message is not { Text: { } messageText } message)
                return;

            var chatId = message.Chat.Id;
            string terminal = null;

            if (messageText.StartsWith("/status113"))
            {
                terminal = "113";
            }
            else if (messageText.StartsWith("/status116"))
            {
                terminal = "116";
            }
            else if (messageText.StartsWith("/start") || messageText.StartsWith("/help"))
            {
                await botClient.SendTextMessageAsync(chatId,
                    "👋 Привіт! Я показую статус пральних машин у гуртожитку.\n" +
                    "Використовуй команди:\n" +
                    "  `/status113` - статус першого поверху\n" +
                    "  `/status116` - статус п'ятого поверху", 
                    parseMode: ParseMode.Markdown,
                    cancellationToken: cancellationToken);
                return;
            }

            if (terminal != null)
            {
                await botClient.SendTextMessageAsync(chatId,
                    $"🤖 Завантажую актуальний статус для терміналу *{terminal}*...", 
                    parseMode: ParseMode.Markdown,
                    cancellationToken: cancellationToken);

                try
                {
                    var state = await Scraper.GetTerminalStateAsync(terminal);
                    
                    var response = FormatStatuses(state);

                    await botClient.SendTextMessageAsync(
                        chatId: chatId,
                        text: response,
                        parseMode: ParseMode.Markdown,
                        cancellationToken: cancellationToken);
                }
                catch (Exception ex)
                {
                    await botClient.SendTextMessageAsync(chatId,
                        $"🚨 *Помилка*: Не вдалося отримати статус.\nПричина: {ex.Message}", 
                        parseMode: ParseMode.Markdown,
                        cancellationToken: cancellationToken);
                }
            }
            else if (messageText.StartsWith("/start") || messageText.StartsWith("/help"))
            {
                Console.WriteLine($"Отримано команду /start від {chatId}. Відправляю відповідь."); 

                await botClient.SendTextMessageAsync(chatId, 
                    "👋 Привіт! Я показую статус пральних машин у гуртожитку.\n" +
                    "Використовуй команди:\n" +
                    "  `/status113` - статус першого поверху\n" +
                    "  `/status116` - статус п'ятого поверху", 
                    parseMode: ParseMode.Markdown,
                    cancellationToken: cancellationToken);
                return;
            }
        }

        private static string FormatStatuses(TerminalState state)
        {
             var sb = new StringBuilder();
            sb.AppendLine($"🧺 *Статус пральних машин (Термінал {state.CodeName}):*");
            sb.AppendLine($"_(Оновлено: {DateTime.Now:HH:mm:ss})_");
            sb.AppendLine("---");

            if (state.WMs == null || state.WMs.Count == 0)
            {
                sb.AppendLine("Немає активних пральних машин у цьому терміналі.");
                return sb.ToString();
            }

            foreach (var wm in state.WMs.OrderBy(w => w.Number))
            {
                var statusInfo = wm.GetStatusInfo();
                var emoji = wm.IsAvailable ? "✅" : (wm.IsActive ? "⏳" : "🛑"); 
                
                sb.AppendLine($"{emoji} Машина №*{wm.Number}*: {statusInfo}");
            }

            return sb.ToString();
        }

        static Task HandleErrorAsync(ITelegramBotClient botClient, Exception exception, CancellationToken cancellationToken)
        {
            Console.WriteLine($"Помилка Polling: {exception.Message}");
            return Task.CompletedTask;
        }
    }
}