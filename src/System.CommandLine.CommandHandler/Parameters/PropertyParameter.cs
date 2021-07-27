using Microsoft.CodeAnalysis;

namespace System.CommandLine.CommandHandler.Parameters
{
    public abstract class PropertyParameter : Parameter
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
    }
}
