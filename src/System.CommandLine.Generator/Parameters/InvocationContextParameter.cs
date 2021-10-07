using Microsoft.CodeAnalysis;

namespace System.CommandLine.Generator.Parameters
{
    internal class InvocationContextParameter : Parameter, IEquatable<InvocationContextParameter>
    {
        public InvocationContextParameter(ITypeSymbol invocationContextType)
            : base(invocationContextType)
        {
        }

        public override string GetValueFromContext()
            => "context";

        public override int GetHashCode()
            => base.GetHashCode();

        public override bool Equals(object? obj)
            => Equals(obj as InvocationContextParameter);

        public bool Equals(InvocationContextParameter? other)
        {
            if (other is null) return false;
            return base.Equals(other);
        }
    }
}
