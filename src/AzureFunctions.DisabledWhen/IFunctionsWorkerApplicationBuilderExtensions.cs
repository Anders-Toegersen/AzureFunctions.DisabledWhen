using AzureFunctions.DisabledWhen.Internal;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Core.FunctionMetadata;
using Microsoft.Extensions.DependencyInjection;

namespace AzureFunctions.DisabledWhen;

public static class IFunctionsWorkerApplicationBuilderExtensions
{
    public static void UseDisabledWhen(this IFunctionsWorkerApplicationBuilder builder)
    {
        // Validate that an underlying IFunctionMetadataProvider is already registered
        // This is typically done by ConfigureFunctionsWebApplication() or ConfigureFunctionsWorkerDefaults()
        var hasUnderlyingProvider = builder.Services.Any(descriptor => 
            descriptor.ServiceType == typeof(IFunctionMetadataProvider));

        if (!hasUnderlyingProvider)
        {
            throw new InvalidOperationException(
                "UseDisabledWhen() must be called after ConfigureFunctionsWebApplication() or ConfigureFunctionsWorkerDefaults(). " +
                "No underlying IFunctionMetadataProvider was found in the service collection.");
        }

        builder.Services.AddSingleton<IFunctionMetadataProvider, DisabledWhenMetadataProvider>();
    }
}