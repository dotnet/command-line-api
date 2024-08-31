// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.CommandLine.ValueSources;

namespace System.CommandLine;

internal class ValueProvider
{
    private Dictionary<CliSymbol, object?> cachedValues = [];
    private PipelineResult pipelineResult;

    public ValueProvider(PipelineResult pipelineResult)
    {
        this.pipelineResult = pipelineResult;
    }

    private void SetValue(CliSymbol symbol, object? value)
    {
        cachedValues[symbol] = value;
    }

    private bool TryGetValue<T>(CliSymbol symbol, out T? value)
    {
        if (cachedValues.TryGetValue(symbol, out var objectValue))
        {
            value = objectValue is null
                ? default
                : (T)objectValue;
            return true;
        }
        value = default;
        return false;
    }

    public T? GetValue<T>(CliValueSymbol valueSymbol)
        => GetValueInternal<T>(valueSymbol);

    private T? GetValueInternal<T>(CliValueSymbol valueSymbol)
    {
        // TODO: Add guard to prevent reentrancy for the same symbol via RelativeToSymbol of custom ValueSource
        var _ = valueSymbol ?? throw new ArgumentNullException(nameof(valueSymbol));
        if (TryGetValue<T>(valueSymbol, out var value))
        {
            return value;
        }
        if (pipelineResult.ParseResult?.GetValueResult(valueSymbol) is { } valueResult)
        {
            return UseValue(valueSymbol, valueResult.GetValue<T>());
        }
        if (valueSymbol.TryGetDefaultValueSource(out ValueSource? defaultValueSource))
        {
            if (defaultValueSource is not ValueSource<T> typedDefaultValueSource)
            {
                throw new InvalidOperationException("Unexpected ValueSource type");
            }
            if (typedDefaultValueSource.TryGetTypedValue(pipelineResult, out T? defaultValue))
            {
                return UseValue(valueSymbol, defaultValue);
            }
        }
        return UseValue(valueSymbol, default(T));

        TValue UseValue<TValue>(CliSymbol symbol, TValue value)
        {
            SetValue(symbol, value);
            return value;
        }
    }

    private static T? CalculatedDefault<T>(CliValueSymbol valueSymbol, Func<T?> defaultValueCalculation)
    {
        var objectValue = defaultValueCalculation();
        var value = objectValue is null
            ? default
            : (T)objectValue;
        return value;
    }
}
