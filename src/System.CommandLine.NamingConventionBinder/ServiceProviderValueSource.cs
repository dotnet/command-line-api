// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace System.CommandLine.Binding
{
    internal class ServiceProviderValueSource : IValueSource
    {
        public bool TryGetValue(
            IValueDescriptor valueDescriptor,
            BindingContext? bindingContext,
            out object? boundValue)
        {
            boundValue = bindingContext?.ServiceProvider.GetService(valueDescriptor.ValueType);
            return true;
        }
    }
}
