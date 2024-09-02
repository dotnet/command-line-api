// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace System.CommandLine.ValueSources;

public sealed class FallbackValueSource<T> : ValueSource<T>
{
    private List<ValueSource<T>> valueSources = [];

    internal FallbackValueSource(ValueSource<T> firstSource,
                                 ValueSource<T> secondSource,
                                 string? description = null,
                                 params ValueSource<T>[] otherSources)
    {
        valueSources.AddRange([firstSource, secondSource, .. otherSources]);
        Description = description;
    }


    public override string? Description { get; }

    public bool PrecedenceAsEntered { get; set; }

    public override bool TryGetTypedValue(PipelineResult pipelineResult, out T? value)
    {
        var orderedSources = PrecedenceAsEntered
            ? valueSources
            : [.. valueSources.OrderBy(GetPrecedence)];
        foreach (var source in orderedSources)
        {
            if (source.TryGetTypedValue(pipelineResult, out var newValue))
            {
                value = newValue;
                return true;
            }
        }
        value = default;
        return false;

    }

    // TODO: Discuss precedence vs order entered for aggregates
    internal static int GetPrecedence(ValueSource<T> source)
    {
        return source switch
        {
            SimpleValueSource<T> => 0,
            SymbolValueSource<T> => 1,
            CalculatedValueSource<T> => 2,
            //RelativeToConfigurationValueSource<T> => 3,
            EnvironmentVariableValueSource<T> => 4,
            _ => 5
        };
    }
}


