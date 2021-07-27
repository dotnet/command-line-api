using Microsoft.CodeAnalysis;

namespace System.CommandLine.CommandHandler
{

    public class RawParameter : Parameter
    {
        public RawParameter(string localName, ITypeSymbol valueType) 
            : base(valueType)
        {
            LocalName = localName;
        }

        public string LocalName { get; }
        private string ParameterName => LocalName.ToLowerInvariant();

        public override string GetValueFromContext()
            => LocalName;

        public override string GetPropertyDeclaration()
            => $"private {ValueType.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat)} {LocalName} {{ get; }}";

        public override string GetPropertyAssignment()
            => $"{LocalName} = {ParameterName};";

        public override (string Type, string Name) GetMethodParameter()
            => (ValueType.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat), ParameterName);
    }
}
