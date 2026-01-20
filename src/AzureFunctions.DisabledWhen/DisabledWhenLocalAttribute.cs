namespace AzureFunctions.DisabledWhen;

[AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = false)]
public sealed class DisabledWhenLocalAttribute : Attribute
{
}
