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

            builder.AppendLine("    }");
            builder.AppendLine("}");
            builder.AppendLine("#nullable restore");
            
            context.AddSource("GeneratedCommandHandler.g.cs", builder.ToString());
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
