// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Generic;

namespace System.CommandLine.Parsing;

/// <summary>
/// The publicly facing class for argument and option data.
/// </summary>
public class CliValueResult : CliSymbolResult
{
    internal CliValueResult(
        CliValueSymbol valueSymbol,
        object? value,
        IEnumerable<Location> locations,
        ValueResultOutcome outcome,
        // TODO: Error should be an Enumerable<Error> and perhaps should not be here at all, only on ParseResult
        string? error = null)
        : base(locations)
    {
        ValueSymbol = valueSymbol;
        Value = value;
        Outcome = outcome;
        // TODO: Probably a collection of errors here
        Error = error;
    }

    /// <summary>
    /// The CliSymbol the value is for. This is always a CliOption or CliArgument.
    /// </summary>
    public CliValueSymbol ValueSymbol { get; }

    internal object? Value { get; }

    /// <summary>
    /// Returns the value, or the default for the type.
    /// </summary>
    /// <typeparam name="T">The type to return</typeparam>
    /// <returns>The value, cast to the requested type.</returns>
    public T? GetValue<T>()
        => Value is null
            ? default
            : (T?)Value;

    /// <summary>
    /// Returns the value, or the default for the type.
    /// </summary>
    /// <returns>The value, cast to the requested type.</returns>
    public object? GetValue()
        => Value is null
            ? default
            : Value;

    /// <summary>
    /// True when parsing and converting the value was successful
    /// </summary>
    public ValueResultOutcome Outcome { get; }

    /// <summary>
    /// Parsing and conversion errors when parsing or converting failed.
    /// </summary>
    public string? Error { get; }

    /// <summary>
    /// Returns text suitable for display. 
    /// </summary>
    /// <returns></returns>
    /// <exception cref="NotImplementedException"></exception>
    public IEnumerable<string> TextForDisplay()
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Retrieve the portion of the user's entry that was used for this ValuResult.
    /// </summary>
    /// <returns>The text the user entered that resulted in this ValueResult.</returns>
    /// <exception cref="NotImplementedException"></exception>
    public IEnumerable<string> TextForCommandReconstruction()
    {
        // TODO: Write method to retrieve from location.
        throw new NotImplementedException();
    }

    /// <summary>
    /// Retrieve the portion of the user's entry that was used for this ValueResult.
    /// </summary>
    /// <returns>The text the user entered that resulted in this ValueResult.</returns>
    /// <exception cref="NotImplementedException"></exception>
    public override string ToString()
        => $"{nameof(CliArgumentResultInternal)} {ValueSymbol.Name}: {string.Join(" ", TextForDisplay())}";


    // TODO: This might not be the right place for this, (Some completion stuff was stripped out. This was a private method in ArgumentConversionResult)
    /*
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
    */
}
