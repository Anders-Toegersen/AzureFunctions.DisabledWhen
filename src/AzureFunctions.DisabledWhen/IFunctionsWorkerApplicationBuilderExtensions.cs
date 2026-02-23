using AzureFunctions.DisabledWhen.Internal;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Core.FunctionMetadata;
using Microsoft.Extensions.DependencyInjection;

namespace AzureFunctions.DisabledWhen;

/// <summary>
/// DisabledWhen extensions for <see cref="IFunctionsWorkerApplicationBuilder"/>.
/// </summary>
public static class IFunctionsWorkerApplicationBuilderExtensions
{
    /// <summary>
    /// Enables DisabledWhen function filtering based on configuration values.
    /// </summary>
    /// <param name="builder">The functions worker application builder.</param>
    public static void UseDisabledWhen(this IFunctionsWorkerApplicationBuilder builder)
    {
        ArgumentNullException.ThrowIfNull(builder);
        builder.Services.AddSingleton<IFunctionMetadataProvider, DisabledWhenMetadataProvider>();
    }
}