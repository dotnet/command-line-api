// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace System.CommandLine.ValueSources;

public class SimpleValueSource<T>(T value, string? description = null)
    : ValueSource<T>
{
    public override string? Description { get; } = description;

    public override (bool success, T? value) GetTypedValue(PipelineResult pipelineResult)
        => (true, value);
}

