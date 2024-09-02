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
public sealed class SymbolValueSource<T>
    : ValueSource<T>
{
    // TODO: API differences between this adn RelativeToSymbols are very annoying
    internal SymbolValueSource(
       CliValueSymbol otherSymbol,
       Func<object?, (bool success, T? value)>? calculation = null,
       bool onlyUserEnteredValues = false,
       string? description = null)
    {
        OtherSymbol = otherSymbol;
        OnlyUserEnteredValues = onlyUserEnteredValues;
        Calculation = calculation;
        Description = description;
    }

    public override string? Description { get; }
    public CliValueSymbol OtherSymbol { get; }
    public bool OnlyUserEnteredValues { get; }
    public Func<object?, (bool success, T? value)>? Calculation { get; }

    /// <inheritdoc/>
    public override bool TryGetTypedValue(PipelineResult pipelineResult, out T? value)
    {
        if (OnlyUserEnteredValues && pipelineResult.GetValueResult(OtherSymbol) is null)
        {
            value = default;
            return false;
        }

        if (pipelineResult.TryGetValue(OtherSymbol, out var otherSymbolValue))
        {
            if (Calculation is null)
            {
                value = (T?)otherSymbolValue;
                return true;
            }
            (var success, var newValue) = Calculation(otherSymbolValue);
            if (success)
            {
                value = newValue;
                return true;
            }
        }

        value = default;
        return false;
    }
}

