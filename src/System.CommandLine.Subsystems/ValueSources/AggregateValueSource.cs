// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace System.CommandLine.ValueSources;

public class AggregateValueSource : ValueSource
{
    private List<ValueSource> valueSources = [];

    public override string? Description { get; }

    public override (bool success, object? value) GetValue(PipelineResult pipelineResult)
    {
        throw new NotImplementedException();
    }
}

