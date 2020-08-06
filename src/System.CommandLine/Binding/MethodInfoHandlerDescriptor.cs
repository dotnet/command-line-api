﻿// Copyright (c) .NET Foundation and contributors. All rights reserved.
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
        private readonly object? _invocationTarget;

        public MethodInfoHandlerDescriptor(
            MethodInfo handlerMethodInfo,
            object? target = null)
        {
            _handlerMethodInfo = handlerMethodInfo ??
                                 throw new ArgumentNullException(nameof(handlerMethodInfo));
            _invocationTarget = target;
        }

        public override ICommandHandler GetCommandHandler()
        {
            if (_invocationTarget is null)
            {
                return new ModelBindingCommandHandler(
                    _handlerMethodInfo,
                    ParameterDescriptors);
            }
            else
            {
                return new ModelBindingCommandHandler(
                    _handlerMethodInfo,
                    ParameterDescriptors,
                    _invocationTarget);
            }
        }

        public override ModelDescriptor Parent => ModelDescriptor.FromType(_handlerMethodInfo.DeclaringType);

        protected override IEnumerable<ParameterDescriptor> InitializeParameterDescriptors() =>
            _handlerMethodInfo.GetParameters()
                              .Select(p => new ParameterDescriptor(p, this));
    }
}