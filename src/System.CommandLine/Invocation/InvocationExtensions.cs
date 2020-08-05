using System.Reflection;

namespace System.CommandLine.Invocation
{
    public static class InvocationExtensions
    {
        public static void BindParameter(this ICommandHandler handler, ParameterInfo param, Option option)
        {
            // check for nulls
            if (!(handler is ModelBindingCommandHandler bindingHandler))
            {
                throw new InvalidOperationException("Cannot bind to this type of handler");
            }
            bindingHandler.BindParameter(param, option);
        }

        public static void BindParameter(this ICommandHandler handler, ParameterInfo param, Argument argument)
        {
            if (!(handler is ModelBindingCommandHandler bindingHandler))
            {
                throw new InvalidOperationException("Cannot bind to this type of handler");
            }
            bindingHandler.BindParameter(param, argument);
        }
    }
}
