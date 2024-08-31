// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.CommandLine.Parsing;
using System.CommandLine.Validation;
using System.CommandLine.ValueSources;

namespace System.CommandLine.ValueConditions;

/// <summary>
/// Declares the range condition for the option or argument.
/// </summary>
/// <remarks>
/// The non-generic version is used by the <see cref="RangeValidator/>.
/// </remarks>
public abstract class Range : ValueCondition
{
    protected Range(Type valueType)
        : base(nameof(Range))
    {
        ValueType = valueType;
    }

    /// <summary>
    /// The type of the symbol the range applies to.
    /// </summary>
    public Type ValueType { get; }
}

/// <summary>
/// Declares the range condition for the option or argument. Instances 
/// of this method are created via extension methods on <see cref="ValueSymbol"/>
/// </summary>
/// <typeparam name="T">The type of the symbol the range applies to.</typeparam>
public class Range<T> : Range, IValueValidator
    where T : IComparable<T>
{
    internal Range(ValueSource<T>? lowerBound, ValueSource<T>? upperBound, RangeBounds rangeBound = 0) : base(typeof(T))
    {
        LowerBound = lowerBound;
        UpperBound = upperBound;
        RangeBound = rangeBound;
    }    
    
    /// <inheritdoc/>
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
            && validationContext.TryGetTypedValue(LowerBound, out var lowerValue)
            && comparableValue.CompareTo(lowerValue) < 0)
        {
            validationContext.AddError(new ParseError($"The value for '{valueSymbol.Name}' is below the lower bound of {LowerBound}"));
        }


        if (UpperBound is not null
           && validationContext.TryGetTypedValue(UpperBound, out var upperValue)
           && comparableValue.CompareTo(upperValue) > 0)
        {
            validationContext.AddError(new ParseError($"The value for '{valueSymbol.Name}' is above the upper bound of {UpperBound}"));
        }
    }

    /// <summary>
    /// The lower bound of the range.
    /// </summary>
    public ValueSource<T>? LowerBound { get; init; }

    /// <summary>
    /// The upper bound of the range.
    /// </summary>
    public ValueSource<T>? UpperBound { get; init; }

    /// <summary>
    /// Whether values of the range are considered part of the 
    /// range (inclusive) or not (exclusive)
    /// </summary>
    public RangeBounds RangeBound { get; }

}
