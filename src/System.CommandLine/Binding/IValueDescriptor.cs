// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace System.CommandLine.Binding
{
    /// <summary>
    /// Describes and provides access to a bindable named value.
    /// </summary>
    public interface IValueDescriptor
    {
        /// <summary>
        /// The name of the value.
        /// </summary>
        string ValueName { get; }

        /// <summary>
        /// The type of the value.
        /// </summary>
        Type ValueType { get; }

        /// <summary>
        /// Gets a value determining whether there is a default value.
        /// </summary>
        bool HasDefaultValue { get; }

        /// <summary>
        /// Gets the default value, if any.
        /// </summary>
        object? GetDefaultValue();
    }
}
