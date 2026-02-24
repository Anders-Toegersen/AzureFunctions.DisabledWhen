using System.Collections.Immutable;
using AzureFunctions.DisabledWhen.SourceGenerator.Internal;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace AzureFunctions.DisabledWhen.SourceGenerator;

[Generator]
public sealed class DisabledWhenGenerator : IIncrementalGenerator
{
    private const string DisabledWhenAttributeName = "AzureFunctions.DisabledWhen.DisabledWhenAttribute";
    private const string DisabledWhenLocalAttributeName = "AzureFunctions.DisabledWhen.DisabledWhenLocalAttribute";
    private const string DisabledWhenNullOrEmptyAttributeName = "AzureFunctions.DisabledWhen.DisabledWhenNullOrEmptyAttribute";
    private const string UseDisabledWhenMethodName = "AzureFunctions.DisabledWhen.IHostBuilderExtensions.UseDisabledWhen";
    private const string UseDisabledWhenAppBuilderMethodName = "AzureFunctions.DisabledWhen.IFunctionsWorkerApplicationBuilderExtensions.UseDisabledWhen";

    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        var disabledWhenAttributes = context.SyntaxProvider
            .ForAttributeWithMetadataName(
                DisabledWhenAttributeName,
                predicate: static (node, _) => node is MethodDeclarationSyntax,
                transform: static (ctx, ct) => ExtractDisabledWhenInfo(ctx, ct))
            .Where(static info => info.HasValue)
            .Select(static (info, _) => info!.Value)
            .Collect();

        var disabledWhenLocalAttributes = context.SyntaxProvider
            .ForAttributeWithMetadataName(
                DisabledWhenLocalAttributeName,
                predicate: static (node, _) => node is MethodDeclarationSyntax,
                transform: static (ctx, ct) => ExtractDisabledWhenLocalInfo(ctx, ct))
            .Where(static info => info.HasValue)
            .Select(static (info, _) => info!.Value)
            .Collect();

        var disabledWhenNullOrEmptyAttributes = context.SyntaxProvider
            .ForAttributeWithMetadataName(
                DisabledWhenNullOrEmptyAttributeName,
                predicate: static (node, _) => node is MethodDeclarationSyntax,
                transform: static (ctx, ct) => ExtractDisabledWhenNullOrEmptyInfo(ctx, ct))
            .Where(static info => info.HasValue)
            .Select(static (info, _) => info!.Value)
            .Collect();

        var interceptors = context.SyntaxProvider
            .CreateSyntaxProvider(
                predicate: static (node, _) => node is InvocationExpressionSyntax invocation &&
                    invocation.Expression is MemberAccessExpressionSyntax memberAccess &&
                    memberAccess.Name.Identifier.Text == "UseDisabledWhen",
                transform: static (ctx, ct) => ExtractInterceptorInfo(ctx, ct))
            .Where(static info => info.HasValue)
            .Select(static (info, _) => info!.Value)
            .Collect();

        var functions = disabledWhenAttributes
            .Combine(disabledWhenLocalAttributes)
            .Combine(disabledWhenNullOrEmptyAttributes)
            .Select(static (tuple, _) =>
            {
                var ((first, second), third) = tuple;
                var builder = ImmutableArray.CreateBuilder<FunctionDisableInfo>(first.Length + second.Length + third.Length);

                builder.AddRange(first);
                builder.AddRange(second);
                builder.AddRange(third);

                return builder.MoveToImmutable();
            });

        var combined = functions.Combine(interceptors);

        context.RegisterSourceOutput(combined, static (spc, data) =>
        {
            var (funcs, interceptorInfos) = data;

            if (funcs.IsEmpty && interceptorInfos.IsEmpty)
            {
                return;
            }

            spc.AddSource("DisabledWhenRegistry.g.cs", Emitter.GenerateRegistry(funcs, interceptorInfos));
        });
    }

    private static InterceptorInfo? ExtractInterceptorInfo(GeneratorSyntaxContext ctx, CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();

        if (ctx.Node is not InvocationExpressionSyntax invocation)
        {
            return null;
        }

        var symbolInfo = ctx.SemanticModel.GetSymbolInfo(invocation, ct);
        if (symbolInfo.Symbol is not IMethodSymbol methodSymbol)
        {
            return null;
        }

        var fullName = $"{methodSymbol.ContainingType.ToDisplayString()}.{methodSymbol.Name}";

        BuilderType builderType;
        if (fullName == UseDisabledWhenMethodName)
        {
            builderType = BuilderType.HostBuilder;
        }
        else if (fullName == UseDisabledWhenAppBuilderMethodName)
        {
            builderType = BuilderType.FunctionsWorkerApplicationBuilder;
        }
        else
        {
            return null;
        }

#pragma warning disable RSEXPERIMENTAL002
        var interceptableLocation = ctx.SemanticModel.GetInterceptableLocation(invocation, ct);
#pragma warning restore RSEXPERIMENTAL002
        if (interceptableLocation is null)
        {
            return null;
        }

        return new InterceptorInfo(interceptableLocation.Version, interceptableLocation.Data, builderType);
    }

    private static FunctionDisableInfo? ExtractDisabledWhenInfo(GeneratorAttributeSyntaxContext ctx, CancellationToken ct)
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

    private static FunctionDisableInfo? ExtractDisabledWhenLocalInfo(GeneratorAttributeSyntaxContext ctx, CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();

        if (ctx.TargetSymbol is not IMethodSymbol methodSymbol)
        {
            return null;
        }

        var condition = new DisabledCondition("AZURE_FUNCTIONS_ENVIRONMENT", "Development", StringComparison.Ordinal);
        var entryPoint = GetFullyQualifiedMethodName(methodSymbol);

        return new FunctionDisableInfo(entryPoint, [condition]);
    }

    private static FunctionDisableInfo? ExtractDisabledWhenNullOrEmptyInfo(GeneratorAttributeSyntaxContext ctx, CancellationToken ct)
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
