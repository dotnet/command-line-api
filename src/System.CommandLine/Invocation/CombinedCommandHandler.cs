using System.CommandLine.Parsing;
using System.Threading;
using System.Threading.Tasks;

namespace System.CommandLine.Invocation
{
    internal sealed class ChainedCommandHandler : ICommandHandler
    {
        private readonly SymbolResultTree _symbols;
        private readonly ICommandHandler? _commandHandler;

        internal ChainedCommandHandler(SymbolResultTree symbols, ICommandHandler? commandHandler)
        {
            _symbols = symbols;
            _commandHandler = commandHandler;
        }

        public int Invoke(InvocationContext context)
        {
            // We want to build a stack of (action, next) pairs. But we are not using any collection or LINQ.
            // Each handler is closure (a lambda with state), where state is the "next" handler.
            Action<InvocationContext, ICommandHandler?>? chainedHandler = _commandHandler is not null
                ? (ctx, next) => _commandHandler.Invoke(ctx)
                : null;
            ICommandHandler? chainedHandlerArgument = null;

            foreach (var pair in _symbols)
            {
                if (pair.Key is Directive directive && directive.HasHandler)
                {
                    var syncHandler = directive.SyncHandler 
                        ?? throw new NotSupportedException($"Directive {directive.Name} does not provide a synchronous handler.");

                    if (chainedHandler is not null)
                    {
                        // capture the state in explicit way, to hint the compiler that the current state needs to be used
                        var capturedHandler = chainedHandler;
                        var capturedArgument = chainedHandlerArgument;

                        chainedHandlerArgument = new AnonymousCommandHandler(ctx => capturedHandler.Invoke(ctx, capturedArgument));
                    }
                    chainedHandler = syncHandler;
                }
            }

            chainedHandler!.Invoke(context, chainedHandlerArgument);

            return context.ExitCode;
        }

        public async Task<int> InvokeAsync(InvocationContext context, CancellationToken cancellationToken = default)
        {
            Func<InvocationContext, ICommandHandler?, CancellationToken, Task>? chainedHandler = _commandHandler is not null
                ? (ctx, next, ct) => _commandHandler.InvokeAsync(ctx, ct)
                : null;
            ICommandHandler? chainedHandlerArgument = null;

            foreach (var pair in _symbols)
            {
                if (pair.Key is Directive directive && directive.HasHandler)
                {
                    var asyncHandler = directive.AsyncHandler
                        ?? throw new NotSupportedException($"Directive {directive.Name} does not provide an asynchronous handler.");

                    if (chainedHandler is not null)
                    {
                        var capturedHandler = chainedHandler;
                        var capturedArgument = chainedHandlerArgument;

                        chainedHandlerArgument = new AnonymousCommandHandler((ctx, ct) => capturedHandler.Invoke(ctx, capturedArgument, ct));
                    }
                    chainedHandler = asyncHandler;
                }
            }

            await chainedHandler!.Invoke(context, chainedHandlerArgument, cancellationToken);

            return context.ExitCode;
        }
    }
}
