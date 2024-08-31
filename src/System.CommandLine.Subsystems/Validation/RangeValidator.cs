// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.CommandLine.Parsing;

namespace System.CommandLine.Validation;

/// <summary>
/// Validates that a value is within the specified bounds.
/// </summary>
public class RangeValidator : ValueValidator, IValueValidator
{
    public RangeValidator() : base(nameof(ValueConditions.Range), typeof(ValueConditions.Range))
    { }

    /// <inheritdoc/>
    public override void Validate(object? value, CliValueSymbol valueSymbol,
        CliValueResult? valueResult, ValueCondition valueCondition, ValidationContext validationContext)
    {
        if (valueCondition is IValueValidator valueValidator)
        {
            valueValidator.Validate(value, valueSymbol, valueResult, valueCondition, validationContext);
            return;
        }
        if (valueCondition.MustHaveValidator)
        {
            validationContext.AddError(new ParseError($"Range validator missing for {valueSymbol.Name}"));
        }
    }


}
