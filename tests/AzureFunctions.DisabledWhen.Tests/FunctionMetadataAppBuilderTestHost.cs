using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Builder;
using Microsoft.Azure.Functions.Worker.Core.FunctionMetadata;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;

namespace AzureFunctions.DisabledWhen.Tests;

public sealed class FunctionMetadataAppBuilderTestHost : IAsyncDisposable
{
    private IHost? host;
    private readonly Action<IFunctionsWorkerApplicationBuilder> configureTestHost;

    public FunctionMetadataAppBuilderTestHost(Action<IFunctionsWorkerApplicationBuilder> configureTestHost)
        => this.configureTestHost = configureTestHost;

    /// <summary>
    /// Starts the test host with the specified configuration using <see cref="FunctionsApplication.CreateBuilder(string[])"/>.
    /// <para/>
    /// A minimal <c>host.json</c> is created at runtime (not build time) to avoid the Azure Functions SDK
    /// treating the test project as a runnable Functions app.
    /// <para/>
    /// The WorkerHostedService is removed to prevent gRPC errors since no real Azure Functions host is running.
    /// </summary>
    /// <param name="configuration">Optional configuration key-value pairs to use for the host.</param>
    public async Task StartAsync(Dictionary<string, string?>? configuration = null)
    {
        EnsureHostJson();

        var builder = FunctionsApplication.CreateBuilder([]);

        builder.Configuration.AddInMemoryCollection(configuration ?? []);

        configureTestHost(builder);

        // Removes WorkerHostedService to prevent gRPC errors in tests (no real Azure Functions host)
        builder.Services.RemoveAll<IHostedService>();

        host = builder.Build();

        await host.StartAsync();
    }

    /// <summary>
    /// Retrieves the function metadata from the running host.
    /// </summary>
    /// <returns>The collection of function metadata registered with the host.</returns>
    /// <exception cref="InvalidOperationException">Thrown when the host has not been started.</exception>
    public async Task<IEnumerable<IFunctionMetadata>> GetFunctionMetadataAsync()
    {
        if (host is null)
        {
            throw new InvalidOperationException("Host has not been started. Call StartAsync first.");
        }

        return await host.Services
            .GetRequiredService<IFunctionMetadataProvider>()
            .GetFunctionMetadataAsync(AppContext.BaseDirectory);
    }

    /// <summary>
    /// Retrieves the name of the function metadata provider.
    /// </summary>
    /// <returns>The name of the function metadata provider.</returns>
    /// <exception cref="InvalidOperationException">Thrown when the host has not been started.</exception>
    public string GetMetadataProviderTypeName()
    {
        if (host is null)
        {
            throw new InvalidOperationException("Host has not been started. Call StartAsync first.");
        }

        return host.Services
            .GetRequiredService<IFunctionMetadataProvider>()
            .GetType()
            .Name;
    }

    public async ValueTask DisposeAsync()
    {
        if (host is not null)
        {
            await host.StopAsync();
            host.Dispose();
            host = null;
        }

        GC.SuppressFinalize(this);
    }

    private static readonly object hostJsonLock = new();

    /// <summary>
    /// Creates a minimal <c>host.json</c> in the base directory if it doesn't already exist.
    /// This file is required by <see cref="FunctionsApplication.CreateBuilder(string[])"/> to locate the project root.
    /// It is created at runtime rather than included as a build artifact to prevent the Azure Functions SDK
    /// from generating a <c>func start</c> RunCommand for the test project.
    /// </summary>
    private static void EnsureHostJson()
    {
        var hostJsonPath = Path.Combine(AppContext.BaseDirectory, "host.json");
        if (File.Exists(hostJsonPath))
        {
            return;
        }

        lock (hostJsonLock)
        {
            if (!File.Exists(hostJsonPath))
            {
                File.WriteAllText(hostJsonPath, """{"version": "2.0"}""");
            }
        }
    }
}
