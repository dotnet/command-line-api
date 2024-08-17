// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace System.CommandLine;

public class Range : ValueCondition
{
    public static Range CreateRange<T>(T? lowerBound, T? upperBound)
        where T : IComparable<T>
    {
        return new Range
        {
            LowerBound = lowerBound,
            UpperBound = upperBound,
            ValueType = typeof(T)
        };
    }

    public required Type ValueType { get; init; }

    public object? LowerBound { get; init; }
    public object? UpperBound { get; init; }
}
