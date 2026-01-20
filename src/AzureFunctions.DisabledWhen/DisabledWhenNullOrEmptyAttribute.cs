namespace AzureFunctions.DisabledWhen;

[AttributeUsage(AttributeTargets.Method, AllowMultiple = true, Inherited = false)]
public sealed class DisabledWhenNullOrEmptyAttribute : Attribute
{
    public DisabledWhenNullOrEmptyAttribute(string configKey)
    {
        ConfigKey = configKey;
    }

    public string ConfigKey { get; }
}
