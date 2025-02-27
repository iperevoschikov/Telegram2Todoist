namespace Telegram2Todoist.Functions;

public interface IAsyncFunctionHandler
{
    Task<FunctionHandlerResponse> HandleAsync(FunctionHandlerRequest request);
}