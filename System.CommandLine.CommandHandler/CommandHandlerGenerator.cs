using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Generic;
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
            
            context.AddSource("GeneratedCommandHandler.g.cs", builder.ToString());
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

    public class SyntaxReceiver : ISyntaxContextReceiver
    {
        public List<string> Invocations { get; } = new();

        public void OnVisitSyntaxNode(GeneratorSyntaxContext context)
        {
            if (context.Node is InvocationExpressionSyntax invocation &&
                invocation.Expression is MemberAccessExpressionSyntax memberAccess &&
                memberAccess.Name.Identifier.Text == "Generate")
            {
                SymbolInfo symbol = context.SemanticModel.GetSymbolInfo(invocation);
                SymbolInfo delegateType = context.SemanticModel.GetSymbolInfo(invocation.ArgumentList.Arguments[0].Expression);
                if (delegateType.Symbol is IMethodSymbol methodSymbol)
                {

                }
                SymbolInfo paramType = context.SemanticModel.GetSymbolInfo(invocation.ArgumentList.Arguments[1].Expression);
            }
            //context.Node.IsKind(Microsoft.CodeAnalysis.CSharp.SyntaxKind.)
        }

    }
}
