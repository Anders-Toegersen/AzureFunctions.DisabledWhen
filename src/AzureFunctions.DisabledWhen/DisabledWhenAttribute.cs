namespace AzureFunctions.DisabledWhen;

/// <summary>
/// Disables a function when a configuration key matches a value.
/// </summary>
[AttributeUsage(AttributeTargets.Method, AllowMultiple = true, Inherited = false)]
public sealed class DisabledWhenAttribute : Attribute
{
    /// <param name="configKey">The configuration key to evaluate.</param>
    /// <param name="configValue">The value that disables the function when matched.</param>
    /// <param name="configValueComparer">The comparison type. Defaults to <see cref="StringComparison.Ordinal"/>.</param>
    public DisabledWhenAttribute(string configKey, string? configValue, StringComparison configValueComparer = StringComparison.Ordinal)
    {
        ConfigKey = configKey;
        ConfigValue = configValue;
        ConfigValueComparer = configValueComparer;
    }

    /// <summary>The configuration key to evaluate.</summary>
    public string ConfigKey { get; }

    /// <summary>The value that disables the function when matched.</summary>
    public string? ConfigValue { get; }

    /// <summary>The comparison type used for matching.</summary>
    public StringComparison ConfigValueComparer { get; }
}
