using Microsoft.CodeAnalysis;

namespace System.CommandLine.CommandHandler.Parameters
{
    public class InvocationContextParameter : Parameter
    {
        public InvocationContextParameter(ITypeSymbol invocationContextType)
            : base(invocationContextType)
        {
        }

        public override string GetValueFromContext()
            => "context";
    }
}
