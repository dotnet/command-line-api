// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Generic;
using System.CommandLine.Generator.Invocations;
using System.CommandLine.Generator.Parameters;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace System.CommandLine.Generator
{
    internal class SyntaxReceiver : ISyntaxContextReceiver
    {
        private static readonly string _nameOfExtensionMethodAnchorType = "global::System.CommandLine.CliCommand";

        public HashSet<DelegateInvocation> Invocations { get; } = new();

        public void OnVisitSyntaxNode(GeneratorSyntaxContext context)
        {
            if (context.Node is not InvocationExpressionSyntax { Expression: MemberAccessExpressionSyntax memberAccess } invocationExpression)
            {
                return;
            }

            if (memberAccess.Name.Identifier.Text != "SetHandler")
            {
                return;
            }

            if (context.SemanticModel.GetSymbolInfo(invocationExpression) is not { Symbol: IMethodSymbol invokeMethodSymbol })
            {
                return;
            }

            if (invokeMethodSymbol.ReceiverType?.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat) != _nameOfExtensionMethodAnchorType)
            {
                return;
            }

            if (invokeMethodSymbol.TypeArguments.Length is not 1)
            {
                return;
            }

            SymbolEqualityComparer symbolEqualityComparer = SymbolEqualityComparer.Default;
            WellKnownTypes wellKnownTypes = new(context.SemanticModel.Compilation, symbolEqualityComparer);

            var delegateParameters = Array.Empty<ISymbol>();

            //Check for model binding condition
            if (invokeMethodSymbol.TypeArguments[0] is INamedTypeSymbol { TypeArguments: { Length: > 0 } } namedDelegateType)
            {
                if (namedDelegateType.DelegateInvokeMethod?.ReturnsVoid == false)
                {
                    delegateParameters = namedDelegateType.TypeArguments
                                                          .Take(namedDelegateType.TypeArguments.Length - 1)
                                                          .Cast<ISymbol>()
                                                          .ToArray();
                }
                else
                {
                    delegateParameters = namedDelegateType.TypeArguments
                                                          .Cast<ISymbol>()
                                                          .ToArray();
                }
            }

            var symbols = invocationExpression.ArgumentList
                                              .Arguments
                                              .Skip(1)
                                              .Select(x => context.SemanticModel.GetSymbolInfo(x.Expression).Symbol)
                                              .ToArray();

            if (symbols.Any(x => x is null))
            {
                return;
            }

            IReadOnlyList<Parameter> givenParameters = GetParameters(symbols!);
            ITypeSymbol delegateType = invokeMethodSymbol.TypeArguments[0];
            ReturnPattern returnPattern = GetReturnPattern(delegateType, context.SemanticModel.Compilation);

            if (IsMatch(delegateParameters, givenParameters, wellKnownTypes))
            {

                var invocation = new DelegateInvocation(delegateType, returnPattern, 1);
                foreach (var parameter in PopulateParameters(delegateParameters, givenParameters, wellKnownTypes))
                {
                    invocation.Parameters.Add(parameter);
                }

                Invocations.Add(invocation);
            }
            else if (delegateParameters[0] is INamedTypeSymbol modelType)
            {
                foreach (var ctor in modelType.Constructors.OrderByDescending(x => x.Parameters.Length))
                {
                    var targetTypes =
                        ctor.Parameters.Select(x => x.Type)
                            .Concat(delegateParameters.Skip(1))
                            .ToArray();
                    if (IsMatch(targetTypes, givenParameters, wellKnownTypes))
                    {
                        var invocation = new ConstructorModelBindingInvocation(ctor, returnPattern, delegateType);
                        foreach (var parameter in PopulateParameters(targetTypes, givenParameters, wellKnownTypes))
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

        private static IReadOnlyList<Parameter> PopulateParameters(
            IReadOnlyList<ISymbol> symbols,
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
                    if (namedTypeSymbol.Name == "CliOption")
                    {
                        return new OptionParameter(localName, namedTypeSymbol, namedTypeSymbol.TypeArguments[0]);
                    }

                    if (namedTypeSymbol.Name == "CliArgument")
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
            if (delegateType is INamedTypeSymbol { DelegateInvokeMethod: { ReturnsVoid: false } delegateInvokeMethod })
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

            //TODO: what about other awaitables?
            INamedTypeSymbol taskType = compilation.GetTypeByMetadataName("System.Threading.Tasks.Task")
                                        ?? throw new InvalidOperationException("Failed to find Task");
            if (symbolEqualityComparer.Equals(returnType, taskType))
            {
                return ReturnPattern.AwaitFunction;
            }

            INamedTypeSymbol taskOfTType = compilation.GetTypeByMetadataName("System.Threading.Tasks.Task`1")
                                           ?? throw new InvalidOperationException("Failed to find Task<T>");

            if (returnType is INamedTypeSymbol { TypeArguments: { Length: 1 } } namedReturnType && symbolEqualityComparer.Equals(namedReturnType.TypeArguments[0], intType) &&
                symbolEqualityComparer.Equals(namedReturnType.ConstructUnboundGenericType(), taskOfTType.ConstructUnboundGenericType()))
            {
                return ReturnPattern.AwaitFunctionReturnValue;
            }

            //TODO: Should this be an error?
            return ReturnPattern.InvocationContextExitCode;
        }
    }
}