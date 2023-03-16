using System.CommandLine.Binding;
using System.CommandLine.Invocation;

namespace System.CommandLine.NamingConventionBinder
{
    public abstract class BindingHandler : CliAction
    {
        private BindingContext? _bindingContext;

        /// <summary>
        /// The binding context for the current invocation.
        /// </summary>
        public virtual BindingContext GetBindingContext(InvocationContext invocationContext)
            => _bindingContext ??= new BindingContext(invocationContext);
    }
}
