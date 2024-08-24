// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace System.CommandLine.ValueSources;

public class RelativeToSymbolValueSource<T>(CliValueSymbol otherSymbol,
                                            bool onlyUserEnteredValues = false,
                                            Func<object?, (bool success, T? value)>? calculation = null,
                                            string? description = null)
    : ValueSource<T>
{
    public override string? Description { get; } = description;

    public override bool TryGetTypedValue(PipelineResult pipelineResult, out T? value)
    {
        if (onlyUserEnteredValues && pipelineResult.GetValueResult(otherSymbol) is null)
        {
            value = default;
            return false;
        }

        var otherSymbolValue = pipelineResult.GetValue<T>(otherSymbol);

        if (calculation is null)
        {
            value = otherSymbolValue;
            return true;
        }
        (var success, var newValue) = calculation(otherSymbolValue);
        if (success)
        {
            value = newValue;
            return true;
        }
        value = default;
        return false;
    }
}

