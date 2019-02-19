using System.Collections.Generic;
using System.CommandLine.Binding;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace System.CommandLine.Invocation
{
    internal class ModelBindingCommandHandler : ICommandHandler
    {
        private readonly Delegate _handlerDelegate;
        private readonly ModelBinder _invocationTargetBinder;
        private readonly MethodInfo _handlerMethodInfo;
        private readonly IReadOnlyCollection<ModelBinder> _parameterBinders;

        public ModelBindingCommandHandler(
            MethodInfo handlerMethodInfo,
            IReadOnlyCollection<ModelBinder> parameterBinders,
            ModelBinder invocationTargetBinder = null)
        {
            _invocationTargetBinder = invocationTargetBinder;
            _handlerMethodInfo = handlerMethodInfo;
            _parameterBinders = parameterBinders;
        }

        public ModelBindingCommandHandler(
            Delegate handlerDelegate,
            IReadOnlyCollection<ModelBinder> parameterBinders)
        {
            _handlerDelegate = handlerDelegate;
            _parameterBinders = parameterBinders;
        }

        public async Task<int> InvokeAsync(InvocationContext context)
        {
            var bindingContext = context.BindingContext;

            var invocationTarget =
                _invocationTargetBinder?.CreateInstance(bindingContext);

            var invocationArguments =
                _parameterBinders.Select(p => p.CreateInstance(bindingContext))
                                 .ToArray();

            var result =
                _handlerDelegate == null
                    ? _handlerMethodInfo.Invoke(
                        invocationTarget,
                        invocationArguments)
                    : _handlerDelegate.DynamicInvoke(invocationArguments);

            return await CommandHandler.GetResultCodeAsync(result, context);
        }
    }
}
