using Microsoft.CodeAnalysis;
using System.Collections.Generic;
using System.CommandLine.Generator.Parameters;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;

namespace System.CommandLine.Generator.Invocations
{
    internal class DelegateInvocation : IEquatable<DelegateInvocation>
    {
        protected static SymbolEqualityComparer SymbolComparer { get; } = SymbolEqualityComparer.Default;

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
                    builder.Append(@"
                var rv = ");
                    break;
            }

            builder.Append(@"
                Method.Invoke(");
            builder.Append(string.Join(", ", Parameters.Select(x => x.GetValueFromContext())));
            builder.AppendLine(");");

            switch (ReturnPattern)
            {
                case ReturnPattern.InvocationContextExitCode:
                    builder.Append(@"
                return 0;");
                    break;
                case ReturnPattern.FunctionReturnValue:
                    builder.Append(@"
                return rv;");
                    break;
                case ReturnPattern.AwaitFunction:
                    builder.Append(@"
                await rv;");
                    builder.Append(@"
                return 0;");
                    break;
                case ReturnPattern.AwaitFunctionReturnValue:
                    builder.Append(@"
                return await rv;");
                    break;
            }
            return builder.ToString();
        }

        public override int GetHashCode()
        {
            int hashCode = SymbolComparer.GetHashCode(DelegateType) * -1521134295 +
                HashCode(ReturnPattern) * -1521134295 + 
                HashCode(NumberOfGenerericParameters) * -1521134295;

            foreach(Parameter parameter in Parameters)
            {
                hashCode += HashCode(parameter) * -1521134295;
            }

            return hashCode;
        }

        protected static int HashCode<T>([DisallowNull] T value)
                => EqualityComparer<T>.Default.GetHashCode(value);

        public override bool Equals(object? obj)
        {
            return Equals(obj as DelegateInvocation);
        }

        public bool Equals(DelegateInvocation? other)
        {
            if (other is null) return false;

            bool areEqual = SymbolComparer.Equals(DelegateType, other.DelegateType) &&
                Equals(ReturnPattern, other.ReturnPattern) &&
                Equals(NumberOfGenerericParameters, other.NumberOfGenerericParameters) &&
                Equals(Parameters.Count, other.Parameters.Count);
            for(int i = 0; areEqual && i < Parameters.Count; i++)
            {
                areEqual &= Equals(Parameters[i], other.Parameters[i]);
            }
            return areEqual;
        }

        protected static bool Equals<T>(T first, T second)
            => EqualityComparer<T>.Default.Equals(first, second);
    }
}
