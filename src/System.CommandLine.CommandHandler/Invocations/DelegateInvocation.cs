using Microsoft.CodeAnalysis;
using System.Collections.Generic;
using System.CommandLine.CommandHandler.Parameters;
using System.Linq;
using System.Text;

namespace System.CommandLine.CommandHandler.Invocations
{
    public class DelegateInvocation
    {
        public ITypeSymbol DelegateType { get; }
        public int NumberOfGenerericParameters { get; }

        public DelegateInvocation(ITypeSymbol delegateType, int numberOfGenerericParameters)
        {
            DelegateType = delegateType;
            NumberOfGenerericParameters = numberOfGenerericParameters;
        }

        public List<Parameter> Parameters { get; } = new();

        public virtual string InvokeContents()
        {
            StringBuilder builder = new();
            builder.Append("Method.Invoke(");
            builder.Append(string.Join(", ", Parameters.Select(x => x.GetValueFromContext())));
            builder.AppendLine(");");

            builder.AppendLine("return Task.FromResult(context.ExitCode);");
            return builder.ToString();
        }
    }
}
