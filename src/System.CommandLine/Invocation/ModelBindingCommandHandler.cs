// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Generic;
using System.CommandLine.Binding;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace System.CommandLine.Invocation
{
    public class ModelBindingCommandHandler : ICommandHandler
    {
        private readonly Delegate? _handlerDelegate;
        private readonly object? _invocationTarget;
        private readonly ModelBinder? _invocationTargetBinder;
        private readonly MethodInfo? _handlerMethodInfo;
        private readonly IMethodDescriptor _methodDescriptor;
        private Dictionary<IValueDescriptor, IValueSource> invokeArgumentBindingSources { get; } =
            new Dictionary<IValueDescriptor, IValueSource>();
        private readonly bool _enforceExplicitBinding = false; // We have not exposed this because we anticipate changing how explicit/implicit binding is defined.

        public ModelBindingCommandHandler(
            MethodInfo handlerMethodInfo,
            IMethodDescriptor methodDescriptor)
        {
            _handlerMethodInfo = handlerMethodInfo ?? throw new ArgumentNullException(nameof(handlerMethodInfo));
            _invocationTargetBinder = _handlerMethodInfo.IsStatic
                                          ? null
                                          : new ModelBinder(_handlerMethodInfo.ReflectedType);
            _methodDescriptor = methodDescriptor ?? throw new ArgumentNullException(nameof(methodDescriptor));
        }

        public ModelBindingCommandHandler(
            MethodInfo handlerMethodInfo,
            IMethodDescriptor methodDescriptor,
            object? invocationTarget)
            :this(handlerMethodInfo, methodDescriptor )
        {
            _invocationTarget = invocationTarget;
        }

        public ModelBindingCommandHandler(
             Delegate handlerDelegate,
             IMethodDescriptor methodDescriptor)
        {
            _handlerDelegate = handlerDelegate ?? throw new ArgumentNullException(nameof(handlerDelegate));
            _methodDescriptor = methodDescriptor ?? throw new ArgumentNullException(nameof(methodDescriptor));
        }

        public async Task<int> InvokeAsync(InvocationContext context)
        {
            var bindingContext = context.BindingContext;

            var (boundValues, _) = ModelBinder.GetBoundValues(
                                                invokeArgumentBindingSources,
                                                bindingContext,
                                                _methodDescriptor.ParameterDescriptors,
                                                _enforceExplicitBinding);

            var invocationArguments = boundValues
                                        .Select(x => x.Value)
                                        .ToArray();

            object result;
            if (_handlerDelegate is null)
            {
                var invocationTarget = _invocationTarget ?? 
                    bindingContext.ServiceProvider.GetService(_handlerMethodInfo!.DeclaringType);
                if(invocationTarget is { })
                {
                    _invocationTargetBinder?.UpdateInstance(invocationTarget, bindingContext);
                }

                invocationTarget ??= _invocationTargetBinder?.CreateInstance(bindingContext);
                result = _handlerMethodInfo!.Invoke(invocationTarget, invocationArguments);
            }
            else
            {
                result = _handlerDelegate.DynamicInvoke(invocationArguments);
            }

            return await CommandHandler.GetExitCodeAsync(result, context);
        }

        public void BindParameter(ParameterInfo param, Argument argument)
        {
            var _ = argument ?? throw new InvalidOperationException("You must specify an argument to bind");
            BindValueSource(param, new SpecificSymbolValueSource(argument));
        }

        public void BindParameter(ParameterInfo param, Option option)
        {
            var _ = option ?? throw new InvalidOperationException("You must specify an option to bind");
            BindValueSource(param, new SpecificSymbolValueSource(option));
        }

        private void BindValueSource(ParameterInfo param, IValueSource valueSource)
        {
            var paramDesc = FindParameterDescriptor(param);
            if (paramDesc is null)
            {
                throw new InvalidOperationException("You must bind to a parameter on this handler");
            }
            invokeArgumentBindingSources.Add(paramDesc, valueSource);
        }

        private ParameterDescriptor? FindParameterDescriptor(ParameterInfo? param)
            => param == null
               ? null
               : _methodDescriptor.ParameterDescriptors
                    .FirstOrDefault(x => x.ValueName == param.Name &&
                                            x.ValueType == param.ParameterType);
    }
}