// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace System.CommandLine.Binding
{
    public class BoundValue
    {
        // ?? Why have an internal constructor on a public readonly class?
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

        public virtual object? Value { get; }

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

    //public class LazyBoundValue : BoundValue
    //{
    //    private bool _valueHasBeenSet;
    //    internal LazyBoundValue( IValueDescriptor valueDescriptor, IValueSource valueSource)
    //        : base(null, valueDescriptor, valueSource)
    //    {
    //    }

    //    public override object? Value
    //    {
    //        get
    //        {
    //            if (!_valueHasBeenSet )
    //            {
    //                object? value;
    //               if (ValueSource.TryGetValue(ValueDescriptor, ))
    //            }
    //            return base.Value;
    //        }
    //    }
    //}
}
