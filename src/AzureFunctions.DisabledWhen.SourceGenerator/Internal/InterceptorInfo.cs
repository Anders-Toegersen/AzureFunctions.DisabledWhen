namespace AzureFunctions.DisabledWhen.SourceGenerator.Internal;

internal readonly record struct InterceptorInfo
{
    public InterceptorInfo(int version, string data)
    {
        Version = version;
        Data = data;
    }

    public int Version { get; }

    public string Data { get; }
}
