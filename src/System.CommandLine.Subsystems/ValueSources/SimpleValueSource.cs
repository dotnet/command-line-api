// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace System.CommandLine.ValueSources;

public sealed class SimpleValueSource<T>
    : ValueSource<T>
{
    internal SimpleValueSource(T value, string? description = null)
    {
        Value = value;
        Description = description;
    }

    public T Value { get; }
    public override string? Description { get; }

    public override bool TryGetTypedValue(PipelineResult pipelineResult, out T? value)
    {
        value = Value;
        return true;
    }
}

