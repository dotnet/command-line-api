using Microsoft.CodeAnalysis;

namespace System.CommandLine.CommandHandler.Parameters
{
    public class BindingContextParameter : Parameter
    {
        public BindingContextParameter(ITypeSymbol bindingContextType)
            : base(bindingContextType)
        {
        }

        public override string GetValueFromContext()
            => "context.BindingContext";
    }
}
