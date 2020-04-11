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

            if( context.ModelBinder == null )
            {
                context.InvocationResult = new ObjectBinderErrorResult( $"{nameof(context.ModelBinder)} is undefined" );
                return Task.FromResult( 1 );
            }

            var cancelToken = context.GetCancellationToken();
            if( cancelToken.IsCancellationRequested )
                return (Task<int>) Task.FromCanceled( cancelToken );

            context.ModelBinder.UpdateInstance( _target, context.BindingContext );

            return Task.FromResult(0);
        }
    }
}
