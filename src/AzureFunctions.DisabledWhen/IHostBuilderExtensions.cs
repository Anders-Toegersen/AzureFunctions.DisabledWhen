using AzureFunctions.DisabledWhen.Internal;
using Microsoft.Azure.Functions.Worker.Core.FunctionMetadata;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace AzureFunctions.DisabledWhen;

/// <summary>
/// DisabledWhen extensions for <see cref="IHostBuilder"/>.
/// </summary>
public static class IHostBuilderExtensions
{
    /// <summary>
    /// Enables DisabledWhen function filtering based on configuration values.
    /// </summary>
    /// <param name="builder">The host builder.</param>
    /// <returns>The <paramref name="builder"/> for chaining.</returns>
    public static IHostBuilder UseDisabledWhen(this IHostBuilder builder)
    {
        ArgumentNullException.ThrowIfNull(builder);
        return builder.ConfigureServices(services => services.AddSingleton<IFunctionMetadataProvider, DisabledWhenMetadataProvider>());
    }
}
