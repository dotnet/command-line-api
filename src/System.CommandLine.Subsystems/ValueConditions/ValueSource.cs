// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace System.CommandLine.ValueConditions;

public abstract class ValueSource
{
    public abstract object? GetValue(PipelineResult pipelineResult);

    // TODO: Should we use ToString() here?
    public abstract string Description { get; }
}

public abstract class ValueSource<T> : ValueSource
{
    public abstract T GetTypedValue(PipelineResult pipelineResult);

    public override object? GetValue(PipelineResult pipelineResult)
    {
        return GetTypedValue(pipelineResult);
    }
}

public class SimpleValueSource<T>(T value, string description)
    : ValueSource<T>
{
    public override string Description { get; } = description;

    public override T GetTypedValue(PipelineResult pipelineResult)
        => value;
}

// Find an example of when this is useful beyond Random and Guid. Is a time lag between building the CLI and validating important (DateTime.Now())
public class CalculatedValueSource<T>(Func<T> calculation, string description)
    : ValueSource<T>
{
    public override string Description { get; } = description;

    public override T GetTypedValue(PipelineResult pipelineResult)
        => calculation();
}

public class RelativeToSymbolValueSource<T>(CliValueSymbol otherSymbol, Func<object?, T> calculation, string description)
    : ValueSource<T>
{
    public override string Description { get; } = description;

    public override T GetTypedValue(PipelineResult pipelineResult)
        => calculation(pipelineResult.GetValue(otherSymbol));
}

public class RelativeToEnvironmentVariableValueSource<T>(string environmentVariableName, Func<object?, T> calculation, string description)
    : ValueSource<T>
{
    public override string Description { get; } = description;

    public override T GetTypedValue(PipelineResult pipelineResult)
        => calculation(Environment.GetEnvironmentVariable(environmentVariableName));
}

