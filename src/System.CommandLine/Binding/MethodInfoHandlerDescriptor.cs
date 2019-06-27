// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Generic;
using System.CommandLine.Invocation;
using System.Linq;
using System.Reflection;

namespace System.CommandLine.Binding
{
    internal class MethodInfoHandlerDescriptor : HandlerDescriptor
    {
        private readonly MethodInfo _handlerMethodInfo;
        private readonly object _invocationTarget;

        public MethodInfoHandlerDescriptor(MethodInfo handlerMethodInfo, object target = null)
        {
            _handlerMethodInfo = handlerMethodInfo;
            _invocationTarget = target;
        }

        public override ICommandHandler GetCommandHandler()
        {
            var parameterBinders = ParameterDescriptors
                                   .Select(parameterDescriptor => new ModelBinder(parameterDescriptor))
                                   .ToList();

            if (_invocationTarget == null)
            {
                var invocationTargetBinder =
                    _handlerMethodInfo.IsStatic
                        ? null
                        : new ModelBinder(_handlerMethodInfo.DeclaringType);

                return new ModelBindingCommandHandler(
                    _handlerMethodInfo,
                    parameterBinders,
                    invocationTargetBinder);
            }
            else
            {
                if (_handlerMethodInfo.IsStatic)
                {
                    throw new ArgumentException(nameof(_invocationTarget));
                }

                return new ModelBindingCommandHandler(
                    _handlerMethodInfo,
                    parameterBinders,
                    _invocationTarget);
            }
        }

        public override ModelDescriptor Parent => ModelDescriptor.FromType(_handlerMethodInfo.DeclaringType);

        protected override IEnumerable<ParameterDescriptor> InitializeParameterDescriptors() =>
            _handlerMethodInfo.GetParameters()
                              .Select(p => new ParameterDescriptor(p, this));
    }
}
