// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace System.CommandLine.Binding
{
    internal class TypeDefaultValueSource : IValueSource
    {
        public static IValueSource Instance = new TypeDefaultValueSource();

        public bool TryGetValue(
            IValueDescriptor valueDescriptor,
            BindingContext bindingContext,
            out object value)
        {
            value = valueDescriptor.Type.GetDefaultValueForType();
            return true;
        }
    }
}
