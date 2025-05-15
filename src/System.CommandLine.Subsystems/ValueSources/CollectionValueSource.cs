﻿// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace System.CommandLine.ValueSources;

// TODO: Consider creating a set of classes with different arity: T1 and T1, T2 and T1, T2, T3, etc.

/// <summary>
/// <see cref="ValueSource"/> that returns the value value calculated from a set of other symbols.
/// The calculation must be supplied. the returned value of the calculation is returned.
/// </summary>
/// <typeparam name="T">The type to be returned, which is almost always the type of the symbol the ValueSource will be used for.</typeparam>
/// <param name="otherSymbols">The <see cref="CliOption">, <see cref="CliArgument"/>, or <see cref="CalculatedValue"/> to include as sources.</param>
/// <param name="calculation">A delegate that returns a value of the type of the collection source, which can be either a single value or a collection of values.</param>
/// <param name="description">The description of this value, used to clarify the intent of the values that appear in error messages.</param>
public sealed class CollectionValueSource<T>
    : ValueSource<T>
{
    internal CollectionValueSource(
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

    /// <summary>
    /// The description that will be used in messages, such as value conditions
    /// </summary>
    public override string? Description { get; }

    /// <summary>
    /// The other symbols that this ValueSource depends on.
    /// </summary>
    public IEnumerable<CliValueSymbol> OtherSymbols { get; }

    /// <summary>
    /// If true, default values will not be used.
    /// </summary>
    // TODO: Find scenarios for good tests. This is based on intuition not a known scenario. Overall we are pretty aggressive about default values.
    public bool OnlyUserEnteredValues { get; }

    /// <summary>
    /// The calculation that determines a single value, which might an instance of a complex type, based on the values provided.
    /// </summary>
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

