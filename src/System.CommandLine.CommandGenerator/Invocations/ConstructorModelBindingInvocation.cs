﻿using Microsoft.CodeAnalysis;
using System.Linq;
using System.Text;

namespace System.CommandLine.CommandGenerator.Invocations
{
    internal class ConstructorModelBindingInvocation : DelegateInvocation, IEquatable<ConstructorModelBindingInvocation>
    {
        public ConstructorModelBindingInvocation(
            IMethodSymbol constructor, 
            ReturnPattern returnPattern,
            ITypeSymbol delegateType)
            : base(delegateType, returnPattern, 1)
        {
            Constructor = constructor;
        }

        public IMethodSymbol Constructor { get; }

        public override string InvokeContents()
        {
            StringBuilder builder = new();
            builder.Append($"var model = new {Constructor.ContainingType.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat)}(");
            builder.Append(string.Join(", ", Parameters.Take(Constructor.Parameters.Length)
                .Select(x => x.GetValueFromContext())));
            builder.AppendLine(");");

            switch (ReturnPattern)
            {
                case ReturnPattern.FunctionReturnValue:
                case ReturnPattern.AwaitFunction:
                case ReturnPattern.AwaitFunctionReturnValue:
                    builder.Append("var rv = ");
                    break;
            }
            builder.Append("Method.Invoke(model");
            var remainigParameters = Parameters.Skip(Constructor.Parameters.Length).ToList();
            if (remainigParameters.Count > 0)
            {
                builder.Append(", ");
                builder.Append(string.Join(", ", remainigParameters.Select(x => x.GetValueFromContext())));
            }
            builder.AppendLine(");");
            switch (ReturnPattern)
            {
                case ReturnPattern.InvocationContextExitCode:
                    builder.AppendLine("return await Task.FromResult(context.ExitCode);");
                    break;
                case ReturnPattern.FunctionReturnValue:
                    builder.AppendLine("return await Task.FromResult(rv);");
                    break;
                case ReturnPattern.AwaitFunction:
                    builder.AppendLine("await rv;");
                    builder.AppendLine("return context.ExitCode;");
                    break;
                case ReturnPattern.AwaitFunctionReturnValue:
                    builder.AppendLine("return await rv;");
                    break;
            }
            return builder.ToString();
        }

        public override int GetHashCode()
        {
            return base.GetHashCode() * -1521134295 +
                SymbolComparer.GetHashCode(Constructor);
        }

        public override bool Equals(object? obj)
            => Equals(obj as ConstructorModelBindingInvocation);

        public bool Equals(ConstructorModelBindingInvocation? other)
        {
            if (other is null) return false;
            return base.Equals(other) &&
                SymbolComparer.Equals(Constructor, other.Constructor);
        }
    }
}
