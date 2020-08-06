// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace System.CommandLine.Binding
{
    public interface IValueSource
    {
        bool TryGetValue(
            IValueDescriptor valueDescriptor,
            BindingContext? bindingContext,
            out object? boundValue);
    }
}
