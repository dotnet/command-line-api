using Microsoft.CodeAnalysis;
using System.Linq;
using System.Text;

namespace System.CommandLine.CommandHandler
{
    public class FactoryModelBindingInvocation : DelegateInvocation
    {
        public FactoryModelBindingInvocation(ITypeSymbol delegateType) 
            : base(delegateType, 2)
        {
        }

        public override string InvokeContents()
        {
            StringBuilder builder = new();

            var factoryParam = (FactoryParameter)Parameters[0];
            builder.AppendLine($"var model = {factoryParam.LocalName}.Invoke(context);");
            builder.Append("Method.Invoke(model");
            var remainigParameters = Parameters.Skip(1).ToList();
            if (remainigParameters.Count > 0)
            {
                builder.Append(", ");
                builder.Append(string.Join(", ", remainigParameters.Select(x => x.GetValueFromContext())));
            }
            builder.AppendLine(");");
            builder.AppendLine("return Task.FromResult(0);");
            return builder.ToString();
        }
    }
}
