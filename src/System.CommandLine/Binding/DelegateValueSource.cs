// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace System.CommandLine.Binding
{
    internal class DelegateValueSource : IValueSource
    {
        private readonly Func<BindingContext?, object?> _getValue;

        public DelegateValueSource(Func<BindingContext?, object?> getValue)
        {
            _getValue = getValue;
        }

        public bool TryGetValue(IValueDescriptor valueDescriptor, BindingContext? bindingContext, out object? boundValue)
        {
            boundValue = _getValue(bindingContext);

            return true;
        }
    }
}
