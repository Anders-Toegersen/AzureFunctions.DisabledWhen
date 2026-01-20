# AzureFunctions.DisabledWhen

Conditionally disable Azure Functions based on configuration values using attributes.

## Packages

| Package | Description |
|---------|-------------|
| `AzureFunctions.DisabledWhen.Reflection` | Reflection-based implementation |
| `AzureFunctions.DisabledWhen.SourceGenerator` | Source-generated implementation |

## Installation

```bash
# Reflection-based
dotnet add package AzureFunctions.DisabledWhen.Reflection

# Source-generated
dotnet add package AzureFunctions.DisabledWhen.SourceGenerator
```

## Usage

### 1. Register the metadata provider

In your `Program.cs`, call `UseDisabledWhen()` on the host builder after `ConfigureFunctionsWebApplication()`:

```csharp
using AzureFunctions.DisabledWhen;

var host = new HostBuilder()
    .ConfigureFunctionsWebApplication()
    .UseDisabledWhen()
    .Build();

host.Run();
```

### 2. Apply attributes to functions

```csharp
using AzureFunctions.DisabledWhen;

public class MyFunctions
{
    // Disable in Development environment
    [DisabledWhenLocal]
    [Function("MyScheduledFunction")]
    public async Task Run([TimerTrigger("0 */5 * * * *")] TimerInfo timer)
    {
        // ...
    }

    // Disable when config matches a value
    [DisabledWhen("FeatureFlags:DisableThis", "true")]
    [Function("FeatureFlaggedFunction")]
    public async Task RunFlagged([HttpTrigger(AuthorizationLevel.Function, "get")] HttpRequest req)
    {
        // ...
    }

    // Disable when config is missing or empty
    [DisabledWhenNullOrEmpty("ExternalService:ApiKey")]
    [Function("RequiresApiKey")]
    public async Task RunWithKey([HttpTrigger(AuthorizationLevel.Function, "post")] HttpRequest req)
    {
        // ...
    }
}
```

## Attributes

### `DisabledWhenAttribute`

Disables a function when a configuration key matches a specific value.

```csharp
[DisabledWhen("ConfigKey", "ConfigValue")]
[DisabledWhen("ConfigKey", "ConfigValue", StringComparison.OrdinalIgnoreCase)]
```

### `DisabledWhenLocalAttribute`

Disables a function when `AZURE_FUNCTIONS_ENVIRONMENT` equals `Development`.

```csharp
[DisabledWhenLocal]
```

Read more about [`AZURE_FUNCTIONS_ENVIRONMENT`](https://learn.microsoft.com/en-us/azure/azure-functions/functions-app-settings#azure_functions_environment).

### `DisabledWhenNullOrEmptyAttribute`

Disables a function when a configuration key is missing, null, or empty.

```csharp
[DisabledWhenNullOrEmpty("RequiredConfigKey")]
```

## How It Works

The library provides a custom `IFunctionMetadataProvider` that evaluates attribute conditions at startup and excludes disabled functions from registration.

Since filtering happens at startup:
* Configuration changes require a restart to take effect
* Disabled functions don't appear in the Azure Portal
* Disabled functions are logged as warnings

## License

MIT
