using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;

namespace AzureFunctions.DisabledWhen.TestHost;

public static class HostBuilderExtensions
{
    /// <summary>
    /// Removes WorkerHostedService to prevent gRPC errors in tests (no real Azure Functions host)
    /// </summary>
    /// <param name="builder">The IHostBuilder</param>
    /// <returns>The IHostBuilder</returns>
    public static IHostBuilder RemoveWorkerHostedService(this IHostBuilder builder)
        => builder.ConfigureServices(services => services.RemoveAll<IHostedService>());
}