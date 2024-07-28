// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.CommandLine.Parsing;
using System.CommandLine.Validation.DataTraits;

namespace System.CommandLine.Validation;

public class RangeValidator : Validator
{
    public RangeValidator(string name) : base("Range")
    { }

    public override IEnumerable<ParseError>? Validate(CliSymbol symbol, DataTrait trait, Pipeline pipeline, ValidationSubsystem validationSubsystem)
    {

        List<ParseError>? parseErrors = null;

        var dataSymbol = GetDataSymbolOrThrow(symbol);
        var range = GetDataTraitOrThrow<RangeData>(trait);
        var comparableValue = GetValueAsTypeOrThrow<IComparable>(dataSymbol, pipeline);

        if (range.LowerBound is not null)
        {
            if (comparableValue.CompareTo(range.LowerBound) <= 0)
            {
                AddValidationError(parseErrors, $"The value for {0} is below the lower bound of {1}", [symbol.Name, range.LowerBound]);
            }
        }

        if (range.UpperBound is not null)
        {
            if (comparableValue.CompareTo(range.UpperBound) >= 0)
            {
                AddValidationError(parseErrors, $"The value for {0} is above the upper bound of {1}", [symbol.Name, range.UpperBound]);
            }
        }
        return parseErrors;
    }


}
