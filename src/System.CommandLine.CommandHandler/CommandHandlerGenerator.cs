using Microsoft.CodeAnalysis;
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

                builder.AppendLine(@$"public static {ICommandHandlerType} Generate<{string.Join(", ", Enumerable.Range(1, invocation.NumberOfGenerericParameters).Select(x => $"Unused{x}"))}>(this CommandHandlerGenerator handler,");
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
}
