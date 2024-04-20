// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Generic;

namespace System.CommandLine.Parsing;

public class ValueResult
{
    internal ValueResult(
        CliSymbol valueSymbol,
        object? value,
        IEnumerable<Location> locations,
        ValueResultOutcome outcome,
        // TODO: Error should be an Enumerable<Error> and perhaps should not be here at all, only on ParseResult
        string? error = null)
    {
        ValueSymbol = valueSymbol;
        Value = value;
        Locations = locations;
        Outcome = outcome;
        Error = error;
    }

    public CliSymbol ValueSymbol { get; }
    internal object? Value { get; }

    public T? GetValue<T>()
        => (T?)Value;

    // This needs to be a collection because collection types have multiple tokens and they will not be simple offsets when response files are used
    // TODO: Consider more efficient ways to do this in the case where there is a single location
    public IEnumerable<Location> Locations { get; }

    public ValueResultOutcome Outcome { get; }

    public string? Error { get; }

    public IEnumerable<string> TextForDisplay()
    {
        throw new NotImplementedException();
    }

    public IEnumerable<string> TextForCommandReconstruction()
    {
        throw new NotImplementedException();
    }

    public override string ToString()
        //=> $"{nameof(ValueResult)} ({FormatOutcomeMessage()}) {ValueSymbol?.Name}";
        => $"{nameof(ArgumentResult)} {ValueSymbol.Name}: {string.Join(" ", TextForDisplay())}";

 
    // TODO: This definitely feels like the wrong place for this, (Some completion stuff was stripped out. This was a private method in ArgumentConversionResult
    private string FormatOutcomeMessage()
        => ValueSymbol switch
        {
            CliOption option
                => LocalizationResources.ArgumentConversionCannotParseForOption(Value?.ToString() ?? "", option.Name, ValueSymbolType),
            CliCommand command
                => LocalizationResources.ArgumentConversionCannotParseForCommand(Value?.ToString() ?? "", command.Name, ValueSymbolType),
            //TODO
            _ => throw new NotImplementedException()
        };

    private Type ValueSymbolType
        => ValueSymbol switch
        {
            CliArgument argument => argument.ValueType,
            CliOption option => option.Argument.ValueType,
            _ => throw new NotImplementedException()
        };
}
