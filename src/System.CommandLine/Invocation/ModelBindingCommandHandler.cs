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
        private readonly IMethodDescriptor _methodDescriptor;
        private Dictionary<IValueDescriptor, IValueSource> _invokeArgumentBindingSources { get; } =
            new Dictionary<IValueDescriptor, IValueSource>();
        private bool EnforceExplicitBinding = false; // Wrong formatting as hint to figure out how to set this

        public ModelBindingCommandHandler(
            MethodInfo handlerMethodInfo,
            IMethodDescriptor methodDescriptor)
        {
            _handlerMethodInfo = handlerMethodInfo ?? throw new ArgumentNullException(nameof(handlerMethodInfo));
            _invocationTargetBinder = _handlerMethodInfo.IsStatic
                                          ? null
                                          : new ModelBinder(_handlerMethodInfo.DeclaringType);
            _methodDescriptor = methodDescriptor ?? throw new ArgumentNullException(nameof(methodDescriptor));
            _parameterDescriptors = methodDescriptor.ParameterDescriptors ;
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
                                                _invokeArgumentBindingSources,
                                                bindingContext,
                                                _methodDescriptor.ParameterDescriptors,
                                                _methodDescriptor.Parent?.ModelType ?? typeof(object),
                                                EnforceExplicitBinding,
                                                true);
            var invocationArguments = boundValues
                                        .Select(x => x.Value)
                                        .ToArray();

            //var invocationArguments = new object?[_parameterDescriptors.Count()];
            //var length = _parameterDescriptors.Count();

            //for (int i = 0; i < length; i++)
            //{
            //    var paramDesc = _parameterDescriptors[i];
            //    var binder = bindingContext.GetModelBinder(paramDesc);
            //    IValueSource? valueSource;
            //    if (!_invokeArgumentBindingSources.TryGetValue(paramDesc, out valueSource))
            //    {
            //        valueSource = binder.GetValueSource(_invokeArgumentBindingSources, bindingContext, paramDesc);
            //    }
            //    var (boundValue, _) = ModelBinder.GetBoundValue(valueSource, bindingContext, paramDesc, true, binder.ModelDescriptor);
            //    if (!(boundValue is null))
            //    {
            //        invocationArguments[i] = boundValue.Value;
            //        continue;
            //    }

            //    invocationArguments[i] = binder.CreateInstance(bindingContext);
            //}

            object result;
            if (_handlerDelegate is null)
            {
                var invocationTarget = _invocationTarget ??
                                       _invocationTargetBinder?.CreateInstance(bindingContext);
                result = _handlerMethodInfo!.Invoke(invocationTarget, invocationArguments);
            }
            else
            {
                result = _handlerDelegate.DynamicInvoke(invocationArguments);
            }

            return await CommandHandler.GetResultCodeAsync(result, context);
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