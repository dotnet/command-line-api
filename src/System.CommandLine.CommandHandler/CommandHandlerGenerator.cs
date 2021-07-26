using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace System.CommandLine.CommandHandler
{
    [Generator]
    public class CommandHandlerGenerator : ISourceGenerator
    {
        private const string ICommandHandlerType = "System.CommandLine.Invocation.ICommandHandler";

        public void Execute(GeneratorExecutionContext context)
        {
            //AddGeneratorClass(context);

            SyntaxReceiver rx = (SyntaxReceiver)context.SyntaxContextReceiver!;

            StringBuilder builder = new();
            builder.AppendLine(@"
// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#nullable enable
using System.CommandLine.Binding;
using System.Reflection;
using System.Threading.Tasks;

namespace System.CommandLine.Invocation
{
    public static partial class CommandHandlerGeneratorExtensions_Generated
    {
");
            int count = 1;
            foreach (var invocation in rx.Invocations)
            {
                var methodParamters = invocation.Parameters
                    .Select(x => x.GetMethodParameter())
                    .Where(x => !string.IsNullOrWhiteSpace(x.Name))
                    .ToList();

                builder.AppendLine(@$"public static {ICommandHandlerType} Generate<TUnused>(this CommandHandlerGenerator handler,");
                builder.AppendLine($"{invocation.DelegateType.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat)} method,");
                builder.AppendLine(string.Join($", ", methodParamters.Select(x => $"{x.Type} {x.Name}")));
                builder.AppendLine(")");
                builder.AppendLine("{");
                builder.AppendLine($"return new GeneratedHandler_{count}(method, {string.Join(", ", methodParamters.Select(x => x.Name))});");
                builder.AppendLine("}");


                //TODO: fully qualify type names
                builder.AppendLine($@"
        private class GeneratedHandler_{count} : {ICommandHandlerType}
        {{
            public GeneratedHandler_{count}({invocation.DelegateType} method,");

                builder.AppendLine(string.Join($", ", methodParamters.Select(x => $"{x.Type} {x.Name}")));

                builder.AppendLine($@")
            {{
                Method = method;");
                foreach (var propertyAssignment in invocation.Parameters
                    .Select(x => x.GetPropertyAssignment())
                    .Where(x => !string.IsNullOrWhiteSpace(x)))
                {
                    builder.AppendLine(propertyAssignment);
                }
                builder.AppendLine($@"
            }}
                
            public {invocation.DelegateType} Method {{ get; }}");

                foreach (var propertyDeclaration in invocation.Parameters
                    .Select(x => x.GetPropertyDeclaration())
                    .Where(x => !string.IsNullOrWhiteSpace(x)))
                {
                    builder.AppendLine(propertyDeclaration);
                }

                builder.AppendLine($@"
            public Task<int> InvokeAsync(InvocationContext context)
            {{");
                builder.AppendLine(invocation.InvokeContents());
                builder.AppendLine($@"
            }}
        }}");
                count++;
            }

            builder.AppendLine(@"
    }
}
#nullable restore");

            context.AddSource("CommandHandlerGeneratorExtensions_Generated.g.cs", builder.ToString());
        }



        private static void AddGeneratorClass(GeneratorExecutionContext context)
        {
            context.AddSource("GeneratedCommandHandler2.g.cs", @"
// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.CommandLine.Binding;
using System.Reflection;
using System.Threading.Tasks;

namespace System.CommandLine.Invocation
{
    public static class CommandHandlerGeneratorExtensions_Generated
    {
private class GeneratedHandler_1 : ICommandHandler
{
    public GeneratedHandler_1(Action<string, IConsole, int> method,
    Option<string> param1,
    Option<int> param2)
    {
        Method = method;
        Param1 = param1;
        Param2 = param2;
    }
        
    public Action<string, IConsole, int> Method { get; }
    public Option<string> Param1 { get; }
    public Option<int> Param2 { get; }
        
    public Task<int> InvokeAsync(InvocationContext context)
    {
        string value1 = context.ParseResult.ValueForOption(Param1);
        int value2 = context.ParseResult.ValueForOption(Param2);
        
        Method.Invoke(value1, context.Console, value2);
        
        return Task.FromResult(0);
    }
}

        private class GeneratedHandler_2 : ICommandHandler
        {
            public GeneratedHandler_2(Action<System.CommandLine.Tests.Invocation.CommandHandlerTests.Character, IConsole> method,
            Option<string> param1,
            Option<int> param2)
            {
                Method = method;
                Param1 = param1;
                Param2 = param2;
            }
        
            public Action<System.CommandLine.Tests.Invocation.CommandHandlerTests.Character, IConsole> Method { get; }
            public Option<string> Param1 { get; }
            public Option<int> Param2 { get; }
        
            public Task<int> InvokeAsync(InvocationContext context)
            {
                string value1 = context.ParseResult.ValueForOption(Param1);
                int value2 = context.ParseResult.ValueForOption(Param2);
                System.CommandLine.Tests.Invocation.CommandHandlerTests.Character model = new(value1, value2);
                Method.Invoke(model, context.Console);
        
                return Task.FromResult(0);
            }
        }

        private class GeneratedHandler_3 : ICommandHandler
        {
            public GeneratedHandler_3(Action<System.CommandLine.Tests.Invocation.CommandHandlerTests.Character, IConsole> method,
            Func<InvocationContext, System.CommandLine.Tests.Invocation.CommandHandlerTests.Character> modelBuilder)
            {
                Method = method;
                ModelBuilder = modelBuilder;
            }
        
            public Action<System.CommandLine.Tests.Invocation.CommandHandlerTests.Character, IConsole> Method { get; }
            public Func<InvocationContext, System.CommandLine.Tests.Invocation.CommandHandlerTests.Character> ModelBuilder { get; }
        
            public Task<int> InvokeAsync(InvocationContext context)
            {
                System.CommandLine.Tests.Invocation.CommandHandlerTests.Character model = ModelBuilder(context);
                Method.Invoke(model, context.Console);
        
                return Task.FromResult(0);
            }
        }

        public static ICommandHandler Generate<TUnused>(this CommandHandlerGenerator handler, 
            Action<string, IConsole, int> method,
            Option<string> param1, Option<int> param2)
        {
            return new GeneratedHandler_1(method, param1, param2);
        }

        public static ICommandHandler Generate<TUnused>(this CommandHandlerGenerator handler, 
            Action<System.CommandLine.Tests.Invocation.CommandHandlerTests.Character, IConsole> method,
            Option<string> param1, Option<int> param2)
        {
            return new GeneratedHandler_2(method, param1, param2);
        }

        public static ICommandHandler Generate<TUnused, TUnused2>(this CommandHandlerGenerator handler, 
            Action<System.CommandLine.Tests.Invocation.CommandHandlerTests.Character, IConsole> method,
            Func<InvocationContext, System.CommandLine.Tests.Invocation.CommandHandlerTests.Character> modelBuilder)
        {
            return new GeneratedHandler_3(method, modelBuilder);
        }
    }
}
");
        }

        public void Initialize(GeneratorInitializationContext context)
        {
#if DEBUG
            if (!System.Diagnostics.Debugger.IsAttached)
            {
                System.Diagnostics.Debugger.Launch();
            }
#endif

            context.RegisterForSyntaxNotifications(() => new SyntaxReceiver());
        }
    }

    public class ConstructorModelBindingInvocation : DelegateInvocation
    {
        public ConstructorModelBindingInvocation(ITypeSymbol delegateType)
            : base(delegateType)
        {
        }
    }

    public class DelegateInvocation
    {
        public ITypeSymbol DelegateType { get; }

        public DelegateInvocation(ITypeSymbol delegateType)
        {
            DelegateType = delegateType;
        }

        public List<Parameter> Parameters { get; } = new();

        public string InvokeContents()
        {
            StringBuilder builder = new();
            //NB: Should invoke and return Task<int>
            /*
             * Method.Invoke(value1, context.Console, value2);
        
            return Task.FromResult(0);
             */
            builder.Append("Method.Invoke(");
            builder.Append(string.Join(", ", Parameters.Select(x => x.GetValueFromContext())));
            builder.AppendLine(");");
            builder.AppendLine("return Task.FromResult(0);");
            return builder.ToString();
            //return $@"Method.Invoke(value1, context.Console, value2);";


        }
    }

    public abstract class Parameter
    {
        public ITypeSymbol ValueType { get; }

        protected Parameter(ITypeSymbol valueType)
        {
            ValueType = valueType;
        }

        public abstract string GetValueFromContext();

        public virtual string GetPropertyDeclaration() => "";
        public virtual string GetPropertyAssignment() => "";
        public virtual (string Type, string Name) GetMethodParameter() => ("", "");
    }

    //public class Argument : Parameter
    //{
    //    public Argument(string type, string valueType) 
    //        : base(type, valueType)
    //    {
    //    }
    //
    //    public override string GetValueFromContext()
    //    {
    //        throw new NotImplementedException();
    //    }
    //}

    public class Option : Parameter
    {
        public Option(string localName, INamedTypeSymbol type, ITypeSymbol valueType)
            : base(valueType)
        {
            LocalName = localName;
            Type = type;
        }

        public INamedTypeSymbol Type { get; }

        public string LocalName { get; }

        private string ParameterName => LocalName.ToLowerInvariant();

        public override string GetValueFromContext()
            => $"context.ParseResult.ValueForOption({LocalName})";

        public override string GetPropertyDeclaration()
            => $"private {Type.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat)} {LocalName} {{ get; }}";

        public override string GetPropertyAssignment()
            => $"{LocalName} = {ParameterName};";

        public override (string Type, string Name) GetMethodParameter()
            => (Type.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat), ParameterName);
    }

    public class Console : Parameter
    {
        public Console(ITypeSymbol consoleType)
            : base(consoleType)
        {
        }

        public override string GetValueFromContext()
            => "context.Console";
    }

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
                invokeMethodSymbol.TypeArguments.Length == 1)
            {
                INamedTypeSymbol? iConsole = context.SemanticModel.Compilation.GetTypeByMetadataName("System.CommandLine.IConsole");
                if (iConsole is null) return;

                IList<ISymbol> delegateParameters = Array.Empty<ISymbol>();
                //Check for model binding condition
                if (invokeMethodSymbol.TypeArguments[0] is INamedTypeSymbol namedDelegateType &&
                    namedDelegateType.TypeArguments.Length > 0)
                {
                    delegateParameters = namedDelegateType.TypeArguments.Cast<ISymbol>().ToList();
                }

                List<ISymbol?> symbols = invocationExpression.ArgumentList.Arguments
                    .Skip(1)
                    .Select(x => context.SemanticModel.GetSymbolInfo(x.Expression).Symbol)
                    .ToList();
                if (symbols.Any(x => x is null)) return;
                var symbolTypes = symbols.Select(x => GetType(x!)).ToList();

                SymbolEqualityComparer symbolEqualityComparer = SymbolEqualityComparer.Default;
                HashSet<ISymbol> knownTypes = new(symbolEqualityComparer);

                if (IsMatch(delegateParameters, symbolTypes, knownTypes))
                {
                    var invocation = new DelegateInvocation(invokeMethodSymbol.TypeArguments[0]);
                    foreach (var parameter in GetParameters(delegateParameters, iConsole))
                    {
                        invocation.Parameters.Add(parameter);
                    }
                    Invocations.Add(invocation);
                }
                else if (delegateParameters[0] is INamedTypeSymbol modelType)
                {
                    foreach (var ctor in modelType.Constructors)
                    {
                        var targetTypes =
                            ctor.Parameters.Select(x => x.Type)
                            .Concat(delegateParameters.Skip(1))
                            .ToList();
                        if (IsMatch(targetTypes, symbolTypes, knownTypes))
                        {
                            var invocation = new ConstructorModelBindingInvocation(modelType);
                            foreach (var parameter in GetParameters(targetTypes, iConsole))
                            {
                                invocation.Parameters.Add(parameter);
                            }
                            Invocations.Add(invocation);
                        }
                    }
                }

                static List<Parameter> GetParameters(IEnumerable<ISymbol> symbols,
                    ITypeSymbol iConsole)
                {
                    SymbolEqualityComparer symbolEqualityComparer = SymbolEqualityComparer.Default;
                    List<Parameter> parameters = new();
                    int parameterIndex = 1;
                    foreach (ISymbol symbol in symbols)
                    {
                        if (symbolEqualityComparer.Equals(iConsole, symbol))
                        {
                            parameters.Add(new Console(iConsole));
                        }
                        else
                        {
                            parameters.Add(GetParameter(symbol, $"Param{parameterIndex}"));
                            parameterIndex++;
                        }
                    }
                    return parameters;
                }

                static bool IsMatch(
                    IList<ISymbol> targetSymbols,
                    IList<ITypeSymbol> providedSymbols,
                    HashSet<ISymbol> knownTypes)
                {
                    SymbolEqualityComparer symbolEqualityComparer = SymbolEqualityComparer.Default;
                    for (int i = 0; i < targetSymbols.Count; i++)
                    {
                        if (symbolEqualityComparer.Equals(providedSymbols[i], targetSymbols[i]) == false
                            && !knownTypes.Contains(targetSymbols[i]))
                        {
                            return false;
                        }
                    }
                    return true;
                }
            }
        }

        private static ITypeSymbol GetType(ISymbol argumentSymbol)
        {
            return argumentSymbol switch
            {
                ILocalSymbol local => local.Type,
                _ => throw new NotImplementedException($"Cannot get type from '{argumentSymbol?.Kind}' {argumentSymbol?.ToDisplayString()}")
            };
        }

        private static Parameter GetParameter(ISymbol argumentSymbol, string localName)
        {
            return argumentSymbol switch
            {
                ILocalSymbol local => FromTypeSymbol(local.Type),
                _ => throw new NotImplementedException($"Cannot convert from '{argumentSymbol?.Kind}' {argumentSymbol?.ToDisplayString()}")
            };

            Parameter FromNamedTypeSymbol(INamedTypeSymbol namedTypeSymbol)
            {
                return new Option(localName, namedTypeSymbol, namedTypeSymbol.TypeArguments[0]);
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
