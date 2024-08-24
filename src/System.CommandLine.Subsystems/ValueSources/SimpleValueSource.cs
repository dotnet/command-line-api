// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace System.CommandLine.ValueSources;

public class SimpleValueSource<T>(T value, string? description = null)
    : ValueSource<T>
{
    public T Value { get; } = value;
    public override string? Description { get; } = description;

    public override bool TryGetTypedValue(PipelineResult pipelineResult, out T? value)
    {
        value = Value;
        return true;
    }
}

