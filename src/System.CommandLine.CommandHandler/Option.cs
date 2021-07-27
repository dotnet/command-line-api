using Microsoft.CodeAnalysis;

namespace System.CommandLine.CommandHandler
{
    public class Option : Parameter
    {
        public Option(string localName, INamedTypeSymbol type, ITypeSymbol valueType)
            : base(valueType)
        {
            LocalName = localName;
            Type = type;
        }

        public INamedTypeSymbol Type { get; }

        public string LocalName { get; }

        private string ParameterName => LocalName.ToLowerInvariant();

        public override string GetValueFromContext()
            => $"context.ParseResult.ValueForOption({LocalName})";

        public override string GetPropertyDeclaration()
            => $"private {Type.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat)} {LocalName} {{ get; }}";

        public override string GetPropertyAssignment()
            => $"{LocalName} = {ParameterName};";

        public override (string Type, string Name) GetMethodParameter()
            => (Type.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat), ParameterName);
    }
}
