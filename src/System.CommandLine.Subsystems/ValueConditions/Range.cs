// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.CommandLine.Parsing;
using System.CommandLine.Validation;
using System.CommandLine.ValueSources;

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

public class Range<T>(ValueSource<T>? lowerBound, ValueSource<T>? upperBound, RangeBounds rangeBound = 0)
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
        if (LowerBound is not null
            && LowerBound.TryGetTypedValue(validationContext.PipelineResult, out var lowerValue)
            && comparableValue.CompareTo(lowerValue) < 0)
        {
            validationContext.PipelineResult.AddError(new ParseError($"The value for '{valueSymbol.Name}' is below the lower bound of {LowerBound}"));
        }


        if (UpperBound is not null
           && UpperBound.TryGetTypedValue(validationContext.PipelineResult, out var upperValue)
           && comparableValue.CompareTo(upperValue) > 0)
        {
            validationContext.PipelineResult.AddError(new ParseError($"The value for '{valueSymbol.Name}' is above the upper bound of {UpperBound}"));
        }
    }

    public ValueSource<T>? LowerBound { get; init; } = lowerBound;
    public ValueSource<T>? UpperBound { get; init; } = upperBound;
    public RangeBounds RangeBound { get; } = rangeBound;

}
