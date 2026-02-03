using System.Text.Json;
using System.Text.RegularExpressions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;
using Telegram2Todoist.Functions.Storage;
using Telegram2Todoist.Functions.Todoist;
using YandexCloudFunctions.Net.Sdk;
using YandexCloudFunctions.Net.Sdk.Webhook;

namespace Telegram2Todoist.Functions;

public class WebHookAsyncFunctionHandler() : WebhookFunctionHandler(HandleAsync)
{
    private static async Task<WebhookHandlerResponse> HandleAsync(
        WebhookHandlerRequest request,
        ITelegramBotClient telegramClient,
        UsersStorage usersStorage,
        ILogger<WebHookAsyncFunctionHandler> logger,
        TodoistApiClientFactory todoistApiClientFactory,
        TodoistAuthClient todoistAuthClient,
        AuthStateStorage authStateStorage
    )
    {
        var update = JsonSerializer.Deserialize<Update>(
            request.body,
            new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower }
        )!;

        logger.LogInformation("Received webhook update: {Update},", update.Id);

        if (update.Message?.From?.Username == "iperevoschikov")
            logger.LogInformation("Request body {Body}", request.body);

        var message = update.Message;

        try
        {
            if (message == null)
                throw new NullReferenceException("Message is null");
            if (message.From?.Id == null)
                throw new NullReferenceException("From is null");

            var userId = message.From.Id;
            var authToken = await usersStorage.GetAccessTokenFor(userId);

            if (string.IsNullOrEmpty(authToken))
            {
                var state = await authStateStorage.CreateAuthState(userId);

                await telegramClient.SendMessage(
                    message.Chat.Id,
                    "Для работы необходимо дать боту разрешение создавать задачи в вашем Todoist",
                    replyMarkup: new InlineKeyboardMarkup(
                        InlineKeyboardButton.WithUrl(
                            "Авторизовать бота",
                            todoistAuthClient.GetAuthUrl(state)
                        )
                    )
                );

                return WebhookHandlerResponses.Ok();
            }

            var title =
                BuildContactName(update.Message?.ForwardFrom)
                ?? (
                    update.Message?.ForwardSenderName == null
                        ? null
                        : Regex.Unescape(update.Message?.ForwardSenderName!)
                )
                ?? BuildContactName(update.Message?.From)
                ?? "Задача из Telegram";
            var description = TelegramMessageEntitiesFormatter.ToMarkdown(update.Message);
            await todoistApiClientFactory
                .Create(authToken)
                .CreateTaskAsync(
                    title,
                    description,
                    DateOnly.FromDateTime(DateTime.UtcNow.AddHours(5))
                );

            await telegramClient.SendMessage(
                message.Chat.Id,
                "✔",
                replyParameters: new ReplyParameters { MessageId = message.Id }
            );
        }
        catch (Exception e)
        {
            logger.LogError(e, "Exception occurred {Message}", e.Message);

            if (message != null)
                await telegramClient.SendMessage(
                    message.Chat.Id,
                    "Не смог создать задачу",
                    replyParameters: new ReplyParameters { MessageId = message.Id }
                );
        }

        return WebhookHandlerResponses.Ok();
    }

    private static string? BuildContactName(User? user)
    {
        return user == null ? null : $"{user.FirstName} {user.LastName}";
    }

    protected override void ConfigureServices(IServiceCollection serviceCollection) =>
        serviceCollection.ConfigureServices();
}
