// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace System.CommandLine.Binding
{
    /// <summary>
    /// A value created by binding command line input.
    /// </summary>
    public readonly struct BoundValue
    {
        internal BoundValue(
            object? value,
            IValueDescriptor valueDescriptor,
            IValueSource valueSource)
        {
            Value = value;
            ValueDescriptor = valueDescriptor;
            ValueSource = valueSource;
        }

        /// <summary>
        /// The descriptor for the bound value.
        /// </summary>
        public IValueDescriptor ValueDescriptor { get; }

        /// <summary>
        /// The source from which the value was bound.
        /// </summary>
        public IValueSource ValueSource { get; }

        /// <summary>
        /// The value bound from the specified source.
        /// </summary>
        public object? Value { get; }

        /// <inheritdoc />
        public override string ToString() => $"{ValueDescriptor}: {Value}";
    }
}
