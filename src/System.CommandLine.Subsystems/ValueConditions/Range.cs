// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.CommandLine.Parsing;
using System.CommandLine.Validation;

namespace System.CommandLine.ValueConditions;

public abstract class Range : ValueCondition
{
    protected Range(Type valueType)
        : base(nameof(Range))
    {
        ValueType = valueType;
    }
    public Type ValueType { get; }
}

public class Range<T>(RangeBound<T>? lowerBound, RangeBound<T>? upperBound)
    : Range(typeof(T)), IValueValidator
    where T : IComparable<T>
{
    public void Validate(object? value,
                         CliValueSymbol valueSymbol,
                         CliValueResult? valueResult,
                         ValueCondition valueCondition,
                         ValidationContext validationContext)
    {
        if (valueCondition != this) throw new InvalidOperationException("Unexpected value condition type");
        if (value is not T comparableValue) throw new InvalidOperationException("Unexpected value type");

        if (comparableValue is null) return; // nothing to do

        // TODO: Replace the strings we are comparing with a diagnostic ID when we update ParseError
        if (LowerBound is not null)
        {
            var lowerValue = LowerBound.ValueSource.GetTypedValue(validationContext.PipelineResult);
            if (comparableValue.CompareTo(lowerValue) < 0)
            {
                validationContext.PipelineResult.AddError(new ParseError($"The value for '{valueSymbol.Name}' is below the lower bound of {LowerBound}"));
            }
        }

        if (UpperBound is not null)
        {
            var upperValue = UpperBound.ValueSource.GetTypedValue(validationContext.PipelineResult);
            if (comparableValue.CompareTo(upperValue) > 0)
            {
                validationContext.PipelineResult.AddError(new ParseError($"The value for '{valueSymbol.Name}' is above the upper bound of {UpperBound}"));
            }
        }
    }

    public RangeBound<T>? LowerBound { get; init; } = lowerBound;
    public RangeBound<T>? UpperBound { get; init; } = upperBound;
}
