using System.Collections.Immutable;
using System.Reflection;
using Microsoft.Azure.Functions.Worker.Core.FunctionMetadata;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace AzureFunctions.DisabledWhen.Internal;

internal sealed class DisabledWhenMetadataProvider : IFunctionMetadataProvider
{
    private readonly IServiceProvider serviceProvider;
    private readonly IConfiguration configuration;
    private readonly ILogger<DisabledWhenMetadataProvider>? logger;

    public DisabledWhenMetadataProvider(
        IServiceProvider serviceProvider,
        IConfiguration configuration,
        ILogger<DisabledWhenMetadataProvider>? logger = null)
    {
        this.serviceProvider = serviceProvider;
        this.configuration = configuration;
        this.logger = logger;
    }

    public async Task<ImmutableArray<IFunctionMetadata>> GetFunctionMetadataAsync(string directory)
    {
        var functionMetadataProvider = serviceProvider
            .GetServices<IFunctionMetadataProvider>()
            .LastOrDefault(x => x.GetType() != typeof(DisabledWhenMetadataProvider))
            ?? throw new InvalidOperationException("No other IFunctionMetadataProvider is registered. Ensure the default Azure Functions metadata provider is available before calling UseDisabledWhen().");

        var metaData = await functionMetadataProvider
            .GetFunctionMetadataAsync(directory)
            .ConfigureAwait(false);

        var assemblies = metaData
            .Select(m => m.ScriptFile)
            .Where(f => f is not null && File.Exists(f))
            .Cast<string>()
            .Distinct(StringComparer.Ordinal)
            .ToDictionary(path => path, Assembly.LoadFrom);

        var disabledFunctions = new HashSet<string>(StringComparer.Ordinal);

        foreach (var function in metaData)
        {
            if (function.EntryPoint is null || function.ScriptFile is null || function.Name is null)
            {
                continue;
            }

            if (!assemblies.TryGetValue(function.ScriptFile, out var assembly))
            {
                continue;
            }

            var lastDot = function.EntryPoint.LastIndexOf('.');
            if (lastDot < 0)
            {
                continue;
            }

            var typeName = function.EntryPoint[..lastDot];
            var methodName = function.EntryPoint[(lastDot + 1)..];

            var type = assembly.GetType(typeName);
            var method = type?.GetMethod(methodName, BindingFlags.DeclaredOnly | BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public);

            if (method is not null && IsDisabled(method))
            {
                logger?.FunctionDisabled(function.Name);
                disabledFunctions.Add(function.EntryPoint);
            }
        }

        return [.. metaData.Where(m => !disabledFunctions.Contains(m.EntryPoint!))];
    }

    private bool IsDisabled(MethodInfo method)
    {
        foreach (var attr in method.GetCustomAttributes<DisabledWhenAttribute>())
        {
            var configValue = configuration.GetValue<string>(attr.ConfigKey);
            if (string.Equals(configValue, attr.ConfigValue, attr.ConfigValueComparer))
            {
                return true;
            }
        }

        if (method.GetCustomAttribute<DisabledWhenLocalAttribute>() is not null)
        {
            var configValue = configuration.GetValue<string>("AZURE_FUNCTIONS_ENVIRONMENT");
            if (string.Equals(configValue, "Development", StringComparison.Ordinal))
            {
                return true;
            }
        }

        foreach (var attr in method.GetCustomAttributes<DisabledWhenNullOrEmptyAttribute>())
        {
            var configValue = configuration.GetValue<string>(attr.ConfigKey);
            if (string.IsNullOrEmpty(configValue))
            {
                return true;
            }
        }

        return false;
    }
}
