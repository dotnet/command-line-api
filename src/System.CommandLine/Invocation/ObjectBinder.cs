using System;
using System.Collections.Generic;
using System.CommandLine.Binding;
using System.CommandLine.Collections;
using System.CommandLine.Invocation;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace System.CommandLine.Invocation
{
    /// <summary>
    /// Implements an ICommandHandler for mapping parse results to a TModel
    /// object by specifying the alias-to-property bindings explicitly
    /// </summary>
    /// <typeparam name="TModel"></typeparam>
    public class ObjectBinder<TModel> : ICommandHandler
        where TModel : class
    {
        private readonly TModel _target;

        public ObjectBinder( TModel target  )
        {
            _target = target ?? throw new NullReferenceException( nameof(target) );
        }

        public Task<int> InvokeAsync( InvocationContext context )
        {
            if( context == null )
                return Task.FromResult( 1 );

            var cancelToken = context.GetCancellationToken();
            if( cancelToken.IsCancellationRequested )
                return (Task<int>) Task.FromCanceled( cancelToken );

            // Trap possible binding failures. These usually occur because a provided
            // alias doesn't match the aliases defined among the options and arguments
            // of the active Command
            try
            {
                // Only provide the Options and Arguments of the active command as potential
                // sources for bindings. Child Options and Arguments (i.e., from subcommands)
                // should be bound to child objects.

                // There might be a clever way of allowing the bindings for child objects to be
                // defined when the parent object bindings are defined but I wasn't able to
                // think of one.
                var modelBinder = context.Parser.Configuration.ModelBinderFactory(
                    context.ParseResult.CommandResult.Command.Options.ToList(),
                    context.ParseResult.CommandResult.Command.Arguments.ToList()
                );

                modelBinder.UpdateInstance( _target, context.BindingContext );
            }
            catch( UnknownAliasException e )
            {
                context.InvocationResult = new ObjectBinderErrorResult( e.Alias, e.ForOption );

                return Task.FromResult(1);
            }

            return Task.FromResult(0);
        }
    }
}
