using Microsoft.Azure.Functions.Worker.Core.FunctionMetadata;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;

namespace AzureFunctions.DisabledWhen.Tests;

public sealed class FunctionMetadataTestHost : IAsyncDisposable
{
    private IHost? host;
    private readonly Action<IHostBuilder> configureTestHost;

    public FunctionMetadataTestHost(Action<IHostBuilder> configureTestHost)
        => this.configureTestHost = configureTestHost;

    /// <summary>
    /// Starts the test host with the specified configuration.
    /// <para/>
    /// The WorkerHostedService is removed to prevent gRPC errors since no real Azure Functions host is running.
    /// </summary>
    /// <param name="configuration">Optional configuration key-value pairs to use for the host.</param>
    public async Task StartAsync(Dictionary<string, string?>? configuration = null)
    {
        var builder = new HostBuilder()
            .ConfigureFunctionsWorkerDefaults()
            .ConfigureAppConfiguration(config =>
            {
                config.SetBasePath(AppContext.BaseDirectory);
                config.AddInMemoryCollection(configuration ?? []);
            })
            .ConfigureServices(services =>
            {
                // Removes WorkerHostedService to prevent gRPC errors in tests (no real Azure Functions host)
                services.RemoveAll<IHostedService>();
            });

        configureTestHost(builder);

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
}