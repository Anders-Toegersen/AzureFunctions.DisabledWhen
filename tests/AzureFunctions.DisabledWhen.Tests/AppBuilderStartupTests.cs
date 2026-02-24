namespace AzureFunctions.DisabledWhen.Tests;

public class AppBuilderStartupTests
{
    private readonly FunctionMetadataAppBuilderTestHost testHost = new(b => b.UseDisabledWhen());

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
}
