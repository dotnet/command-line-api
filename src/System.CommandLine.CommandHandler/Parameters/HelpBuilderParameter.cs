using Microsoft.CodeAnalysis;

namespace System.CommandLine.CommandHandler.Parameters
{
    public class HelpBuilderParameter : Parameter
    {
        public HelpBuilderParameter(ITypeSymbol helpBuilderType)
            : base(helpBuilderType)
        {
        }

        public override string GetValueFromContext()
            => "context.HelpBuilder";
    }
}
