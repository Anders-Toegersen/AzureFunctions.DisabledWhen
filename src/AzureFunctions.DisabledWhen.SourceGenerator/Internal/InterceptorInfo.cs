namespace AzureFunctions.DisabledWhen.SourceGenerator.Internal;

internal enum BuilderType
{
    HostBuilder,
    FunctionsWorkerApplicationBuilder,
}

internal readonly record struct InterceptorInfo
{
    public InterceptorInfo(int version, string data, BuilderType builderType)
    {
        Version = version;
        Data = data;
        BuilderType = builderType;
    }

    public int Version { get; }

    public string Data { get; }

    public BuilderType BuilderType { get; }
}
