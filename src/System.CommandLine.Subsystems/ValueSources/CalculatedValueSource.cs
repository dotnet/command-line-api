// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace System.CommandLine.ValueSources;

// Find an example of when this is useful beyond Random and Guid. Is a time lag between building the CLI and validating important (DateTime.Now())
public sealed class CalculatedValueSource<T> : ValueSource<T>
{
    private readonly Func<(bool success, T? value)> calculation;

    internal CalculatedValueSource(Func<(bool success, T? value)> calculation, string? description = null)
    {
        this.calculation = calculation;
        Description = description;
    }

    public override string? Description { get; }

    public override bool TryGetTypedValue(PipelineResult pipelineResult, out T? value)
    {
        (bool success, T? newValue) = calculation();
        if (success)
        {
            value = newValue;
            return true;
        }
        value = default;
        return false;
    }
}

