// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace System.CommandLine.ValueSources;

public abstract class ValueSource
{
    public abstract (bool success, object? value) GetValue(PipelineResult pipelineResult);

    // TODO: Should we use ToString() here?
    public abstract string? Description { get; }
    public static ValueSource<T> Create<T>(T value, string? description = null)
        => new SimpleValueSource<T>(value, description);

    public static ValueSource<T> Create<T>(Func<(bool success, T? value)> calculation, string? description = null)
        => new CalculatedValueSource<T>(calculation);

    public static ValueSource<T> Create<T>(CliValueSymbol otherSymbol, Func<object, (bool success, T? value)>? calculation = null, string? description = null)
        => new RelativeToSymbolValueSource<T>(otherSymbol, calculation, description);

    public static ValueSource<T> Create<T>(ValueSource<T> firstSource, ValueSource<T> secondSource, string? description = null, params ValueSource<T>[] otherSources)
    {
        return new AggregateValueSource<T>(firstSource, secondSource, description, otherSources);
    }

    public static ValueSource<T> CreateFromEnvironmentVariable<T>(string environmentVariableName, Func<string?, (bool success, T? value)>? calculation = null, string? description = null)
        => new RelativeToEnvironmentVariableValueSource<T>(environmentVariableName, calculation, description);
}

public abstract class ValueSource<T> : ValueSource
{
    public abstract (bool success, T? value) GetTypedValue(PipelineResult pipelineResult);

    public override (bool success, object? value) GetValue(PipelineResult pipelineResult)
    {
        return GetTypedValue(pipelineResult);
    }

    public static implicit operator ValueSource<T>(T value) => new SimpleValueSource<T>(value);
    public static implicit operator ValueSource<T>(Func<(bool success, T? value)> calculated) => new CalculatedValueSource<T>(calculated);
    public static implicit operator ValueSource<T>(CliValueSymbol symbol) => new RelativeToSymbolValueSource<T>(symbol);
    // Environment variable does not have an explicit operator, because converting to string was too broad
}

