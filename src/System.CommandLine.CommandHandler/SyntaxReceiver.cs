using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Collections.Generic;
using System.CommandLine.CommandHandler.Invocations;
using System.CommandLine.CommandHandler.Parameters;
using System.Linq;

namespace System.CommandLine.CommandHandler
{
    public class SyntaxReceiver : ISyntaxContextReceiver
    {
        public List<DelegateInvocation> Invocations { get; } = new();

        public void OnVisitSyntaxNode(GeneratorSyntaxContext context)
        {
            if (context.Node is InvocationExpressionSyntax invocationExpression &&
                invocationExpression.Expression is MemberAccessExpressionSyntax memberAccess &&
                memberAccess.Name.Identifier.Text == "Generate" &&
                context.SemanticModel.GetSymbolInfo(invocationExpression) is { } invocationSymbol &&
                invocationSymbol.Symbol is IMethodSymbol invokeMethodSymbol &&
                invokeMethodSymbol.ReceiverType?.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat) == "global::System.CommandLine.Invocation.CommandHandlerGenerator" &&
                (invokeMethodSymbol.TypeArguments.Length == 1 || invokeMethodSymbol.TypeArguments.Length == 2))
            {
                INamedTypeSymbol? iConsole = context.SemanticModel.Compilation.GetTypeByMetadataName("System.CommandLine.IConsole");
                if (iConsole is null) return;

                IReadOnlyList<ISymbol> delegateParameters = Array.Empty<ISymbol>();
                //Check for model binding condition
                if (invokeMethodSymbol.TypeArguments[0] is INamedTypeSymbol namedDelegateType &&
                    namedDelegateType.TypeArguments.Length > 0)
                {
                    delegateParameters = namedDelegateType.TypeArguments.Cast<ISymbol>().ToList();
                }

                IReadOnlyList<ISymbol?> symbols = invocationExpression.ArgumentList.Arguments
                    .Skip(1)
                    .Select(x => context.SemanticModel.GetSymbolInfo(x.Expression).Symbol)
                    .ToList();
                if (symbols.Any(x => x is null)) return;
                IReadOnlyList<Parameter> givenParameters = GetParameters(symbols!);
                if (invokeMethodSymbol.TypeArguments.Length == 2)
                {
                    //System.Diagnostics.Debugger.Launch();
                    var rawParameter = (RawParameter)givenParameters[0];
                    var factoryParameter = new FactoryParameter(rawParameter, invokeMethodSymbol.Parameters[1].Type);
                    givenParameters = new Parameter[] { factoryParameter }.Concat(givenParameters.Skip(1)).ToList();
                }

                SymbolEqualityComparer symbolEqualityComparer = SymbolEqualityComparer.Default;
                HashSet<ISymbol> knownTypes = new(symbolEqualityComparer);
                knownTypes.Add(iConsole);

                if (IsMatch(delegateParameters, givenParameters, knownTypes))
                {
                    if (invokeMethodSymbol.TypeArguments.Length == 2)
                    {
                        var invocation = new FactoryModelBindingInvocation(invokeMethodSymbol.TypeArguments[0]);
                        foreach (var parameter in PopulateParameters(delegateParameters, givenParameters, iConsole))
                        {
                            invocation.Parameters.Add(parameter);
                        }
                        Invocations.Add(invocation);
                    }
                    else
                    {
                        var invocation = new DelegateInvocation(invokeMethodSymbol.TypeArguments[0], 1);
                        foreach (var parameter in PopulateParameters(delegateParameters, givenParameters, iConsole))
                        {
                            invocation.Parameters.Add(parameter);
                        }
                        Invocations.Add(invocation);
                    }
                }
                else if (delegateParameters[0] is INamedTypeSymbol modelType)
                {
                    foreach (var ctor in modelType.Constructors.OrderByDescending(x => x.Parameters.Length))
                    {
                        var targetTypes =
                            ctor.Parameters.Select(x => x.Type)
                            .Concat(delegateParameters.Skip(1))
                            .ToList();
                        if (IsMatch(targetTypes, givenParameters, knownTypes))
                        {
                            var invocation = new ConstructorModelBindingInvocation(ctor, invokeMethodSymbol.TypeArguments[0]);
                            foreach (var parameter in PopulateParameters(targetTypes, givenParameters, iConsole))
                            {
                                invocation.Parameters.Add(parameter);
                            }
                            Invocations.Add(invocation);
                            break;
                        }
                    }
                }

                static bool IsMatch(
                    IReadOnlyList<ISymbol> targetSymbols,
                    IReadOnlyList<Parameter> providedSymbols,
                    HashSet<ISymbol> knownTypes)
                {
                    SymbolEqualityComparer symbolEqualityComparer = SymbolEqualityComparer.Default;

                    for (int i = 0, j = 0; i < targetSymbols.Count; i++)
                    {
                        if (j < providedSymbols.Count &&
                            symbolEqualityComparer.Equals(providedSymbols[j].ValueType, targetSymbols[i]))
                        {
                            j++;
                            //TODO: Handle the case where there are more provided symbols than needed
                        }
                        else if (!knownTypes.Contains(targetSymbols[i]))
                        {
                            return false;
                        }
                    }
                    return true;
                }
            }
        }

        private static IReadOnlyList<Parameter> PopulateParameters(
            IReadOnlyList<ISymbol> symbols,
            IReadOnlyList<Parameter> givenParameters,
            ITypeSymbol iConsole)
        {
            SymbolEqualityComparer symbolEqualityComparer = SymbolEqualityComparer.Default;
            List<Parameter> parameters = new(givenParameters);
            for (int i = 0; i < symbols.Count; i++)
            {
                if (symbolEqualityComparer.Equals(iConsole, symbols[i]))
                {
                    parameters.Insert(i, new ConsoleParameter(iConsole));
                }
            }
            return parameters;
        }

        private static IReadOnlyList<Parameter> GetParameters(IEnumerable<ISymbol> symbols)
        {
            List<Parameter> parameters = new();
            int parameterIndex = 1;
            foreach (ISymbol symbol in symbols)
            {
                parameters.Add(GetParameter(symbol, $"Param{parameterIndex}"));
                parameterIndex++;
            }
            return parameters;
        }

        private static Parameter GetParameter(ISymbol argumentSymbol, string localName)
        {
            return argumentSymbol switch
            {
                ILocalSymbol local => FromTypeSymbol(local.Type),
                INamedTypeSymbol namedType => FromNamedTypeSymbol(namedType),
                IMethodSymbol methodSymbol => FromTypeSymbol(methodSymbol.ReturnType),
                _ => throw new NotImplementedException($"Cannot convert from '{argumentSymbol?.Kind}' {argumentSymbol?.ToDisplayString()}")
            };

            Parameter FromNamedTypeSymbol(INamedTypeSymbol namedTypeSymbol)
            {
                if (namedTypeSymbol.TypeArguments.Length > 0)
                {
                    return new OptionParameter(localName, namedTypeSymbol, namedTypeSymbol.TypeArguments[0]);
                }
                return new RawParameter(localName, namedTypeSymbol);
            }

            Parameter FromTypeSymbol(ITypeSymbol typeSymbol)
            {
                return typeSymbol switch
                {
                    INamedTypeSymbol namedType => FromNamedTypeSymbol(namedType),
                    _ => throw new NotImplementedException($"Cannot convert from type symbol '{typeSymbol?.Kind}' {typeSymbol?.ToDisplayString()}")
                };
            }
        }
    }
}
