using Microsoft.CodeAnalysis;

namespace System.CommandLine.Generator.Parameters
{
    internal abstract class PropertyParameter : Parameter, IEquatable<PropertyParameter>
    {
        protected PropertyParameter(string localName, ITypeSymbol type, ITypeSymbol valueType)
            : base(valueType)
        {
            LocalName = localName;
            Type = type;
        }

        public ITypeSymbol Type { get; }

        public string LocalName { get; }

        public string ParameterName => LocalName.ToLowerInvariant();

        public override string GetPropertyDeclaration()
            => $"private {Type.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat)} {LocalName} {{ get; }}";

        public override string GetPropertyAssignment()
            => $"{LocalName} = {ParameterName};";

        public override (string Type, string Name) GetMethodParameter()
            => (Type.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat), ParameterName);

        public override int GetHashCode()
        {
            return base.GetHashCode() * -1521134295 +
                SymbolComparer.GetHashCode(Type) * -1521134295 +
                HashCode(LocalName);
        }

        public override bool Equals(object? obj)
            => Equals(obj as PropertyParameter);

        public bool Equals(PropertyParameter? other)
        {
            return base.Equals(other) &&
                SymbolComparer.Equals(Type, other.Type) &&
                Equals(LocalName, other.LocalName);
        }
    }
}
