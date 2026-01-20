namespace AzureFunctions.DisabledWhen;

[AttributeUsage(AttributeTargets.Method, AllowMultiple = true, Inherited = false)]
public sealed class DisabledWhenAttribute : Attribute
{
    public DisabledWhenAttribute(string configKey, string? configValue, StringComparison configValueComparer = StringComparison.Ordinal)
    {
        ConfigKey = configKey;
        ConfigValue = configValue;
        ConfigValueComparer = configValueComparer;
    }

    public string ConfigKey { get; }

    public string? ConfigValue { get; }

    public StringComparison ConfigValueComparer { get; }
}
