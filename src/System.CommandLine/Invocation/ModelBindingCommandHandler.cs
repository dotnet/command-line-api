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
        private Dictionary<IValueDescriptor, IValueSource> _invokeArgumentBindingSources { get; } =
            new Dictionary<IValueDescriptor, IValueSource>();

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
            var bindingContext = context.BindingContext;

            var invocationArguments = new object?[_parameterDescriptors.Count()];
            var length = _parameterDescriptors.Count();

            for (int i = 0; i < length; i++)
            {
                var paramDesc = _parameterDescriptors[i];
                if (_invokeArgumentBindingSources.TryGetValue(paramDesc, out var valueSource))
                {
                    invocationArguments[i] = ValueFromValueSource(paramDesc, valueSource, bindingContext);
                }
                else
                {
                    var binder = bindingContext.GetModelBinder(paramDesc);
                    invocationArguments[i] = binder.CreateInstance(bindingContext);
                }
            }

            var invocationTarget = _invocationTarget ??
                                   _invocationTargetBinder?.CreateInstance(bindingContext);

            object result;
            if (_handlerDelegate is null)
            {
                result = _handlerMethodInfo!.Invoke(
                    invocationTarget,
                    invocationArguments);
            }
            else
            {
                result = _handlerDelegate.DynamicInvoke(invocationArguments);
            }

            return await CommandHandler.GetResultCodeAsync(result, context);
        }

        private object? ValueFromValueSource(ParameterDescriptor paramDesc, IValueSource valueSource, BindingContext bindingContext)
        {
            BoundValue? boundValue;
            if (valueSource is null)
            {
                // If there is no source to bind from, no value can be bound.
                return null;
            }
            if (bindingContext.TryBindToScalarValue(
                 paramDesc,
                 valueSource,
                 out boundValue))
            { 
                // boundValue has been set
            }
            else if ( paramDesc.HasDefaultValue)
            {
                boundValue = BoundValue.DefaultForValueDescriptor(paramDesc);
            }
            if (!(boundValue is null))
            {
                return boundValue.Value;
            }
            var parameterBinder = bindingContext.GetModelBinder(paramDesc);
            return parameterBinder.CreateInstance(bindingContext);
        }

        public void BindParameter(ParameterInfo param, Argument argument)
        {
            var _ = argument ?? throw new InvalidOperationException("You must specify an argument to bind");
            BindValueSource(param, new SpecificSymbolValueSource(argument));
        }

        public void BindParameter(ParameterInfo param, Option option)
        {
            var _ = option ?? throw new InvalidOperationException("You must specify an argument to bind");
            BindValueSource(param, new SpecificSymbolValueSource(option));
        }

        private void BindValueSource(ParameterInfo param, IValueSource valueSource)
        {
            var paramDesc = FindParameterDescriptor(param);
            if (paramDesc is null)
            {
                throw new InvalidOperationException("You must bind to a parameter on this handler");
            }
            _invokeArgumentBindingSources.Add(paramDesc, valueSource);
        }

        private ParameterDescriptor? FindParameterDescriptor(ParameterInfo? param)
            => param == null
               ? null
               : _parameterDescriptors
                    .FirstOrDefault(x => x.ValueName == param.Name &&
                                            x.ValueType == param.ParameterType);
    }
}