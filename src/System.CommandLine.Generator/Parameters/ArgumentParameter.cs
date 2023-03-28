using Microsoft.CodeAnalysis;

namespace System.CommandLine.Generator.Parameters
{
    internal class ArgumentParameter : PropertyParameter, IEquatable<ArgumentParameter>
    {
        public ArgumentParameter(string localName, INamedTypeSymbol type, ITypeSymbol valueType)
            : base(localName, type, valueType)
        {
        }

        public override string GetValueFromContext()
            => $"context.GetValue({LocalName})";

        public override int GetHashCode() 
            => base.GetHashCode();

        public override bool Equals(object? obj)
            => Equals(obj as ArgumentParameter);

        public bool Equals(ArgumentParameter? other)
        {
            if (other is null) return false;
            return base.Equals(other);
        }
    }
}
