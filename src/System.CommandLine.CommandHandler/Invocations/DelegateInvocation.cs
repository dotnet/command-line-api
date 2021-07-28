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
        public ReturnPattern ReturnPattern { get; }
        public int NumberOfGenerericParameters { get; }

        public DelegateInvocation(
            ITypeSymbol delegateType,
            ReturnPattern returnPattern,
            int numberOfGenerericParameters)
        {
            DelegateType = delegateType;
            ReturnPattern = returnPattern;
            NumberOfGenerericParameters = numberOfGenerericParameters;
        }

        public List<Parameter> Parameters { get; } = new();

        public virtual string InvokeContents()
        {
            StringBuilder builder = new();
            
            switch (ReturnPattern)
            {
                case ReturnPattern.FunctionReturnValue:
                case ReturnPattern.AwaitFunction:
                case ReturnPattern.AwaitFunctionReturnValue:
                    builder.Append("var rv = ");
                    break;
            }

            builder.Append("Method.Invoke(");
            builder.Append(string.Join(", ", Parameters.Select(x => x.GetValueFromContext())));
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
    }
}
