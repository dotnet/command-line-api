// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace System.CommandLine.Rendering.Views
{
    public class ColumnDefinition
    {
        public SizeMode SizeMode { get; }

        public double Value { get; }

        private ColumnDefinition(SizeMode sizeMode, double value)
        {
            SizeMode = sizeMode;
            Value = value;
        }

        public static ColumnDefinition Fixed(int size)
        {
            if (size < 0.0)
            {
                throw new ArgumentException("Fixed size cannot be negative", nameof(size));
            }
            return new ColumnDefinition(SizeMode.Fixed, size);
        }

        public static ColumnDefinition Star(double weight)
        {
            if (weight < 0.0)
            {
                throw new ArgumentException("Weight cannot be negative", nameof(weight));
            }
            return new ColumnDefinition(SizeMode.Star, weight);
        }
        
        public static ColumnDefinition SizeToContent() => new(SizeMode.SizeToContent, 0);
    }
}
