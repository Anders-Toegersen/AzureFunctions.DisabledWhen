# AzureFunctions.DisabledWhen

Conditionally disable Azure Functions based on configuration values using attributes.

## Packages

| Package | Description |
|---------|-------------|
| `AzureFunctions.DisabledWhen` | Reflection-based implementation |
| `AzureFunctions.DisabledWhen.SourceGenerator` | Source-generated implementation (AOT-compatible) |

## Installation

```bash
# Reflection-based
dotnet add package AzureFunctions.DisabledWhen

# Source-generated (requires additional setup)
dotnet add package AzureFunctions.DisabledWhen
dotnet add package AzureFunctions.DisabledWhen.SourceGenerator
```

### Source Generator Setup

When using the SourceGenerator, add this to your `.csproj`:

```xml
<PropertyGroup>
  <InterceptorsPreviewNamespaces>$(InterceptorsPreviewNamespaces);AzureFunctions.DisabledWhen</InterceptorsPreviewNamespaces>
</PropertyGroup>
```

> **Note:** The source generator only discovers functions in the same assembly where `UseDisabledWhen()` is called. If your functions are spread across multiple assemblies, use the reflection-based package instead.

## Usage

### 1. Register the metadata provider

In your `Program.cs`, call `UseDisabledWhen()` on the host builder **after** `ConfigureFunctionsWebApplication()`:

```csharp
using AzureFunctions.DisabledWhen;

var host = new HostBuilder()
    .ConfigureFunctionsWebApplication()
    .UseDisabledWhen() // Must be called after ConfigureFunctionsWebApplication
    .Build();

host.Run();
```

> **Important:** `UseDisabledWhen()` must be called after `ConfigureFunctionsWebApplication()` (or `ConfigureFunctionsWorkerDefaults()`) to ensure the default function metadata provider is registered first.

### 2. Apply attributes to functions

```csharp
using AzureFunctions.DisabledWhen;

public class MyFunctions
{
    [DisabledWhenLocal]
    [Function("MyScheduledFunction")]
    public async Task Run([TimerTrigger("0 */5 * * * *")] TimerInfo timer)
    {
    }

    [DisabledWhen("FeatureFlags:DisableThis", "true")]
    [Function("FeatureFlaggedFunction")]
    public async Task RunFlagged([HttpTrigger(AuthorizationLevel.Function, "get")] HttpRequest req)
    {
    }

    [DisabledWhenNullOrEmpty("ExternalService:ApiKey")]
    [Function("RequiresApiKey")]
    public async Task RunWithKey([HttpTrigger(AuthorizationLevel.Function, "post")] HttpRequest req)
    {
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

When using both packages, the SourceGenerator intercepts calls to `UseDisabledWhen()` and replaces the reflection-based implementation with a compile-time generated version that is AOT-compatible.

Since filtering happens at startup:
* Configuration changes require a restart to take effect
* Disabled functions don't appear in the Azure Portal
* Disabled functions are logged as warnings

## License

MIT
