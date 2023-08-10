using System.Runtime.CompilerServices;
using Telegram.Bot;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Polling;
using Telegram.Bot.Requests;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;

const string TELEGRAM_TOKEN = "6137553444:AAFcCKjDO4GAYgy-7AC3CVSFcytnv4kFWIE";
const long MY_CHAT_ID = 438889695;
bool sentToMeMode = false;

var botClient = new TelegramBotClient(TELEGRAM_TOKEN);
using var cts = new CancellationTokenSource();

var receivedOptions = new ReceiverOptions()
{
    AllowedUpdates = { }
};

botClient.StartReceiving(HandleUpdateAsync, HandleErrorAsync, receivedOptions, cts.Token);

var me = await botClient.GetMeAsync();
Console.WriteLine($"Starting with @{ me.Username }");

await Task.Delay(10000000);

async Task HandleUpdateAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
{
    InlineKeyboardMarkup main = new InlineKeyboardMarkup(new[]
    {
        new[] { InlineKeyboardButton.WithUrl(text: "Come to my account", url: "t.me/ntl_yo") },
        new[] { InlineKeyboardButton.WithCallbackData(text: "Push message for me", callbackData: "send") }
    });

    InlineKeyboardMarkup back = new InlineKeyboardMarkup(new[]
    {
        new[] { InlineKeyboardButton.WithCallbackData(text: "<< Back to Main", callbackData: "back") }
    });

    if (update.Type == UpdateType.Message && update.Message.Type == MessageType.Text)
    {
        var chatId = update.Message.Chat.Id;
        var messageId = update.Message.MessageId;
        var messageText = update.Message.Text;
        string firstName = update?.Message?.From?.FirstName ?? "NO DATA";

        Console.WriteLine($"Incoming message [{messageText}] in chat #{chatId} from @{update.Message.From.Username} when {DateTime.Now}");

        if (messageText == "/start")
        {
            Message sentMessage = await botClient.SendTextMessageAsync(
                chatId: chatId,
                text: $"Hello {firstName}👽! I'm Bot to gather messages.",
                replyMarkup: main,
                cancellationToken: cancellationToken);
        }

        if (sentToMeMode)
        {
            Message sentMessage = await botClient.SendTextMessageAsync(
                chatId: chatId,
                text: $"I have got your message. Thank you love <3",
                replyMarkup: back,
                cancellationToken: cancellationToken);

            Message sentMessageToMe = await botClient.SendTextMessageAsync(
                chatId: MY_CHAT_ID,
                text: messageText + Environment.NewLine + $"Message from @{update.Message.From.Username}",
                cancellationToken: cancellationToken);
        }
    }

    if (update.CallbackQuery != null)
    {
        if (update.CallbackQuery.Data == "send")
        {
            Message sentMessage = await botClient.EditMessageTextAsync(
                messageId: update.CallbackQuery.Message.MessageId,
                chatId: update.CallbackQuery.Message.Chat.Id,
                text: $"Write something funny to me! {update?.Message?.From?.Username}",
                replyMarkup: back,
                cancellationToken: cancellationToken);
            sentToMeMode = true;
        }

        if (update.CallbackQuery.Data == "back")
        {
            Message sentMessage = await botClient.EditMessageTextAsync(
                messageId: update.CallbackQuery.Message.MessageId,
                chatId: update.CallbackQuery.Message.Chat.Id,
                text: $"Hello {update.CallbackQuery.From.FirstName}👽! I'm Bot to gather messages.",
                replyMarkup: main,
                cancellationToken: cancellationToken);
            sentToMeMode = false;
        }
    }
}

Task HandleErrorAsync(ITelegramBotClient botClient, Exception exception, CancellationToken cancellationToken)
{
    var errorMessage = exception switch
    {
        ApiRequestException apiRequestException => $"Telegram API Error: [{apiRequestException.ErrorCode}]\n{apiRequestException.Message}",
        _ => exception.ToString()
    };
    Console.WriteLine(errorMessage);
    return Task.CompletedTask;
}
