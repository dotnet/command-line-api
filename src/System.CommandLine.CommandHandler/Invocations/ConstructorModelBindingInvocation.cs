using Microsoft.CodeAnalysis;
using System.Linq;
using System.Text;

namespace System.CommandLine.CommandHandler.Invocations
{
    public class ConstructorModelBindingInvocation : DelegateInvocation
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
            builder.Append("Method.Invoke(model");
            var remainigParameters = Parameters.Skip(Constructor.Parameters.Length).ToList();
            if (remainigParameters.Count > 0)
            {
                builder.Append(", ");
                builder.Append(string.Join(", ", remainigParameters.Select(x => x.GetValueFromContext())));
            }
            builder.AppendLine(");");
            builder.AppendLine("return Task.FromResult(context.ExitCode);");
            return builder.ToString();
        }
    }
}
