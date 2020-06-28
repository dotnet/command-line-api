// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Generic;
using System.CommandLine.Binding;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace System.CommandLine.Invocation
{
    internal class ModelBindingCommandHandler : ICommandHandler
    {
        private readonly Delegate? _handlerDelegate;
        private readonly object? _invocationTarget;
        private readonly ModelBinder? _invocationTargetBinder;
        private readonly MethodInfo? _handlerMethodInfo;
        private readonly IReadOnlyList<ParameterDescriptor> _parameterDescriptors;

        public ModelBindingCommandHandler(
            MethodInfo handlerMethodInfo,
            IReadOnlyList<ParameterDescriptor> parameterDescriptors)
        {
            _handlerMethodInfo = handlerMethodInfo ?? throw new ArgumentNullException(nameof(handlerMethodInfo));
            _invocationTargetBinder = _handlerMethodInfo.IsStatic
                                          ? null
                                          : new ModelBinder(_handlerMethodInfo.DeclaringType);
            _parameterDescriptors = parameterDescriptors ?? throw new ArgumentNullException(nameof(parameterDescriptors));
        }

        public ModelBindingCommandHandler(
            MethodInfo handlerMethodInfo,
            IReadOnlyList<ParameterDescriptor> parameterDescriptors,
            object? invocationTarget)
        {
            _invocationTarget = invocationTarget;
            _handlerMethodInfo = handlerMethodInfo ?? throw new ArgumentNullException(nameof(handlerMethodInfo));
            _parameterDescriptors = parameterDescriptors ?? throw new ArgumentNullException(nameof(parameterDescriptors));
        }

        public ModelBindingCommandHandler(
            Delegate handlerDelegate,
            IReadOnlyList<ParameterDescriptor> parameterDescriptors)
        {
            _handlerDelegate = handlerDelegate ?? throw new ArgumentNullException(nameof(handlerDelegate));
            _parameterDescriptors = parameterDescriptors ?? throw new ArgumentNullException(nameof(parameterDescriptors));
        }

        public async Task<int> InvokeAsync(InvocationContext context)
        {
            object result;
            var bindingContext = context.BindingContext;
            var invocationTarget = _invocationTarget ??
                                   _invocationTargetBinder?.CreateInstance(bindingContext);
            var invocationArguments = new object?[] { };
            if (!(_handlerDelegate is null))
            {
                invocationArguments = ArgumentsFromDelegate(bindingContext, _parameterDescriptors);
                result = _handlerDelegate.DynamicInvoke(invocationArguments);
            }
            else if (!(_handlerMethodInfo is null))
            {
                var methodBinder = bindingContext.GetMethodBinder(_handlerMethodInfo);
                invocationArguments = methodBinder.GetInvocationArguments(bindingContext);
                result = _handlerMethodInfo!.Invoke(
                    invocationTarget,
                    invocationArguments);
            }
            else
            {
                throw new InvalidOperationException("Either a method info or a delegate must be supplied");
            }

            return await CommandHandler.GetResultCodeAsync(result, context);

            static object?[] ArgumentsFromDelegate(BindingContext bindingContext, IReadOnlyList<ParameterDescriptor> parameterDescriptors)
            {
                var parameterBinders = parameterDescriptors
                      .Select(p => bindingContext.GetModelBinder(p))
                      .ToList();

                return parameterBinders
                       .Select(binder => binder.CreateInstance(bindingContext))
                       .ToArray();
            }
        }
    }
}