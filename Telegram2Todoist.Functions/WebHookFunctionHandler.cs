using System.Text.RegularExpressions;
using Google.Cloud.Firestore;
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
public partial class WebHookFunctionHandler : YcFunction<WebHookFunctionHandlerRequest, WebHookFunctionHandlerResponse>
{
    private const string TodoistApiTokenFieldName = "todoist_api_token";

    public WebHookFunctionHandlerResponse FunctionHandler(WebHookFunctionHandlerRequest request, Context context)
    {
        try
        {
            HandleAsync(request).GetAwaiter().GetResult();
            return WebHookFunctionHandlerResponse.Ok();
        }
        catch (Exception e)
        {
            Console
                .Error
                .WriteLine($"Exception occurred: {e.Message}, StackTrace: {e.StackTrace}");
            return WebHookFunctionHandlerResponse.Fail();
        }
    }

    private static async Task HandleAsync(WebHookFunctionHandlerRequest request)
    {
        var serviceProvider = ContainerConfiguration.ConfigureServices();
        var telegramClient = serviceProvider.GetRequiredService<ITelegramBotClient>();
        var firestoreDb = serviceProvider.GetRequiredService<FirestoreDb>();
        var logger = serviceProvider.GetRequiredService<ILogger<WebHookFunctionHandler>>();
        var todoistServiceFactory = serviceProvider.GetRequiredService<TodoistServiceFactory>();
        logger.LogInformation("Received webhook update: {Update}", request.Body);

        var update = JsonConvert.DeserializeObject<Update>(request.Body)!;
        var message = update.Message;

        try
        {
            if (message == null)
                throw new NullReferenceException("Message is null");
            if (message.From?.Id == null)
                throw new NullReferenceException("From is null");

            var userId = message.From.Id.ToString();
            var userDocumentReference = firestoreDb
                .Collection("users")
                .Document(userId);

            var user = await userDocumentReference.GetSnapshotAsync();

            if (user == null)
            {
                var probablyToken = message.Text;
                if (!string.IsNullOrEmpty(probablyToken)
                    && await IsValidTodoistApiToken(
                        serviceProvider.GetRequiredService<TodoistApiClientFactory>(),
                        probablyToken))
                {
                    await userDocumentReference.SetAsync(new Dictionary<string, object>
                    {
                        [TodoistApiTokenFieldName] = probablyToken,
                    });

                    await telegramClient.SendTextMessageAsync(
                        message.Chat.Id,
                        "Токен успешно сохранён",
                        replyToMessageId: message.MessageId);

                    return;
                }

                await telegramClient.SendTextMessageAsync(message.Chat.Id,
                    "Для работы необходим api-токен todoist.\n" +
                    "Получить токен можно так: Настройки -> Интеграции -> Для разработчиков.\n" +
                    "Пришлите ваш токен отдельным сообщением.");

                return;
            }

            var title = BuildContactName(update.Message?.ForwardFrom)
                        ?? (update.Message?.ForwardSenderName == null
                            ? null
                            : Regex.Unescape(update.Message?.ForwardSenderName!))
                        ?? BuildContactName(update.Message?.From)
                        ?? "Задача из Telegram";
            var description = TelegramMessageEntitiesFormatter.ToMarkdown(update.Message) ?? "Без текста сообщения";

            await todoistServiceFactory
                .Create(user.GetValue<string>(TodoistApiTokenFieldName))
                .CreateTask(title, description);
        }
        catch (Exception e)
        {
            logger.LogError(e, "Exception occurred {Message}", e.Message);

            await telegramClient.SendTextMessageAsync(
                message!.Chat.Id,
                "Не смог создать задачу",
                replyToMessageId: message.MessageId);
        }
    }

    [GeneratedRegex("^[a-zA-Z0-9]+$")]
    private static partial Regex TodoistApiTokenRegex();

    private static async Task<bool> IsValidTodoistApiToken(
        TodoistApiClientFactory todoistApiClientFactory,
        string probablyToken)
    {
        if (!TodoistApiTokenRegex().IsMatch(probablyToken))
            return false;

        var client = todoistApiClientFactory.Create(probablyToken);
        try
        {
            await client.GetProjects();
            return true;
        }
        catch
        {
            return false;
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