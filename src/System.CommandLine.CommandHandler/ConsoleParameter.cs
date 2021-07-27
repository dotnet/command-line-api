using Microsoft.CodeAnalysis;

namespace System.CommandLine.CommandHandler
{
    public class ConsoleParameter : Parameter
    {
        public ConsoleParameter(ITypeSymbol consoleType)
            : base(consoleType)
        {
        }

        public override string GetValueFromContext()
            => "context.Console";
    }
}
