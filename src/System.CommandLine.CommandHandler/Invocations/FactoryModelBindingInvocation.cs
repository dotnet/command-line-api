using Microsoft.CodeAnalysis;
using System.CommandLine.CommandHandler.Parameters;
using System.Linq;
using System.Text;

namespace System.CommandLine.CommandHandler.Invocations
{
    public class FactoryModelBindingInvocation : DelegateInvocation
    {
        public FactoryModelBindingInvocation(
            ITypeSymbol delegateType,
            ReturnPattern returnPattern)
            : base(delegateType, returnPattern, 2)
        { }

        public override string InvokeContents()
        {
            StringBuilder builder = new();

            var factoryParam = (FactoryParameter)Parameters[0];
            builder.AppendLine($"var model = {factoryParam.LocalName}.Invoke(context);");
            
            switch (ReturnPattern)
            {
                case ReturnPattern.FunctionReturnValue:
                case ReturnPattern.AwaitFunction:
                case ReturnPattern.AwaitFunctionReturnValue:
                    builder.Append("var rv = ");
                    break;
            }
            builder.Append("Method.Invoke(model");
            var remainigParameters = Parameters.Skip(1).ToList();
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
    }
}
