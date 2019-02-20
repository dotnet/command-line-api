// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Generic;
using System.CommandLine.Invocation;
using System.Linq;
using System.Reflection;

namespace System.CommandLine.Binding
{
    public abstract class HandlerDescriptor
    {
        private List<ParameterDescriptor> _parameterDescriptors;

        public abstract ICommandHandler GetCommandHandler();

        public IReadOnlyList<ParameterDescriptor> ParameterDescriptors =>
            _parameterDescriptors ?? (_parameterDescriptors = new List<ParameterDescriptor>(InitializeParameterDescriptors()));

        protected abstract IEnumerable<ParameterDescriptor> InitializeParameterDescriptors();

        public static HandlerDescriptor FromMethodInfo(MethodInfo methodInfo) =>
            new MethodInfoHandlerDescriptor(methodInfo);
    }
}
