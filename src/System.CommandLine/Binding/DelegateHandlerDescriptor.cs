// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Generic;
using System.CommandLine.Invocation;
using System.Linq;

namespace System.CommandLine.Binding
{
    internal class DelegateHandlerDescriptor : HandlerDescriptor
    {
        private readonly Delegate _handlerDelegate;

        public DelegateHandlerDescriptor(Delegate handlerDelegate)
        {
            _handlerDelegate = handlerDelegate;
        }

        public override ICommandHandler GetCommandHandler()
        {
            return new ModelBindingCommandHandler(
                _handlerDelegate,
                this);
        }

        public override ModelDescriptor? Parent => null;

        protected override IEnumerable<ParameterDescriptor> InitializeParameterDescriptors()
        {
            return _handlerDelegate.Method
                                   .GetParameters()
                                   .Select(p => new ParameterDescriptor(p, this));
        }
    }
}
