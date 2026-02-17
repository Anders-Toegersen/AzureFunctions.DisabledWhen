using AzureFunctions.DisabledWhen.Internal;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Core.FunctionMetadata;
using Microsoft.Extensions.DependencyInjection;

namespace AzureFunctions.DisabledWhen;

public static class IFunctionsWorkerApplicationBuilderExtensions
{
    public static void UseDisabledWhen(this IFunctionsWorkerApplicationBuilder builder)
    {
        builder.Services.AddSingleton<IFunctionMetadataProvider, DisabledWhenMetadataProvider>();
    }
}