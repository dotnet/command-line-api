// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace System.CommandLine.Binding
{
    internal class TypeDefaultValueSource : IValueSource
    {
        public static readonly IValueSource Instance = new TypeDefaultValueSource();

        private TypeDefaultValueSource()
        {
        }

        public bool TryGetValue(
            IValueDescriptor valueDescriptor,
            BindingContext? bindingContext,
            out object? boundValue)
        {
            boundValue = Binder.GetDefaultValue(valueDescriptor.ValueType);
            return true;
        }
    }
}