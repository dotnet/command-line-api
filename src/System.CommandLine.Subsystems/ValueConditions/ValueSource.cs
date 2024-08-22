// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using static System.Runtime.InteropServices.JavaScript.JSType;

namespace System.CommandLine.ValueConditions;

public abstract class ValueSource
{
    public abstract object? GetValue(PipelineResult pipelineResult);

    // TODO: Should we use ToString() here?
    public abstract string Description { get; }
    public static ValueSource<T> Create<T>(T value, string? description = null)
        => new SimpleValueSource<T>(value, description);

    public static ValueSource<T> Create<T>(Func<T> calculation, string? description = null)
        => new CalculatedValueSource<T>(calculation);

    public static ValueSource<T> Create<T>(CliValueSymbol otherSymbol, Func<object, T>? calculation = null, string? description = null)
        => new RelativeToSymbolValueSource<T>(otherSymbol, calculation, description);

    public static ValueSource<T> CreateFromEnvironmentVariable<T>(string environmentVariableName, Func<string?, T>? calculation = null, string? description = null)
        => new RelativeToEnvironmentVariableValueSource<T>(environmentVariableName, calculation, description);
}

public abstract class ValueSource<T> : ValueSource
{
    public abstract T GetTypedValue(PipelineResult pipelineResult);

    public override object? GetValue(PipelineResult pipelineResult)
    {
        return GetTypedValue(pipelineResult);
    }

    public static implicit operator ValueSource<T>(T value) => new SimpleValueSource<T>(value);
    public static implicit operator ValueSource<T>(Func<T> calculated) => new CalculatedValueSource<T>(calculated);
    public static implicit operator ValueSource<T>(CliValueSymbol symbol) => new RelativeToSymbolValueSource<T>(symbol);
    // Environment variable does not have an explicit operator, because converting to string was too broad
}

public class SimpleValueSource<T>(T value, string? description = null)
    : ValueSource<T>
{
    public override string Description { get; } = description;

    public override T GetTypedValue(PipelineResult pipelineResult)
        => value;
}

// Find an example of when this is useful beyond Random and Guid. Is a time lag between building the CLI and validating important (DateTime.Now())
public class CalculatedValueSource<T>(Func<T> calculation, string? description = null)
    : ValueSource<T>
{
    public override string Description { get; } = description;

    public override T GetTypedValue(PipelineResult pipelineResult)
        => calculation();
}

public class RelativeToSymbolValueSource<T>(CliValueSymbol otherSymbol,
                                            Func<object, T>? calculation = null,
                                            string? description = null)
    : ValueSource<T>
{
    public override string Description { get; } = description;

    public override T GetTypedValue(PipelineResult pipelineResult)
        => calculation is null
                ? pipelineResult.GetValue<T>(otherSymbol)
                : calculation(pipelineResult.GetValue(otherSymbol));
}

public class RelativeToEnvironmentVariableValueSource<T>(string environmentVariableName,
                                                         Func<string?, T>? calculation = null,
                                                         string? description = null)
    : ValueSource<T>
{
    public override string Description { get; } = description;

    public override T GetTypedValue(PipelineResult pipelineResult)
    {
        string? stringValue = Environment.GetEnvironmentVariable(environmentVariableName);
       
        if (stringValue is null)
        {
            // This feels wrong. It isn't saying "Hey, you asked for a value that was not there"
            return default;
        }

        // TODO: What is the best way to do this?
        T value = default(T) switch
        {
            int i => (T)(object)Convert.ToInt32(stringValue),
            _ => throw new NotImplementedException("Looking for a non-dumb way to do this")
        };
        return calculation is null
            ? value
            : calculation(Environment.GetEnvironmentVariable(environmentVariableName));
    }
}

