using Microsoft.CodeAnalysis;
using System.CommandLine.Generator.Parameters;
using System.Linq;
using System.Text;

namespace System.CommandLine.Generator.Invocations
{
    internal class FactoryModelBindingInvocation : DelegateInvocation, IEquatable<FactoryModelBindingInvocation>
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
            builder.AppendLine($@"
                var model = {factoryParam.LocalName}.Invoke(context);");
            
            switch (ReturnPattern)
            {
                case ReturnPattern.FunctionReturnValue:
                case ReturnPattern.AwaitFunction:
                case ReturnPattern.AwaitFunctionReturnValue:
                    builder.Append(@"
                var rv = ");
                    break;
            }
            builder.Append(@"
                Method.Invoke(model");
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
                    builder.Append(@"
                return await Task.FromResult(context.ExitCode);");
                    break;
                case ReturnPattern.FunctionReturnValue:
                    builder.Append(@"
                return await Task.FromResult(rv);");
                    break;
                case ReturnPattern.AwaitFunction:
                    builder.Append(@"
                await rv;");
                    builder.Append(@"
                return context.ExitCode;");
                    break;
                case ReturnPattern.AwaitFunctionReturnValue:
                    builder.Append(@"
                return await rv;");
                    break;
            }
            return builder.ToString();
        }

        public override int GetHashCode()
            => base.GetHashCode();

        public override bool Equals(object? obj)
            => Equals(obj as FactoryModelBindingInvocation);

        public bool Equals(FactoryModelBindingInvocation? other)
        {
            if (other is null) return false;
            return base.Equals(other);
        }
    }
}
