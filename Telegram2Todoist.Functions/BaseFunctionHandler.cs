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
            var serviceProvider = ContainerConfiguration.ConfigureServices();
            var handler = serviceProvider.GetRequiredService<TAsyncHandler>();
            handler.HandleAsync(request).GetAwaiter().GetResult();
            return FunctionHandlerResponse.Ok();
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