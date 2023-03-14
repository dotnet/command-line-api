using System.CommandLine.Binding;
using System.CommandLine.Invocation;
using System.Threading;
using System.Threading.Tasks;

namespace System.CommandLine.NamingConventionBinder
{
    public abstract class BindingHandler : ICommandHandler
    {
        private BindingContext? _bindingContext;

        /// <summary>
        /// The binding context for the current invocation.
        /// </summary>
        public BindingContext GetBindingContext(InvocationContext invocationContext) => _bindingContext ??= new BindingContext(invocationContext);

        public abstract int Invoke(InvocationContext context);

        public abstract Task<int> InvokeAsync(InvocationContext context, CancellationToken cancellationToken = default);
    }
}
