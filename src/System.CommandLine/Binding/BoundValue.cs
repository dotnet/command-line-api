// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace System.CommandLine.Binding
{
    public class BoundValue
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

        public IValueDescriptor ValueDescriptor { get; }

        public IValueSource ValueSource { get; }

        public virtual object? Value { get; }

        public override string ToString() => $"{ValueDescriptor}: {Value}";

        /// <summary>
        /// Gets a <see cref="BoundValue"/> representing the default value for a specified <see cref="IValueDescriptor"/>.
        /// </summary>
        /// <param name="valueDescriptor">A value descriptor for which to get the default value.</param>
        /// <returns>A <see cref="BoundValue"/> representing the default value for a specified <see cref="IValueDescriptor"/>.</returns>
        public static BoundValue DefaultForValueDescriptor(IValueDescriptor valueDescriptor)
        {
            var valueSource = ValueDescriptorDefaultValueSource.Instance;

            valueSource.TryGetValue(valueDescriptor, null, out var value);

            return new BoundValue(
                value,
                valueDescriptor,
                valueSource);
        }
    }
}
