using Microsoft.CodeAnalysis;

namespace System.CommandLine.Generator.Parameters
{
    internal class RawParameter : PropertyParameter, IEquatable<RawParameter>
    {
        public RawParameter(string localName, ITypeSymbol valueType) 
            : base(localName, valueType, valueType)
        {
        }

        public override string GetValueFromContext()
            => LocalName;

        public override int GetHashCode()
            => base.GetHashCode();

        public override bool Equals(object? obj)
            => Equals(obj as RawParameter);

        public bool Equals(RawParameter? other)
        {
            if (other is null) return false;
            return base.Equals(other);
        }
    }
}
