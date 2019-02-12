// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace System.CommandLine.Binding
{
    public class BoundValue
    {
        internal BoundValue(
            object value,
            IValueDescriptor valueDescriptor,
            object valueSource)
        {
            Value = value;
            ValueDescriptor = valueDescriptor;
            ValueSource = valueSource;
        }

        // FIX: (BoundValue) change ValueSource to IValueSource
        public IValueDescriptor ValueDescriptor { get; }

        public object ValueSource { get; }

        public object Value { get; }

        public static BoundValue DefaultForType(IValueDescriptor valueDescriptor)
        {
            var valueSource = TypeDefaultValueSource.Instance;

            valueSource.TryGetValue(valueDescriptor, null, out var value);

            return new BoundValue(
                value,
                valueDescriptor,
                valueSource);
        }
    }
}
