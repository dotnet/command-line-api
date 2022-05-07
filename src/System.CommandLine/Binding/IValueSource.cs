// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace System.CommandLine.Binding
{
    /// <summary>
    /// Binds a value from a <see cref="BindingContext"/> based on a <see cref="IValueDescriptor"/>.
    /// </summary>
    public interface IValueSource
    {
        /// <summary>
        /// Gets a value from a binding context. A return value indicates whether a value matching the specified value descriptor was present.
        /// </summary>
        /// <param name="valueDescriptor">The descriptor for the value to be bound.</param>
        /// <param name="bindingContext">The binding context from which to bind the value.</param>
        /// <param name="boundValue">The bound value.</param>
        /// <returns><see langword="true"/> if a matching value was found; otherwise, <see langword="false"/>.</returns>
        bool TryGetValue(
            IValueDescriptor valueDescriptor,
            BindingContext bindingContext,
            out object? boundValue);
    }
}
