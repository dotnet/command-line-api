// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace System.CommandLine.Binding
{
    internal class ValueDescriptorDefaultValueSource : IValueSource
    {
        public static readonly IValueSource Instance = new ValueDescriptorDefaultValueSource();

        private ValueDescriptorDefaultValueSource()
        {
        }

        public bool TryGetValue(
            IValueDescriptor valueDescriptor, 
            BindingContext bindingContext, 
            out object? boundValue)
        {
            boundValue = valueDescriptor.GetDefaultValue();
            return true;
        }
    }
}
