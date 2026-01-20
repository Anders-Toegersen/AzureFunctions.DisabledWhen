namespace AzureFunctions.DisabledWhen.SourceGenerator.Internal;

internal readonly record struct DisabledCondition
{
    public DisabledCondition(string configKey, string? configValue, StringComparison configValueComparer, bool matchNullOrEmpty = false)
    {
        ConfigKey = configKey;
        ConfigValue = configValue;
        ConfigValueComparer = configValueComparer;
        MatchNullOrEmpty = matchNullOrEmpty;
    }

    public string ConfigKey { get; }

    public string? ConfigValue { get; }

    public StringComparison ConfigValueComparer { get; }

    public bool MatchNullOrEmpty { get; }
}
