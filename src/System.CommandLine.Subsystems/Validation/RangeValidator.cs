// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.CommandLine.Parsing;
using System.CommandLine.Validation.Traits;

namespace System.CommandLine.Validation;

public class RangeValidator : DataValidator<RangeData>
{
    public RangeValidator() : base("Range")
    { }

    public override IEnumerable<ParseError>? Validate(object? value, 
        ValueResult valueResult, Trait trait, ValidationContext context)
    {

        List<ParseError>? parseErrors = null;
        var dataSymbol = valueResult.ValueSymbol;
        var range = GetTypedTraitOrThrow<RangeData>(trait);
        var comparableValue = GetValueAsTypeOrThrow<IComparable>(value);

        // TODO: Replace the strings we are comparing with a diagnostic ID when we update ParseError
        if (range.LowerBound is not null)
        {
            if (comparableValue.CompareTo(range.LowerBound) <= 0)
            {
                AddValidationError(ref parseErrors, $"The value for '{dataSymbol.Name}' is below the lower bound of {range.LowerBound}", []);
            }
        }

        if (range.UpperBound is not null)
        {
            if (comparableValue.CompareTo(range.UpperBound) >= 0)
            {
                AddValidationError(ref parseErrors, $"The value for '{dataSymbol.Name}' is above the upper bound of {range.UpperBound}", []);
            }
        }
        return parseErrors;
    }


}
