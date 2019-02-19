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

        public MethodInfoHandlerDescriptor(MethodInfo handlerMethodInfo)
        {
            _handlerMethodInfo = handlerMethodInfo;
        }

        public override ICommandHandler GetCommandHandler()
        {
            var invocationTargetBinder =
                _handlerMethodInfo.IsStatic
                    ? null
                    : new ModelBinder(_handlerMethodInfo.DeclaringType);

            var parameterBinders = ParameterDescriptors
                                   .Select(parameterDescriptor => new ModelBinder(parameterDescriptor))
                                   .ToList();

            return new ModelBindingCommandHandler(
                _handlerMethodInfo,
                parameterBinders,
                invocationTargetBinder);
        }

        protected override IEnumerable<ParameterDescriptor> InitializeParameterDescriptors() =>
            _handlerMethodInfo.GetParameters()
                              .Select(p => new ParameterDescriptor(p));
    }
}
