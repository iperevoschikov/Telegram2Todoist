using JetBrains.Annotations;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
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
            return WebHookFunctionHandlerResponse.Ok(null);
        }
        catch (Exception e)
        {
            Console.Error.WriteLine($"Exception occured: {e.Message}, StackTrace: {e.StackTrace}");
            return WebHookFunctionHandlerResponse.Ok(null);
        }
    }

    private static async Task HandleAsync(WebHookFunctionHandlerRequest request)
    {
        var serviceProvider = ContainerConfiguration.ConfigureServices();
        var logger = serviceProvider.GetRequiredService<ILogger<WebHookFunctionHandler>>();
        var todoistApiClient = serviceProvider.GetRequiredService<TodoistApiClient>();

        logger.LogInformation("Received webhook update: {Update}", request.Body);
        var update = JsonConvert.DeserializeObject<Update>(request.Body)!;

        var inbox = (await todoistApiClient.GetProjects()).FirstOrDefault(p => p.IsInboxProject);
        if (inbox == null)
            throw new Exception("Inbox project not found");

        await todoistApiClient.CreateTaskAsync(
            inbox.Id,
            update.Message?.Contact?.ToString() ?? "Задача из Telegram",
            update.Message?.Text);
    }
}