using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace System.CommandLine.CommandHandler
{
    [Generator]
    public class CommandHandlerGenerator : ISourceGenerator
    {
        public void Execute(GeneratorExecutionContext context)
        {
            AddGeneratorClass(context);

            SyntaxReceiver rx = (SyntaxReceiver)context.SyntaxContextReceiver!;

            StringBuilder builder = new();
            builder.AppendLine(@"
#nullable enable
using System.CommandLine.Binding;
using System.Reflection;
using System.Threading.Tasks; 
namespace System.CommandLine.Invocation
{
    public static class GeneratedCommandHandler
    {
");
            foreach(var invocation in rx.Invocations)
            {
                builder.AppendLine(invocation);
            }

            builder.AppendLine(@"
    }
}
#nullable restore");
            
            //context.AddSource("GeneratedCommandHandler.g.cs", builder.ToString());
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
    }
}
");
        }

        public void Initialize(GeneratorInitializationContext context)
        {
#if DEBUG
            if (!System.Diagnostics.Debugger.IsAttached)
            {
                //System.Diagnostics.Debugger.Launch();
            }
#endif 
            
            context.RegisterForSyntaxNotifications(() => new SyntaxReceiver());
        }
    }

    public class SyntaxReceiver : ISyntaxContextReceiver
    {
        public List<string> Invocations { get; } = new();

        public void OnVisitSyntaxNode(GeneratorSyntaxContext context)
        {
            if (context.Node is InvocationExpressionSyntax invocation &&
                invocation.Expression is MemberAccessExpressionSyntax memberAccess &&
                memberAccess.Name.Identifier.Text == "Generate")
            {
                //SymbolInfo symbol = context.SemanticModel.GetSymbolInfo(invocation);
                //SymbolInfo delegateType = context.SemanticModel.GetSymbolInfo(invocation.ArgumentList.Arguments[0].Expression);
                //if (delegateType.Symbol is IMethodSymbol methodSymbol)
                //{
                //
                //}
                List<ISymbol?> symbols = invocation.ArgumentList.Arguments
                    .Skip(1)
                    .Select(x => context.SemanticModel.GetSymbolInfo(x.Expression).Symbol)
                    .ToList();
                if (symbols.Any(x => x is null)) return;

                string genericParameters = string.Join(", ", symbols.Select((x, i) => $"T{i + 1}"));
                StringBuilder sb = new();
                sb.AppendLine($"        public static ICommandHandler Generate<{genericParameters}>(Action<{string.Join(", ", symbols.Select((x, i) => $"T{i + 1}?"))}> method,");
                for (int i = 0; i < symbols.Count; i++)
                {
                    var argumentSymbol = symbols[i];
                    INamedTypeSymbol namedType;

                    string FromTypeSymbol(ITypeSymbol typeSymbol)
                    {
                        return typeSymbol switch
                        {
                            INamedTypeSymbol namedType => FromNamedType(namedType),
                            _ => typeSymbol.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat)
                        };
                    }

                    string FromNamedType(INamedTypeSymbol namedType)
                    {
                        if (namedType.TypeArguments.Length == 1)
                        {
                            var displayParts = namedType.ToDisplayParts(SymbolDisplayFormat.FullyQualifiedFormat)
                                .ToList();
                            return string.Join("", displayParts.Take(displayParts.Count - 3)) + $"<T{i + 1}>";
                        }
                        return namedType.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);
                    }

                    string type = argumentSymbol switch
                    {
                        ILocalSymbol local =>  FromTypeSymbol(local.Type),
                        _ => argumentSymbol!.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat)
                    };

                    sb.Append($"            {type} param{i + 1}");
                    if (i < symbols.Count - 1)
                    {
                        sb.Append(", ");
                    }
                    sb.AppendLine();
                }
                sb.AppendLine("            )");
                sb.AppendLine($"            => new GeneratedHandler<{genericParameters}>(method, {string.Join(", ", symbols.Select((_, i) => $"param{i + 1}"))});");


                //SymbolInfo paramType = context.SemanticModel.GetSymbolInfo(invocation.ArgumentList.Arguments[1].Expression);
                Invocations.Add(sb.ToString());
            }
            //context.Node.IsKind(Microsoft.CodeAnalysis.CSharp.SyntaxKind.)
        }
    }
}
