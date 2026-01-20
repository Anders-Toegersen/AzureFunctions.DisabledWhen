using System.Collections.Immutable;

namespace AzureFunctions.DisabledWhen.SourceGenerator.Internal;

internal readonly struct FunctionDisableInfo : IEquatable<FunctionDisableInfo>
{
    public FunctionDisableInfo(string entryPoint, ImmutableArray<DisabledCondition> conditions)
    {
        EntryPoint = entryPoint;
        Conditions = conditions;
    }

    public string EntryPoint { get; }

    public ImmutableArray<DisabledCondition> Conditions { get; }

    public bool Equals(FunctionDisableInfo other) =>
        EntryPoint == other.EntryPoint &&
        Conditions.SequenceEqual(other.Conditions);

    public override bool Equals(object? obj) => obj is FunctionDisableInfo other && Equals(other);

    public override int GetHashCode()
    {
        var hash = new HashCode();
        hash.Add(EntryPoint);
        foreach (var condition in Conditions)
        {
            hash.Add(condition);
        }
        return hash.ToHashCode();
    }
}
