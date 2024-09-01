// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.CommandLine.ValueSources;
using CacheItem = (bool found, object? value);

namespace System.CommandLine;

internal class ValueProvider
{
    private Dictionary<CliSymbol, CacheItem> cachedValues = [];
    private PipelineResult pipelineResult;

    public ValueProvider(PipelineResult pipelineResult)
    {
        this.pipelineResult = pipelineResult;
    }

    private void SetValue(CliSymbol symbol, object? value, bool found)
    {
        cachedValues[symbol] = (found, value);
    }

    private bool TryGetFromCache<T>(CliSymbol symbol, out T? value)
    {
        if (cachedValues.TryGetValue(symbol, out var t))
        {
            if (t.found)
            {
                value = (T?)t.value;
                return true;
            }
        }
        value = default;
        return false;
    }

    public T? GetValue<T>(CliValueSymbol valueSymbol)
    {
        if (TryGetValueInternal<T>(valueSymbol, out var value))
        {
            return value;
        }
        return default;
    }

    public bool TryGetValue<T>(CliValueSymbol valueSymbol, out T? value)
        => TryGetValueInternal<T>(valueSymbol, out value);

    private bool TryGetValueInternal<T>(CliValueSymbol valueSymbol, out T? value)
    {
        // TODO: Add guard to prevent reentrancy for the same symbol via RelativeToSymbol of custom ValueSource
        var _ = valueSymbol ?? throw new ArgumentNullException(nameof(valueSymbol));

        if (TryGetFromCache(valueSymbol, out value))
        {
            return true;
        }
        if (valueSymbol is CalculatedValue calculatedValueSymbol
            && calculatedValueSymbol.TryGetValue(pipelineResult, out value))
        {
            return CacheAndReturnSuccess(valueSymbol, value, true);
        }
        if (valueSymbol is not CalculatedValue
            && pipelineResult.ParseResult?.GetValueResult(valueSymbol) is { } valueResult)
        {
            value = valueResult.GetValue<T>();
            return CacheAndReturnSuccess(valueSymbol, value, true);
        }
        if (valueSymbol.TryGetDefault(out ValueSource? defaultValueSource))
        {
            if (defaultValueSource is not ValueSource<T> typedDefaultValueSource)
            {
                throw new InvalidOperationException("Unexpected ValueSource type for default value.");
            }
            if (typedDefaultValueSource.TryGetTypedValue(pipelineResult, out value))
            {
                return CacheAndReturnSuccess(valueSymbol, value, true);
            }
        }
        // TODO: Determine if we should cache default. If so, additional design is needed to avoid first hit returning false, and remainder returning true
        return CacheAndReturnSuccess(valueSymbol, default, false);

        bool CacheAndReturnSuccess(CliValueSymbol valueSymbol, T? valueToCache, bool valueFound)
        {
            SetValue(valueSymbol, valueToCache, valueFound);
            return valueFound;
        }
    }
}
