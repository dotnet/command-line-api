using System.CommandLine.Binding;

namespace System.CommandLine.NamingConventionBinder
{
    public abstract class BindingHandler : CliAction
    {
        private BindingContext? _bindingContext;

        /// <summary>
        /// The binding context for the current invocation.
        /// </summary>
        public virtual BindingContext GetBindingContext(ParseResult parseResult)
            => _bindingContext ??= new BindingContext(parseResult);
    }
}
