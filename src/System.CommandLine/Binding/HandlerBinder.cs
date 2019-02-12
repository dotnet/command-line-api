// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Generic;

namespace System.CommandLine.Binding
{
    public class HandlerBinder : BinderBase
    {
        public HandlerDescriptor HandlerDescriptor { get; }

        public HandlerBinder(HandlerDescriptor handlerDescriptor)
        {
            HandlerDescriptor = handlerDescriptor;
        }

        public IReadOnlyCollection<object> GetHandlerArguments(BindingContext context)
        {
            return GetValues(context, HandlerDescriptor.ParameterDescriptors);
        }
    }
}
