using Microsoft.CodeAnalysis;

namespace System.CommandLine.CommandGenerator.Parameters
{
    internal class FactoryParameter : Parameter, IEquatable<FactoryParameter>
    {
        public ITypeSymbol FactoryType { get; }
        public string LocalName { get; }
        private string ParameterName => LocalName.ToLowerInvariant();

        public FactoryParameter(RawParameter rawParameter, ITypeSymbol factoryType)
            : base(rawParameter.ValueType)
        {
            LocalName = rawParameter.LocalName;
            FactoryType = factoryType;
        }

        public override string GetValueFromContext() => "";

        public override string GetPropertyDeclaration()
            => $"private {FactoryType.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat)} {LocalName} {{ get; }}";

        public override string GetPropertyAssignment()
            => $"{LocalName} = {ParameterName};";

        public override (string Type, string Name) GetMethodParameter()
            => (FactoryType.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat), ParameterName);

        public override int GetHashCode()
        {
            return base.GetHashCode() * -1521134295 +
                SymbolComparer.GetHashCode(FactoryType) * -1521134295 + 
                HashCode(LocalName);
        }

        public override bool Equals(object obj)
            => Equals(obj as FactoryParameter);

        public bool Equals(FactoryParameter? other)
        {
            return base.Equals(other) &&
                SymbolComparer.Equals(FactoryType, other.FactoryType) &&
                Equals(LocalName, other.LocalName);
        }
    }
}