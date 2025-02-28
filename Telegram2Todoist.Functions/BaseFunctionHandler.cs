using Microsoft.Extensions.DependencyInjection;
using Yandex.Cloud.Functions;

namespace Telegram2Todoist.Functions;

public abstract class BaseFunctionHandler<TAsyncHandler> : YcFunction<FunctionHandlerRequest, FunctionHandlerResponse>
    where TAsyncHandler : IAsyncFunctionHandler
{
    public FunctionHandlerResponse FunctionHandler(FunctionHandlerRequest request, Context context)
    {
        try
        {
            return ContainerConfiguration
                .ConfigureServices()
                .GetRequiredService<TAsyncHandler>()
                .HandleAsync(request)
                .GetAwaiter()
                .GetResult();
        }
        catch (Exception e)
        {
            Console
                .Error
                .WriteLine($"Exception occurred: {e.Message}, StackTrace: {e.StackTrace}");
            return FunctionHandlerResponse.Fail();
        }
    }
}