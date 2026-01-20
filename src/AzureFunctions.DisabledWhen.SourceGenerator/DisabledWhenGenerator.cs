using System.Collections.Immutable;
using AzureFunctions.DisabledWhen.SourceGenerator.Internal;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace AzureFunctions.DisabledWhen.SourceGenerator;

[Generator]
public sealed class DisabledWhenGenerator : IIncrementalGenerator
{
    private const string DisabledWhenAttribute = "AzureFunctions.DisabledWhen.DisabledWhenAttribute";
    private const string DisabledWhenLocalAttribute = "AzureFunctions.DisabledWhen.DisabledWhenLocalAttribute";
    private const string DisabledWhenNullOrEmptyAttribute = "AzureFunctions.DisabledWhen.DisabledWhenNullOrEmptyAttribute";

    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        var disabledWhen = context.SyntaxProvider
            .ForAttributeWithMetadataName(
                DisabledWhenAttribute,
                predicate: static (node, _) => node is MethodDeclarationSyntax,
                transform: static (ctx, ct) => ExtractFromDisabledWhen(ctx, ct))
            .Where(static info => info.HasValue)
            .Select(static (info, _) => info!.Value);

        var disabledWhenLocal = context.SyntaxProvider
            .ForAttributeWithMetadataName(
                DisabledWhenLocalAttribute,
                predicate: static (node, _) => node is MethodDeclarationSyntax,
                transform: static (ctx, ct) => ExtractFromDisabledWhenLocal(ctx, ct))
            .Where(static info => info.HasValue)
            .Select(static (info, _) => info!.Value);

        var disabledWhenNullOrEmpty = context.SyntaxProvider
            .ForAttributeWithMetadataName(
                DisabledWhenNullOrEmptyAttribute,
                predicate: static (node, _) => node is MethodDeclarationSyntax,
                transform: static (ctx, ct) => ExtractFromDisabledWhenNullOrEmpty(ctx, ct))
            .Where(static info => info.HasValue)
            .Select(static (info, _) => info!.Value);

        var combined = disabledWhen.Collect()
            .Combine(disabledWhenLocal.Collect())
            .Combine(disabledWhenNullOrEmpty.Collect())
            .Select(static (tuple, _) =>
            {
                var ((first, second), third) = tuple;
                return first.AddRange(second).AddRange(third);
            });

        context.RegisterSourceOutput(combined, static (spc, functions) =>
        {
            if (functions.IsEmpty)
            {
                return;
            }

            spc.AddSource("DisabledWhenRegistry.g.cs", Emitter.GenerateRegistry(functions));
        });
    }

    private static FunctionDisableInfo? ExtractFromDisabledWhen(GeneratorAttributeSyntaxContext ctx, CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();

        if (ctx.TargetSymbol is not IMethodSymbol methodSymbol)
        {
            return null;
        }

        var conditions = ImmutableArray.CreateBuilder<DisabledCondition>();

        foreach (var attr in ctx.Attributes)
        {
            if (attr.ConstructorArguments.Length >= 2 &&
                attr.ConstructorArguments[0].Value is string configKey)
            {
                var configValue = attr.ConstructorArguments[1].Value as string;
                var comparer = StringComparison.Ordinal;

                if (attr.ConstructorArguments.Length >= 3 &&
                    attr.ConstructorArguments[2].Value is int comparerValue)
                {
                    comparer = (StringComparison)comparerValue;
                }

                conditions.Add(new DisabledCondition(configKey, configValue, comparer));
            }
        }

        if (conditions.Count == 0)
        {
            return null;
        }

        var entryPoint = GetFullyQualifiedMethodName(methodSymbol);
        return new FunctionDisableInfo(entryPoint, conditions.ToImmutable());
    }

    private static FunctionDisableInfo? ExtractFromDisabledWhenLocal(GeneratorAttributeSyntaxContext ctx, CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();

        if (ctx.TargetSymbol is not IMethodSymbol methodSymbol)
        {
            return null;
        }

        // DisabledWhenLocalAttribute has hardcoded values
        var condition = new DisabledCondition("AZURE_FUNCTIONS_ENVIRONMENT", "Development", StringComparison.Ordinal);
        var entryPoint = GetFullyQualifiedMethodName(methodSymbol);

        return new FunctionDisableInfo(entryPoint, ImmutableArray.Create(condition));
    }

    private static FunctionDisableInfo? ExtractFromDisabledWhenNullOrEmpty(GeneratorAttributeSyntaxContext ctx, CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();

        if (ctx.TargetSymbol is not IMethodSymbol methodSymbol)
        {
            return null;
        }

        var conditions = ImmutableArray.CreateBuilder<DisabledCondition>();

        foreach (var attr in ctx.Attributes)
        {
            if (attr.ConstructorArguments.Length >= 1 &&
                attr.ConstructorArguments[0].Value is string configKey)
            {
                conditions.Add(new DisabledCondition(configKey, null, StringComparison.Ordinal, matchNullOrEmpty: true));
            }
        }

        if (conditions.Count == 0)
        {
            return null;
        }

        var entryPoint = GetFullyQualifiedMethodName(methodSymbol);
        return new FunctionDisableInfo(entryPoint, conditions.ToImmutable());
    }

    private static string GetFullyQualifiedMethodName(IMethodSymbol method)
    {
        var containingType = method.ContainingType;
        var typeFullName = containingType.ToDisplayString(
            new SymbolDisplayFormat(
                typeQualificationStyle: SymbolDisplayTypeQualificationStyle.NameAndContainingTypesAndNamespaces));
        return $"{typeFullName}.{method.Name}";
    }
}
