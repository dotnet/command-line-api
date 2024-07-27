// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace System.CommandLine.Subsystems.DataTraits;

public class Range<T> : DataTrait
    where T : IComparable<T>
{
    public Range(T? lowerBound, T? upperBound)
    {
        LowerBound = lowerBound;
        UpperBound = upperBound;
    }

    public T? LowerBound { get; }
    public T? UpperBound { get; }
}
