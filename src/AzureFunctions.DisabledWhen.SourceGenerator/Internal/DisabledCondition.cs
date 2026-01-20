namespace AzureFunctions.DisabledWhen.SourceGenerator.Internal;

internal readonly struct DisabledCondition : IEquatable<DisabledCondition>
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

    public bool Equals(DisabledCondition other) =>
        ConfigKey == other.ConfigKey &&
        ConfigValue == other.ConfigValue &&
        ConfigValueComparer == other.ConfigValueComparer &&
        MatchNullOrEmpty == other.MatchNullOrEmpty;

    public override bool Equals(object? obj) => obj is DisabledCondition other && Equals(other);

    public override int GetHashCode() => HashCode.Combine(ConfigKey, ConfigValue, ConfigValueComparer, MatchNullOrEmpty);
}
