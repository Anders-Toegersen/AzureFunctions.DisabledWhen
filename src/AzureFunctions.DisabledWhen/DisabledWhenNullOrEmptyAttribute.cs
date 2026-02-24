namespace AzureFunctions.DisabledWhen;

/// <summary>
/// Disables a function when a configuration key is missing or empty.
/// </summary>
[AttributeUsage(AttributeTargets.Method, AllowMultiple = true, Inherited = false)]
public sealed class DisabledWhenNullOrEmptyAttribute : Attribute
{
    /// <param name="configKey">The configuration key to evaluate.</param>
    public DisabledWhenNullOrEmptyAttribute(string configKey)
    {
        ConfigKey = configKey;
    }

    /// <summary>The configuration key to evaluate.</summary>
    public string ConfigKey { get; }
}
