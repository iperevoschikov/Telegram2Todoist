using System.Text.RegularExpressions;
using JetBrains.Annotations;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram2Todoist.Functions.Todoist;
using Yandex.Cloud.Functions;

namespace Telegram2Todoist.Functions;

[PublicAPI]
public class WebHookFunctionHandler : YcFunction<WebHookFunctionHandlerRequest, WebHookFunctionHandlerResponse>
{
    public WebHookFunctionHandlerResponse FunctionHandler(WebHookFunctionHandlerRequest request, Context context)
    {
        try
        {
            HandleAsync(request).GetAwaiter().GetResult();
            return WebHookFunctionHandlerResponse.Ok();
        }
        catch (Exception e)
        {
            Console.Error.WriteLine($"Exception occurred: {e.Message}, StackTrace: {e.StackTrace}");
            return WebHookFunctionHandlerResponse.Fail();
        }
    }

    private static async Task HandleAsync(WebHookFunctionHandlerRequest request)
    {
        var serviceProvider = ContainerConfiguration.ConfigureServices();
        var telegramClient = serviceProvider.GetService<ITelegramBotClient>();
        var logger = serviceProvider.GetRequiredService<ILogger<WebHookFunctionHandler>>();
        var todoistApiClient = serviceProvider.GetRequiredService<TodoistApiClient>();
        logger.LogInformation("Received webhook update: {Update}", request.Body);
        var update = JsonConvert.DeserializeObject<Update>(request.Body)!;

        try
        {
            var inbox = (await todoistApiClient.GetProjects())
                .FirstOrDefault(p => p.IsInboxProject);

            if (inbox == null)
                throw new Exception("Inbox project not found");

            await todoistApiClient.CreateTaskAsync(
                inbox.Id,
                BuildContactName(update.Message?.ForwardFrom)
                ?? (update.Message?.ForwardSenderName == null
                    ? null
                    : Regex.Unescape(update.Message?.ForwardSenderName!))
                ?? BuildContactName(update.Message?.From)
                ?? "Задача из Telegram",
                TelegramMessageEntitiesFormatter.ToMarkdown(update.Message) ?? "Без текста сообщения",
                DateOnly.FromDateTime(DateTime.UtcNow.AddHours(5)));
        }
        catch (Exception e)
        {
            logger.LogError(e, "Exception occurred {Message}", e.Message);

            await telegramClient!.SendTextMessageAsync(
                update.Message!.Chat.Id,
                "Не смог создать задачу",
                replyToMessageId: update.Message.MessageId);
        }
    }

    [ContractAnnotation("null => null;notnull=>notnull")]
    private static string? BuildContactName(User? user)
    {
        return user == null
            ? null
            : $"{user.FirstName} {user.LastName}";
    }
}