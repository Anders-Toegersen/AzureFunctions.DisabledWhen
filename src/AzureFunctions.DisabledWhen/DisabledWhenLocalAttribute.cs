namespace AzureFunctions.DisabledWhen;

/// <summary>
/// Disables a function when <c>AZURE_FUNCTIONS_ENVIRONMENT</c> is <c>Development</c>.
/// </summary>
[AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = false)]
public sealed class DisabledWhenLocalAttribute : Attribute
{
}
