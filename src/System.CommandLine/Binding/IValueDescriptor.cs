// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace System.CommandLine.Binding
{
    public interface IValueDescriptor
    {
        string ValueName { get; }

        Type ValueType { get; }

        bool HasDefaultValue { get; }

        object? GetDefaultValue();
    }

    public interface IValueDescriptor<T> : IValueDescriptor
    {
        // FIX: (IValueDescriptor) 
    }
}
