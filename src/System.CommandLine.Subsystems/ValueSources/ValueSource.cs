// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Diagnostics.CodeAnalysis;

namespace System.CommandLine.ValueSources;

public abstract class ValueSource
{
    internal ValueSource()
    {

    }

    /// <summary>
    /// Supplies the requested value, with the calculation applied if it is not null.
    /// </summary>
    /// <param name="pipelineResult">The current pipeline result.</param>
    /// <param name="value">An out parameter which contains the converted value, with the calculation applied, if it is found.</param>
    /// <returns>True if a value was found, otherwise false.</returns>
    public abstract bool TryGetValue(PipelineResult pipelineResult, out object? value);

    // TODO: Should we use ToString() here?
    public abstract string? Description { get; }

    public static ValueSource<T> Create<T>(T value, string? description = null)
        => new SimpleValueSource<T>(value, description);

    public static ValueSource<T> Create<T>(Func<(bool success, T? value)> calculation,
                                           string? description = null)
        => new CalculatedValueSource<T>(calculation, description);

    public static ValueSource<T> Create<T>(CliValueSymbol otherSymbol,
                                           Func<object?, (bool success, T? value)>? calculation = null,
                                           bool userEnteredValueOnly = false,
                                           string? description = null)
        => new RelativeToSymbolValueSource<T>(otherSymbol, calculation, userEnteredValueOnly, description);

    public static ValueSource<T> Create<T>(
                                       Func<IEnumerable<object?>, (bool success, T? value)> calculation,
                                       bool userEnteredValueOnly = false,
                                       string? description = null,
                                       params CliValueSymbol[] otherSymbols)
        => new RelativeToSymbolsValueSource<T>(calculation, userEnteredValueOnly, description, otherSymbols);

    public static ValueSource<T> Create<T>(ValueSource<T> firstSource,
                                           ValueSource<T> secondSource,
                                           string? description = null,
                                           params ValueSource<T>[] otherSources)
        => new AggregateValueSource<T>(firstSource, secondSource, description, otherSources);

    public static ValueSource<T> CreateFromEnvironmentVariable<T>(string environmentVariableName,
                                                                  Func<string?, (bool success, T? value)>? calculation = null,
                                                                  string? description = null)
        => new RelativeToEnvironmentVariableValueSource<T>(environmentVariableName, calculation, description);
}

// TODO: Determine philosophy for custom value sources and whether they can build on existing sources.
public abstract class ValueSource<T> : ValueSource
{
    /// <summary>
    /// Supplies the requested value, with the calculation applied if it is not null.
    /// </summary>
    /// <param name="pipelineResult">The current pipeline result.</param>
    /// <param name="value">An out parameter which contains the converted value, with the calculation applied, if it is found.</param>
    /// <returns>True if a value was found, otherwise false.</returns>
    // TODO: Determine whether this and `TryGetValue` should have NotNullWhen(true) attribute. Discussion in <reporoot>/OpenQuestions.md
    public abstract bool TryGetTypedValue(PipelineResult pipelineResult,
                                          out T? value);

    /// <inheritdoc/>
    public override bool TryGetValue(PipelineResult pipelineResult,
                                     out object? value)
    {

        if (TryGetTypedValue(pipelineResult, out T? newValue))
        {
            value = newValue;
            return true;
        }
        value = null;
        return false;
    }

    public static implicit operator ValueSource<T>(T value) => new SimpleValueSource<T>(value);
    public static implicit operator ValueSource<T>(Func<(bool success, T? value)> calculated) => new CalculatedValueSource<T>(calculated);
    public static implicit operator ValueSource<T>(CliValueSymbol symbol) => new RelativeToSymbolValueSource<T>(symbol);
    // Environment variable does not have an explicit operator, because converting to string was too broad
}

