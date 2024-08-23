// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace System.CommandLine.ValueSources;

public class RelativeToSymbolValueSource<T>(CliValueSymbol otherSymbol,
                                            Func<object, (bool success, T? value)>? calculation = null,
                                            string? description = null)
    : ValueSource<T>
{
    public override string Description { get; } = description;

    public override (bool success, T? value) GetTypedValue(PipelineResult pipelineResult)
        => calculation is null
                ? (true, pipelineResult.GetValue<T>(otherSymbol))
                : calculation(pipelineResult.GetValue(otherSymbol));
}

