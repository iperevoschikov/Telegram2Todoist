using JetBrains.Annotations;
using Telegram2Todoist.Functions.Storage;
using Telegram2Todoist.Functions.Todoist;

namespace Telegram2Todoist.Functions;

[UsedImplicitly]
public class OAuthAsyncFunctionHandler(
    TodoistAuthClient todoistAuth,
    AuthStateStorage authStateStorage,
    UsersStorage usersStorage
) : IAsyncFunctionHandler
{
    public async Task<FunctionHandlerResponse> HandleAsync(FunctionHandlerRequest request)
    {
        if (!request.Query.TryGetValue("code", out var code) || !request.Query.TryGetValue("state", out var state))
            return FunctionHandlerResponse.BadRequest();

        var userId = await authStateStorage.GetUserIdForState(state);
        if (userId == null)
            return FunctionHandlerResponse.NotFound();

        var accessToken = await todoistAuth.ObtainAccessToken(code);
        await usersStorage.SetAccessTokenFor(userId.Value, accessToken);
        return FunctionHandlerResponse.Ok();
    }
}