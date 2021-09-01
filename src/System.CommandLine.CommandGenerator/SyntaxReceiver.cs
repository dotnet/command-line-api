﻿using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Collections.Generic;
using System.CommandLine.CommandGenerator.Invocations;
using System.CommandLine.CommandGenerator.Parameters;
using System.Linq;

namespace System.CommandLine.CommandGenerator
{

    internal class SyntaxReceiver : ISyntaxContextReceiver
    {
        public HashSet<DelegateInvocation> Invocations { get; } = new();

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
                SymbolEqualityComparer symbolEqualityComparer = SymbolEqualityComparer.Default;
                WellKnownTypes wellKnonwTypes = new(context.SemanticModel.Compilation, symbolEqualityComparer);
                
                IReadOnlyList<Microsoft.CodeAnalysis.ISymbol> delegateParameters = Array.Empty<Microsoft.CodeAnalysis.ISymbol>();
                //Check for model binding condition
                if (invokeMethodSymbol.TypeArguments[0] is INamedTypeSymbol namedDelegateType &&
                    namedDelegateType.TypeArguments.Length > 0)
                {
                    if (namedDelegateType.DelegateInvokeMethod?.ReturnsVoid == false)
                    {
                        delegateParameters = namedDelegateType.TypeArguments
                            .Take(namedDelegateType.TypeArguments.Length - 1).Cast<Microsoft.CodeAnalysis.ISymbol>().ToList();
                    }
                    else
                    {
                        delegateParameters = namedDelegateType.TypeArguments.Cast<Microsoft.CodeAnalysis.ISymbol>().ToList();
                    }
                }

                IReadOnlyList<Microsoft.CodeAnalysis.ISymbol?> symbols = invocationExpression.ArgumentList.Arguments
                    .Skip(1)
                    .Select(x => context.SemanticModel.GetSymbolInfo(x.Expression).Symbol)
                    .ToList();
                if (symbols.Any(x => x is null)) return;
                IReadOnlyList<Parameter> givenParameters = GetParameters(symbols!);
                if (invokeMethodSymbol.TypeArguments.Length == 2)
                {
                    var rawParameter = (RawParameter)givenParameters[0];
                    var factoryParameter = new FactoryParameter(rawParameter, invokeMethodSymbol.Parameters[1].Type);
                    givenParameters = new Parameter[] { factoryParameter }.Concat(givenParameters.Skip(1)).ToList();
                }

                ITypeSymbol delegateType = invokeMethodSymbol.TypeArguments[0];
                ReturnPattern returnPattern = GetReturnPattern(delegateType, context.SemanticModel.Compilation);

                
                if (IsMatch(delegateParameters, givenParameters, wellKnonwTypes))
                {
                    if (invokeMethodSymbol.TypeArguments.Length == 2)
                    {
                        var invocation = new FactoryModelBindingInvocation(delegateType, returnPattern);
                        foreach (var parameter in PopulateParameters(delegateParameters, givenParameters, wellKnonwTypes))
                        {
                            invocation.Parameters.Add(parameter);
                        }
                        Invocations.Add(invocation);
                    }
                    else
                    {
                        var invocation = new DelegateInvocation(delegateType, returnPattern, 1);
                        foreach (var parameter in PopulateParameters(delegateParameters, givenParameters, wellKnonwTypes))
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
                        if (IsMatch(targetTypes, givenParameters, wellKnonwTypes))
                        {
                            var invocation = new ConstructorModelBindingInvocation(ctor, returnPattern, delegateType);
                            foreach (var parameter in PopulateParameters(targetTypes, givenParameters, wellKnonwTypes))
                            {
                                invocation.Parameters.Add(parameter);
                            }
                            Invocations.Add(invocation);
                            break;
                        }
                    }
                }

                static bool IsMatch(
                    IReadOnlyList<Microsoft.CodeAnalysis.ISymbol> targetSymbols,
                    IReadOnlyList<Parameter> providedSymbols,
                    WellKnownTypes knownTypes)
                {
                    SymbolEqualityComparer symbolEqualityComparer = SymbolEqualityComparer.Default;
                    int j = 0;
                    for (int i = 0; i < targetSymbols.Count; i++)
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
                    return j == providedSymbols.Count;
                }
            }
        }

        private static IReadOnlyList<Parameter> PopulateParameters(
            IReadOnlyList<Microsoft.CodeAnalysis.ISymbol> symbols,
            IReadOnlyList<Parameter> givenParameters,
            WellKnownTypes knownTypes)
        {
            List<Parameter> parameters = new(givenParameters);
            for (int i = 0; i < symbols.Count; i++)
            {
                if (knownTypes.TryGet(symbols[i], out Parameter? parameter))
                {
                    parameters.Insert(i, parameter!);
                }
            }
            return parameters;
        }

        private static IReadOnlyList<Parameter> GetParameters(IEnumerable<Microsoft.CodeAnalysis.ISymbol> symbols)
        {
            List<Parameter> parameters = new();
            int parameterIndex = 1;
            foreach (Microsoft.CodeAnalysis.ISymbol symbol in symbols)
            {
                parameters.Add(GetParameter(symbol, $"Param{parameterIndex}"));
                parameterIndex++;
            }
            return parameters;
        }

        private static Parameter GetParameter(Microsoft.CodeAnalysis.ISymbol argumentSymbol, string localName)
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
                    if (namedTypeSymbol.Name == "Option")
                    {
                        return new OptionParameter(localName, namedTypeSymbol, namedTypeSymbol.TypeArguments[0]);
                    }
                    else if (namedTypeSymbol.Name == "Argument")
                    {
                        return new ArgumentParameter(localName, namedTypeSymbol, namedTypeSymbol.TypeArguments[0]);
                    }
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

        private static ReturnPattern GetReturnPattern(ITypeSymbol delegateType, Compilation compilation)
        {
            ITypeSymbol? returnType = null;
            if (delegateType is INamedTypeSymbol namedSymbol &&
                namedSymbol.DelegateInvokeMethod is { } delegateInvokeMethod &&
                !delegateInvokeMethod.ReturnsVoid)
            {
                returnType = delegateInvokeMethod.ReturnType;
            }

            if (returnType is null)
            {
                return ReturnPattern.InvocationContextExitCode;
            }

            SymbolEqualityComparer symbolEqualityComparer = SymbolEqualityComparer.Default;
            
            INamedTypeSymbol intType = compilation.GetSpecialType(SpecialType.System_Int32);
            if (symbolEqualityComparer.Equals(returnType, intType))
            {
                return ReturnPattern.FunctionReturnValue;
            }

            //TODO: what about toher awaiatables?
            INamedTypeSymbol taskType = compilation.GetTypeByMetadataName("System.Threading.Tasks.Task")
                ?? throw new InvalidOperationException("Failed to find Task");
            if (symbolEqualityComparer.Equals(returnType, taskType))
            {
                return ReturnPattern.AwaitFunction;
            }

            INamedTypeSymbol taskOfTType = compilation.GetTypeByMetadataName("System.Threading.Tasks.Task`1")
                ?? throw new InvalidOperationException("Failed to find Task<T>");

            if (returnType is INamedTypeSymbol namedReturnType &&
                namedReturnType.TypeArguments.Length == 1 &&
                symbolEqualityComparer.Equals(namedReturnType.TypeArguments[0], intType) &&
                symbolEqualityComparer.Equals(namedReturnType.ConstructUnboundGenericType(), taskOfTType.ConstructUnboundGenericType()))
            {
                 return ReturnPattern.AwaitFunctionReturnValue;
            }
            //TODO: Should this be an error?
            return ReturnPattern.InvocationContextExitCode;
        }
    }
}
