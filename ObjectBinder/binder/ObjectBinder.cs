using System;
using System.Collections.Generic;
using System.CommandLine;
using System.CommandLine.Binding;
using System.CommandLine.Invocation;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace J4JSoftware.CommandLine
{
    public class ObjectBinder<TModel> : IObjectBinder<TModel>
        where TModel : IRootObjectModel
    {
        public ObjectBinder(
            TModel target,
            string cmdName,
            IRootObjectModel parentModel = null,
            string cmdDescription = null
        )
        {
            if( string.IsNullOrEmpty( cmdName ) && parentModel != null )
                throw new ArgumentException(
                    $"If you specify an {nameof(IObjectModel)} you must also specify a {nameof(cmdName)}" );

            if( target == null )
                throw new NullReferenceException(nameof(target));

            Command = parentModel == null ? new RootCommand( cmdDescription ) : new Command( cmdName, cmdDescription );
            Target = target;

            ModelBinder = new ModelBinder<TModel>();
        }

        public Command Command { get; }
        public bool IsRootBinder => Command is RootCommand;
        public ModelBinder<TModel> ModelBinder { get; }
        public TModel Target { get; }

        public virtual Task<int> Bind(InvocationContext context)
        {
            if (context == null)
                return Task.FromResult(1);

            var cancelToken = context.GetCancellationToken();
            if (cancelToken.IsCancellationRequested)
                return Task.FromCanceled<int>(cancelToken);

            ModelBinder.UpdateInstance(Target, context.BindingContext);

            return Task.FromResult(0);
        }
    }
}
