using System.CommandLine.Hosting;
using System.Threading.Tasks;

using Microsoft.Extensions.DependencyInjection;

namespace System.CommandLine.Invocation
{
    /// <summary>
    /// Proviveds helper methods to initialize a command handler that uses
    /// Dependency Injection from the .NET Generic Host to materialize
    /// the handler.
    /// </summary>
    /// <seealso cref="CommandHandler"/>
    public static class HostedCommandHandler
    {
        private class HostedCommandHandlerWrapper<THostedCommandHandler> : ICommandHandler
            where THostedCommandHandler : ICommandHandler
        {
            public Task<int> InvokeAsync(InvocationContext context)
            {
                var host = context.GetHost();
                var handler = host.Services.GetRequiredService<THostedCommandHandler>();
                return handler.InvokeAsync(context);
            }
        }

        /// <summary>
        /// Creates an <see cref="ICommandHandler"/> instance that when invoked
        /// will forward the <see cref="InvocationContext"/> to an instance of
        /// <paramref name="commandHandlerType"/> obtained from the DI-container
        /// of the .NET Generic Host used in the invocation pipeline.
        /// </summary>
        /// <param name="commandHandlerType">A command handler service type implementing <see cref="ICommandHandler"/> that has been registered with the .NET Generic Host DI-container.</param>
        /// <returns>A wrapper object that implements the <see cref="ICommandHandler"/> interface by forwarding the call to <see cref="ICommandHandler.InvokeAsync(InvocationContext)"/> to the implementation of <typeparamref name="TCommandHandler"/>.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="commandHandlerType"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentException"><paramref name="commandHandlerType"/> does not implement the <see cref="ICommandHandler"/> interface type.</exception>
        public static ICommandHandler CreateFromHost(Type commandHandlerType)
        {
            _ = commandHandlerType ?? throw new ArgumentNullException(nameof(commandHandlerType));
            Type wrapperHandlerType;
            try
            {
                wrapperHandlerType = typeof(HostedCommandHandlerWrapper<>)
                    .MakeGenericType(commandHandlerType);
            }
            catch (ArgumentException argExcept)
            {
                throw new ArgumentException(
                    paramName: nameof(commandHandlerType),
                    message: $"{commandHandlerType} does not implement the {typeof(ICommandHandler)} interface.",
                    innerException: argExcept);
            }
            return (ICommandHandler)Activator.CreateInstance(wrapperHandlerType);
        }

        /// <summary>
        /// Creates an <see cref="ICommandHandler"/> instance that when invoked
        /// will forward the <see cref="InvocationContext"/> to an instance of
        /// <typeparamref name="TCommandHandler"/> obtained from the DI-container
        /// of the .NET Generic Host used in the invocation pipeline.
        /// </summary>
        /// <typeparam name="TCommandHandler">A command handler service type that has been registered with the .NET Generic Host DI-container.</typeparam>
        /// <returns>A wrapper object that implements the <see cref="ICommandHandler"/> interface by forwarding the call to <see cref="ICommandHandler.InvokeAsync(InvocationContext)"/> to the implementation of <typeparamref name="TCommandHandler"/>.</returns>
        public static ICommandHandler CreateFromHost<TCommandHandler>()
            where TCommandHandler : ICommandHandler =>
            new HostedCommandHandlerWrapper<TCommandHandler>();
    }
}