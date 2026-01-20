using AzureFunctions.DisabledWhen.TestHost;

namespace AzureFunctions.DisabledWhen.Tests;

public class HostStartupTests
{
    private readonly FunctionMetadataTestHost testHost = new(b => b.UseDisabledWhen());

    [After(Test)]
    public async Task Cleanup() => await testHost.DisposeAsync();

    [Test]
    public async Task Host_UsesReflectionMetadataProvider()
    {
        await testHost.StartAsync();

        var providerTypeName = testHost.GetMetadataProviderTypeName();

        await Assert.That(providerTypeName).IsEqualTo("DisabledWhenMetadataProvider");
    }

    [Test]
    public async Task Host_IncludesAllFunctions_WhenNoConditionsMatch()
    {
        await testHost.StartAsync(new Dictionary<string, string?>
        {
            ["ENVIRONMENT"] = "Development",
            ["FEATURE_FLAG"] = "enabled",
            ["API_KEY"] = "test-key",
            ["CONNECTION_STRING"] = "test-connection",
            ["DEBUG_MODE"] = "false"
        });

        var functions = await testHost.GetFunctionMetadataAsync();
        var functionNames = functions.Select(f => f.Name!).ToList();

        await Assert.That(functionNames).Contains("AlwaysEnabled");
        await Assert.That(functionNames).Contains("DisabledInProduction");
        await Assert.That(functionNames).Contains("FeatureFlagged");
        await Assert.That(functionNames).Contains("RequiresApiKey");
        await Assert.That(functionNames).Contains("MultipleConditions");
    }

    [Test]
    public async Task Host_FiltersFunction_WhenDisabledWhenConditionMatches()
    {
        await testHost.StartAsync(new Dictionary<string, string?>
        {
            ["ENVIRONMENT"] = "Production",
            ["API_KEY"] = "test-key",
            ["CONNECTION_STRING"] = "test-connection"
        });

        var functions = await testHost.GetFunctionMetadataAsync();
        var functionNames = functions.Select(f => f.Name!).ToList();

        await Assert.That(functionNames).Contains("AlwaysEnabled");
        await Assert.That(functionNames).DoesNotContain("DisabledInProduction");
        await Assert.That(functionNames).DoesNotContain("MultipleConditions");
    }

    [Test]
    public async Task Host_FiltersFunction_WhenDisabledWhenCaseInsensitiveMatches()
    {
        await testHost.StartAsync(new Dictionary<string, string?>
        {
            ["FEATURE_FLAG"] = "DISABLED",
            ["API_KEY"] = "test-key",
            ["CONNECTION_STRING"] = "test-connection"
        });

        var functions = await testHost.GetFunctionMetadataAsync();
        var functionNames = functions.Select(f => f.Name!).ToList();

        await Assert.That(functionNames).DoesNotContain("FeatureFlagged");
    }

    [Test]
    public async Task Host_FiltersFunction_WhenDisabledWhenLocalInDevelopment()
    {
        await testHost.StartAsync(new Dictionary<string, string?>
        {
            ["AZURE_FUNCTIONS_ENVIRONMENT"] = "Development",
            ["API_KEY"] = "test-key",
            ["CONNECTION_STRING"] = "test-connection"
        });

        var functions = await testHost.GetFunctionMetadataAsync();
        var functionNames = functions.Select(f => f.Name!).ToList();

        await Assert.That(functionNames).Contains("AlwaysEnabled");
        await Assert.That(functionNames).DoesNotContain("DisabledLocally");
        await Assert.That(functionNames).DoesNotContain("MixedConditions");
    }

    [Test]
    public async Task Host_IncludesFunction_WhenDisabledWhenLocalNotInDevelopment()
    {
        await testHost.StartAsync(new Dictionary<string, string?>
        {
            ["AZURE_FUNCTIONS_ENVIRONMENT"] = "Production",
            ["API_KEY"] = "test-key",
            ["CONNECTION_STRING"] = "test-connection"
        });

        var functions = await testHost.GetFunctionMetadataAsync();
        var functionNames = functions.Select(f => f.Name!).ToList();

        await Assert.That(functionNames).Contains("DisabledLocally");
    }

    [Test]
    public async Task Host_FiltersFunction_WhenDisabledWhenNullOrEmptyConfigMissing()
    {
        await testHost.StartAsync(new Dictionary<string, string?>
        {
            ["CONNECTION_STRING"] = "test-connection"
        });

        var functions = await testHost.GetFunctionMetadataAsync();
        var functionNames = functions.Select(f => f.Name!).ToList();

        await Assert.That(functionNames).Contains("AlwaysEnabled");
        await Assert.That(functionNames).DoesNotContain("RequiresApiKey");
    }

    [Test]
    public async Task Host_FiltersFunction_WhenDisabledWhenNullOrEmptyConfigEmpty()
    {
        await testHost.StartAsync(new Dictionary<string, string?>
        {
            ["API_KEY"] = "",
            ["CONNECTION_STRING"] = "test-connection"
        });

        var functions = await testHost.GetFunctionMetadataAsync();
        var functionNames = functions.Select(f => f.Name!).ToList();

        await Assert.That(functionNames).DoesNotContain("RequiresApiKey");
    }

    [Test]
    public async Task Host_IncludesFunction_WhenDisabledWhenNullOrEmptyConfigPresent()
    {
        await testHost.StartAsync(new Dictionary<string, string?>
        {
            ["API_KEY"] = "my-api-key",
            ["CONNECTION_STRING"] = "test-connection"
        });

        var functions = await testHost.GetFunctionMetadataAsync();
        var functionNames = functions.Select(f => f.Name!).ToList();

        await Assert.That(functionNames).Contains("RequiresApiKey");
    }

    [Test]
    public async Task Host_FiltersFunction_WhenMultipleConditions_FirstMatches()
    {
        await testHost.StartAsync(new Dictionary<string, string?>
        {
            ["ENVIRONMENT"] = "Production",
            ["DEBUG_MODE"] = "false",
            ["API_KEY"] = "test-key",
            ["CONNECTION_STRING"] = "test-connection"
        });

        var functions = await testHost.GetFunctionMetadataAsync();
        var functionNames = functions.Select(f => f.Name!).ToList();

        await Assert.That(functionNames).DoesNotContain("MultipleConditions");
    }

    [Test]
    public async Task Host_FiltersFunction_WhenMultipleConditions_SecondMatches()
    {
        await testHost.StartAsync(new Dictionary<string, string?>
        {
            ["ENVIRONMENT"] = "Development",
            ["DEBUG_MODE"] = "true",
            ["API_KEY"] = "test-key",
            ["CONNECTION_STRING"] = "test-connection"
        });

        var functions = await testHost.GetFunctionMetadataAsync();
        var functionNames = functions.Select(f => f.Name!).ToList();

        await Assert.That(functionNames).DoesNotContain("MultipleConditions");
    }

    [Test]
    public async Task Host_IncludesFunction_WhenMultipleConditions_NoneMatch()
    {
        await testHost.StartAsync(new Dictionary<string, string?>
        {
            ["ENVIRONMENT"] = "Staging",
            ["DEBUG_MODE"] = "false",
            ["API_KEY"] = "test-key",
            ["CONNECTION_STRING"] = "test-connection"
        });

        var functions = await testHost.GetFunctionMetadataAsync();
        var functionNames = functions.Select(f => f.Name!).ToList();

        await Assert.That(functionNames).Contains("MultipleConditions");
    }

    [Test]
    public async Task Host_FiltersFunction_WhenMixedConditions_LocalMatches()
    {
        await testHost.StartAsync(new Dictionary<string, string?>
        {
            ["AZURE_FUNCTIONS_ENVIRONMENT"] = "Development",
            ["CONNECTION_STRING"] = "test-connection"
        });

        var functions = await testHost.GetFunctionMetadataAsync();
        var functionNames = functions.Select(f => f.Name!).ToList();

        await Assert.That(functionNames).DoesNotContain("MixedConditions");
    }

    [Test]
    public async Task Host_FiltersFunction_WhenMixedConditions_NullOrEmptyMatches()
    {
        await testHost.StartAsync(new Dictionary<string, string?>
        {
            ["AZURE_FUNCTIONS_ENVIRONMENT"] = "Production"
        });

        var functions = await testHost.GetFunctionMetadataAsync();
        var functionNames = functions.Select(f => f.Name!).ToList();

        await Assert.That(functionNames).DoesNotContain("MixedConditions");
    }

    [Test]
    public async Task Host_FiltersFunction_WhenNestedConfigKeyMatches()
    {
        await testHost.StartAsync(new Dictionary<string, string?>
        {
            ["Features:BetaFeature:Enabled"] = "false",
            ["Database:ConnectionString"] = "test-connection"
        });

        var functions = await testHost.GetFunctionMetadataAsync();
        var functionNames = functions.Select(f => f.Name!).ToList();

        await Assert.That(functionNames).DoesNotContain("NestedConfigDisabled");
        await Assert.That(functionNames).Contains("NestedConfigRequired");
    }

    [Test]
    public async Task Host_IncludesFunction_WhenNestedConfigKeyDoesNotMatch()
    {
        await testHost.StartAsync(new Dictionary<string, string?>
        {
            ["Features:BetaFeature:Enabled"] = "true",
            ["Database:ConnectionString"] = "test-connection"
        });

        var functions = await testHost.GetFunctionMetadataAsync();
        var functionNames = functions.Select(f => f.Name!).ToList();

        await Assert.That(functionNames).Contains("NestedConfigDisabled");
        await Assert.That(functionNames).Contains("NestedConfigRequired");
    }

    [Test]
    public async Task Host_FiltersFunction_WhenNestedConfigKeyNullOrEmpty()
    {
        await testHost.StartAsync(new Dictionary<string, string?>
        {
            ["Features:BetaFeature:Enabled"] = "true"
        });

        var functions = await testHost.GetFunctionMetadataAsync();
        var functionNames = functions.Select(f => f.Name!).ToList();

        await Assert.That(functionNames).Contains("NestedConfigDisabled");
        await Assert.That(functionNames).DoesNotContain("NestedConfigRequired");
    }
}
