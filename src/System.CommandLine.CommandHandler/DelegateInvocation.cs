using Microsoft.CodeAnalysis;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace System.CommandLine.CommandHandler
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
            //NB: Should invoke and return Task<int>
            /*
             * Method.Invoke(value1, context.Console, value2);
        
            return Task.FromResult(0);
             */
            builder.Append("Method.Invoke(");
            builder.Append(string.Join(", ", Parameters.Select(x => x.GetValueFromContext())));
            builder.AppendLine(");");
            builder.AppendLine("return Task.FromResult(0);");
            return builder.ToString();
            //return $@"Method.Invoke(value1, context.Console, value2);";


        }
    }
}
