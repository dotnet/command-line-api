﻿// Copyright (c) .NET Foundation and contributors. All rights reserved.
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
            if (value != null &&
                !valueDescriptor.ValueType.IsInstanceOfType(value))
            {
                throw new ArgumentException($"Value {value} ({value.GetType()}) must be an instance of type {valueDescriptor.ValueType}");
            }

            Value = value;
            ValueDescriptor = valueDescriptor;
            ValueSource = valueSource;
        }

        public IValueDescriptor ValueDescriptor { get; }

        public IValueSource ValueSource { get; }

        public object? Value { get; }

        public override string ToString() => $"{ValueDescriptor}: {Value}";

        public static BoundValue DefaultForType(IValueDescriptor valueDescriptor)
        {
            var valueSource = TypeDefaultValueSource.Instance;

            valueSource.TryGetValue(valueDescriptor, null, out var value);

            return new BoundValue(
                value,
                valueDescriptor,
                valueSource);
        }

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
