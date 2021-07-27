using Microsoft.CodeAnalysis;

namespace System.CommandLine.CommandHandler.Parameters
{
    public class ArgumentParameter : PropertyParameter
    {
        public ArgumentParameter(string localName, INamedTypeSymbol type, ITypeSymbol valueType)
            : base(localName, type, valueType)
        {
        }

        public override string GetValueFromContext()
            => $"context.ParseResult.ValueForArgument({LocalName})";
    }
}
