// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

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
        private readonly object _invocationTarget;
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
            MethodInfo handlerMethodInfo,
            IReadOnlyCollection<ModelBinder> parameterBinders,
            object invocationTarget)
        {
            _invocationTarget = invocationTarget;
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

            var invocationArguments =
                _parameterBinders.Select(p => p.CreateInstance(bindingContext))
                    .ToArray();

            var invocationTarget = _invocationTarget ??
                _invocationTargetBinder?.CreateInstance(bindingContext);

            object result;
            if (_handlerDelegate == null)
            {
                result = _handlerMethodInfo.Invoke(
                    invocationTarget,
                    invocationArguments);
            }
            else
            {
                result = _handlerDelegate.DynamicInvoke(invocationArguments);
            }

            var resultCode = await CommandHandler.GetResultCodeAsync(result, context);

            context.SetInvokeResult(result);
            
            return resultCode;
        }
    }
}
