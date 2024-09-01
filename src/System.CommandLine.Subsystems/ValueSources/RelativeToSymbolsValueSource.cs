// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace System.CommandLine.ValueSources;

/// <summary>
/// <see cref="ValueSource"/> that returns the value of the specified other symbol.
/// If the calculation delegate is supplied, the returned value of the calculation is returned.
/// </summary>
/// <typeparam name="T">The type to be returned, which is almost always the type of the symbol the ValueSource will be used for.</typeparam>
/// <param name="otherSymbol">The option or argument to return, with the calculation supplied if it is not null.</param>
/// <param name="calculation">A delegate that returns the requested type.</param>
/// <param name="description">The description of this value, used to clarify the intent of the values that appear in error messages.</param>
    // TODO: Do we want this to be an aggregate, such that you could build a type from other symbols, calcs and env variables. Ooo aahh
public sealed class RelativeToSymbolsValueSource<T>
    : ValueSource<T>
{
    internal RelativeToSymbolsValueSource(
       Func<IEnumerable<object?>, (bool success, T? value)> calculation,
       bool onlyUserEnteredValues = false,
       string? description = null,
       params CliValueSymbol[] otherSymbols)
    {
        OtherSymbols = otherSymbols;
        OnlyUserEnteredValues = onlyUserEnteredValues;
        Calculation = calculation;
        Description = description;
    }

    public override string? Description { get; }
    public IEnumerable<CliValueSymbol> OtherSymbols { get; }
    public bool OnlyUserEnteredValues { get; }
    public Func<IEnumerable<object?>, (bool success, T? value)> Calculation { get; }

    /// <inheritdoc/>
    public override bool TryGetTypedValue(PipelineResult pipelineResult, out T? value)
    {
        // TODO: How do we test for only user values. (no defaults)
        //if (OnlyUserEnteredValues && pipelineResult.GetValueResult(OtherSymbol) is null)
        //{
        //    value = default;
        //    return false;
        //}

        var otherSymbolValues = OtherSymbols.Select(GetOtherSymbolValues).ToArray();
        (var success, var newValue) = Calculation(otherSymbolValues);
        if (success)
        {
            value = newValue;
            return true;
        }

        value = default;
        return false;

        object? GetOtherSymbolValues(CliValueSymbol otherSymbol)
        {
            if (pipelineResult.TryGetValue(otherSymbol, out var otherSymbolValue))
            {
                return otherSymbolValue;
            }
            // TODO: I suspect we will need more data here, such as whether it exists and whether user entered
            return null;
        }
    }
}

