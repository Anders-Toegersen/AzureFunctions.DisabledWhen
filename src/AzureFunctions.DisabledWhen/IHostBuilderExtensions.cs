using AzureFunctions.DisabledWhen.Internal;
using Microsoft.Azure.Functions.Worker.Core.FunctionMetadata;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace AzureFunctions.DisabledWhen;

public static class IHostBuilderExtensions
{
    public static IHostBuilder UseDisabledWhen(this IHostBuilder builder)
        => builder.ConfigureServices(services => services.AddSingleton<IFunctionMetadataProvider, DisabledWhenMetadataProvider>());
}
