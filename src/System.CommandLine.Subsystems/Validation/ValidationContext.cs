// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.CommandLine.Parsing;
using System.CommandLine.ValueSources;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace System.CommandLine.Validation;

/// <summary>
/// Provides the context for IValidator implementations
/// </summary>
public class ValidationContext
{
    private PipelineResult pipelineResult { get; }

    internal ValidationContext(PipelineResult pipelineResult, ValidationSubsystem validationSubsystem)
    {
        this.pipelineResult = pipelineResult;
        ValidationSubsystem = validationSubsystem;
    }

    /// <summary>
    /// Adds an error to the PipelineContext.
    /// </summary>
    /// <param name="error">The <see cref="ParseError"/> to add</param>
    public void AddError(ParseError error)
        => pipelineResult.AddError(error);

    /// <summary>
    /// Gets the value for an option or argument.
    /// </summary>
    /// <param name="valueSymbol">The symbol to get the value for.</param>
    /// <returns></returns>
    public object? GetValue(CliValueSymbol valueSymbol)
        => pipelineResult.GetValue<object>(valueSymbol);

    /// <summary>
    /// Gets the <see cref="ValueResult"/> for the option or argument, if the user entered a value. 
    /// </summary>
    /// <param name="valueSymbol">The symbol to get the ValueResult for.</param>
    /// <returns>The ValueResult for the option or argument, or null if the user did not enter a value.</returns>
    public CliValueResult? GetValueResult(CliValueSymbol valueSymbol)
        => pipelineResult.GetValueResult(valueSymbol);

    /// <summary>
    /// Tries to get the value for a <see cref="ValueSource"/> and returns it a an `out` parameter.
    /// </summary>
    /// <typeparam name="T">The type of the value to retrieve</typeparam>
    /// <param name="valueSource">The <see cref="ValueSource"/> to query for its result.</param>
    /// <param name="value">An output parameter that contains the value, if it is found.</param>
    /// <returns>True if the <see cref="ValueSource"/> succeeded, otherwise false.</returns>
    public bool TryGetTypedValue<T>(ValueSource<T> valueSource, out T? value)
        => valueSource.TryGetTypedValue(pipelineResult, out value);

    internal ValidationSubsystem ValidationSubsystem { get; }
}
