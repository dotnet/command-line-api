// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.CommandLine.Generator.Invocations;
using System.Linq;
using System.Text;
using Microsoft.CodeAnalysis;

namespace System.CommandLine.Generator
{
    [Generator]
    public class CommandHandlerSourceGenerator : ISourceGenerator
    {
        private const string CliActionType = "System.CommandLine.CliAction";

        public void Execute(GeneratorExecutionContext context)
        {
            SyntaxReceiver rx = (SyntaxReceiver)context.SyntaxContextReceiver!;

            if (rx.Invocations.Count == 0)
            {
                return;
            }

            StringBuilder builder = new();
            builder.Append(
$@"// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.ComponentModel;
using System.CommandLine.Binding;
using System.Reflection;
using System.Threading.Tasks;
using System.CommandLine.Invocation;

#pragma warning disable

namespace System.CommandLine
{{
    [global::System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
    [global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)]
    internal static class GeneratedCommandHandlers
    {{
");
            int handlerCount = 1;

            foreach (var invocation in rx.Invocations)
            {
                var methodParameters = GetMethodParameters(invocation);

                GenerateSetHandler(builder, invocation, methodParameters, handlerCount, true);
                //The non-geric overload is to support C# 10 natural type lambdas
                GenerateSetHandler(builder, invocation, methodParameters, handlerCount, false);

                GenerateHandlerClass(builder, invocation, methodParameters, handlerCount);

                //TODO: fully qualify type names
                
                handlerCount++;
            }

            builder.Append(@"
    }
}
");

            context.AddSource("CommandHandlerGeneratorExtensions_Generated.g.cs", builder.ToString());
        }

        private static void GenerateHandlerClass(
            StringBuilder builder,
            DelegateInvocation invocation,
            (string Type, string Name)[] methodParameters,
            int handlerCount)
        {
            builder.Append($@"
        private class GeneratedHandler_{handlerCount} : {CliActionType}
        {{
            public GeneratedHandler_{handlerCount}(
                {invocation.DelegateType.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat)} method");

            if (methodParameters.Length > 0)
            {
                builder.Append(",");
                builder.Append(string.Join($", ", methodParameters.Select(x => $@"
                {x.Type} {x.Name}")) + ")");
            }
            else
            {
                builder.Append(")");
            }

            builder.Append($@"
            {{
                Method = method;");
            foreach (var propertyAssignment in invocation.Parameters
                                                         .Select(x => x.GetPropertyAssignment())
                                                         .Where(x => !string.IsNullOrWhiteSpace(x)))
            {
                builder.Append($@"
                {propertyAssignment}");
            }

            builder.AppendLine($@"
            }}
                
            public {invocation.DelegateType.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat)} Method {{ get; }}");

            foreach (var propertyDeclaration in invocation.Parameters
                                                          .Select(x => x.GetPropertyDeclaration())
                                                          .Where(x => !string.IsNullOrWhiteSpace(x)))
            {
                builder.Append($@"
            {propertyDeclaration}");
            }

            builder.Append($@"
            public override int Invoke(global::System.CommandLine.ParseResult context) => InvokeAsync(context, global::System.Threading.CancellationToken.None).GetAwaiter().GetResult();");

            builder.Append($@"
            public override async global::System.Threading.Tasks.Task<int> InvokeAsync(global::System.CommandLine.ParseResult context, global::System.Threading.CancellationToken cancellationToken)
            {{");
            builder.Append($@"
                {invocation.InvokeContents()}");
            builder.Append($@"
            }}
        }}");
        }

        private static (string Type, string Name)[] GetMethodParameters(DelegateInvocation invocation)
        {
            return invocation.Parameters
                    .Select(x => x.GetMethodParameter())
                    .Where(x => !string.IsNullOrWhiteSpace(x.Name))
                    .ToArray();
        }

        private static void GenerateSetHandler(
            StringBuilder builder, 
            DelegateInvocation invocation,
            (string Type, string Name)[] methodParameters,
            int handlerCount,
            bool isGeneric)
        {
            builder.Append(
                @$"
        public static void SetHandler");

            if (isGeneric)
            {
                builder.Append($"<{string.Join(", ", Enumerable.Range(1, invocation.NumberOfGenerericParameters).Select(x => $@"T{x}"))}>");
            }
            builder.Append(@$"(
            this global::System.CommandLine.CliCommand command,");

            builder.Append($@"
            {invocation.DelegateType.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat)} method");

            if (methodParameters.Length > 0)
            {
                builder.Append(",");
                builder.AppendLine(string.Join(", ", methodParameters.Select(x => $@"
            {x.Type} {x.Name}")) + ")");
            }
            else
            {
                builder.Append(")");
            }

            builder.Append(@"
        {");
            builder.Append($@"
            command.Action = new GeneratedHandler_{handlerCount}(method");

            if (methodParameters.Length > 0)
            {
                builder.Append(", ");
                builder.Append(string.Join(", ", methodParameters.Select(x => x.Name)));
            }

            builder.Append(");");

            builder.AppendLine(@"
        }");
        }

        public void Initialize(GeneratorInitializationContext context)
        {
            //System.Diagnostics.Debugger.Launch();

            context.RegisterForSyntaxNotifications(() => new SyntaxReceiver());
        }
    }
}