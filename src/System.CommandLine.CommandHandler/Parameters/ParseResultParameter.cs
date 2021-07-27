using Microsoft.CodeAnalysis;

namespace System.CommandLine.CommandHandler.Parameters
{
    public class ParseResultParameter : Parameter
    {
        public ParseResultParameter(ITypeSymbol parseResultType)
            : base(parseResultType)
        {
        }

        public override string GetValueFromContext()
            => "context.ParseResult";
    }
}
