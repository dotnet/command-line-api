// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace System.CommandLine.Rendering.Views
{
    public class RowDefinition
    {
        public double Value { get; }
        public SizeMode SizeMode { get; }

        private RowDefinition(SizeMode sizeMode, double value)
        {
            SizeMode = sizeMode;
            Value = value;
        }
        public static RowDefinition Fixed(int size)
        {
            if (size < 0.0)
            {
                throw new ArgumentException("Fixed size cannot be negative", nameof(size));
            }
            return new RowDefinition(SizeMode.Fixed, size);
        }

        public static RowDefinition Star(double weight)
        {
            if (weight < 0.0)
            {
                throw new ArgumentException("Weight cannot be negative", nameof(weight));
            }
            return new RowDefinition(SizeMode.Star, weight);
        }
        
        public static RowDefinition SizeToContent() => new(SizeMode.SizeToContent, 0);
    }
}
