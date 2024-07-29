﻿using System.CommandLine.Subsystems.Annotations;
using System.CommandLine.Validation.DataTraits;

namespace System.CommandLine.Validation;

public static class ValidationExtensions
{
    public static void SetRange<T>(this CliDataSymbol symbol, T lowerBound, T upperBound)
        where T : IComparable<T>
    {
        var range = new RangeData
        {
            ValueType = symbol.ValueType,
            LowerBound = lowerBound,
            UpperBound = upperBound
        };

        symbol.SetDataTrait(range);
    }
}