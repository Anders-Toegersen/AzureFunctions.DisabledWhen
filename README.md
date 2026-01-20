# AzureFunctions.DisabledWhen

Conditionally disable Azure Functions based on configuration values using attributes.

## Packages

| Package | Description |
|---------|-------------|
| `AzureFunctions.DisabledWhen` | Reflection-based implementation |
| `AzureFunctions.DisabledWhen.SourceGenerator` | Source-generated implementation |

## Installation

### Reflection-based (simple setup)

```bash
dotnet add package AzureFunctions.DisabledWhen
```

See [`sample/SampleFunctionApp`](sample/SampleFunctionApp) for a working example.

### Source-generated (AOT-compatible)

```bash
dotnet add package AzureFunctions.DisabledWhen
dotnet add package AzureFunctions.DisabledWhen.SourceGenerator
```

Add this to your `.csproj` to enable interceptors:

```xml
<PropertyGroup>
  <InterceptorsPreviewNamespaces>$(InterceptorsPreviewNamespaces);AzureFunctions.DisabledWhen</InterceptorsPreviewNamespaces>
</PropertyGroup>
```

See [`sample/SampleFunctionApp.SourceGenerated`](sample/SampleFunctionApp.SourceGenerated) for a working example.

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
    [Function("ScheduledCleanup")]
    [DisabledWhenLocal]
    public static void ScheduledCleanup(
        [TimerTrigger("0 */5 * * * *", RunOnStartup = true)] TimerInfo timer)
    {
    }

    [Function("ProcessOrderQueue")]
    [DisabledWhenNullOrEmpty("ServiceBusConnection")]
    public static void ProcessOrderQueue(
        [ServiceBusTrigger("orders", Connection = "ServiceBusConnection")] string message)
    {
    }

    [Function("GdprDataExport")]
    [DisabledWhen("EnvironmentOptions:RegionAbbreviation", "US", StringComparison.OrdinalIgnoreCase)]
    [DisabledWhen("EnvironmentOptions:RegionAbbreviation", "CA", StringComparison.OrdinalIgnoreCase)]
    [DisabledWhen("EnvironmentOptions:RegionAbbreviation", "AU", StringComparison.OrdinalIgnoreCase)]
    public static IActionResult GdprDataExport(
        [HttpTrigger(AuthorizationLevel.Function, "get", Route = "gdpr/export/{userId}")] HttpRequest req,
        string userId)
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

The library provides a custom `IFunctionMetadataProvider` that evaluates attribute conditions at startup and excludes disabled functions from registration. The approach is inspired from this [github issue](https://github.com/Azure/azure-functions-dotnet-worker/issues/1398#issuecomment-2144149276).

When using both packages, the SourceGenerator intercepts calls to `UseDisabledWhen()` and replaces the reflection-based implementation with a compile-time generated version that is AOT-compatible.

Since filtering happens at startup:
* Configuration changes require a restart to take effect
* Disabled functions don't appear in the Azure Portal
* Disabled functions are logged as warnings

## License

MIT
