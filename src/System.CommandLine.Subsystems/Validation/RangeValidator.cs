// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.CommandLine.Parsing;

namespace System.CommandLine.Validation;

public class RangeValidator : ValueValidator
{
    public RangeValidator() : base(nameof(Range), typeof(Range))
    { }

    public override void Validate(object? value, CliValueSymbol valueSymbol,
        CliValueResult? valueResult, ValueCondition valueCondition, ValidationContext validationContext)
    {

        var range = GetTypedValueConditionOrThrow<Range>(valueCondition);
        var comparableValue = GetValueAsTypeOrThrow<IComparable>(value);

        // TODO: Replace the strings we are comparing with a diagnostic ID when we update ParseError
        if (range.LowerBound is not null)
        {
            if (comparableValue.CompareTo(range.LowerBound) < 0)
            {
               validationContext.PipelineResult.AddError(new ParseError( $"The value for '{valueSymbol.Name}' is below the lower bound of {range.LowerBound}"));
            }
        }

        if (range.UpperBound is not null)
        {
            if (comparableValue.CompareTo(range.UpperBound) > 0)
            {
                validationContext.PipelineResult.AddError(new ParseError($"The value for '{valueSymbol.Name}' is above the upper bound of {range.LowerBound}"));
            }
        }
    }


}
