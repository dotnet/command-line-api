using Microsoft.CodeAnalysis;

namespace System.CommandLine.Generator.Parameters
{
    internal class BindingContextParameter : Parameter, IEquatable<BindingContextParameter>
    {
        public BindingContextParameter(ITypeSymbol bindingContextType)
            : base(bindingContextType)
        {
        }

        public override string GetValueFromContext()
            => "context.GetBindingContext()";

        public override int GetHashCode()
            => base.GetHashCode();

        public override bool Equals(object? obj)
            => Equals(obj as BindingContextParameter);

        public bool Equals(BindingContextParameter? other)
        {
            if (other is null) return false;
            return base.Equals(other);
        }
    }
}
