// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.CommandLine.ValueSources;

namespace System.CommandLine;

internal class ValueProvider
{
    private struct CacheItem
    {
        internal object? Value;
        internal bool WasFound;
        internal bool IsCalculating;
    }

    private Dictionary<CliSymbol, CacheItem> cachedValues = [];
    private PipelineResult pipelineResult;

    public ValueProvider(PipelineResult pipelineResult)
    {
        this.pipelineResult = pipelineResult;
    }

    private void SetCachedValue(CliSymbol symbol, object? value, bool found)
    {
        // TODO: MHutch: Feel free to optimize this and SetIsCalculating. We need the struct or a tuple here for "WasFound" which turns out we need.
        cachedValues[symbol] = new CacheItem()
        {
            Value = value,
            WasFound = found,
            IsCalculating = false
        };
    }

    private void SetIsCalculating(CliSymbol symbol)
    {
        cachedValues[symbol] = new CacheItem()
        {
            IsCalculating = true
        };
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
        var _ = valueSymbol ?? throw new ArgumentNullException(nameof(valueSymbol));

        if (cachedValues.TryGetValue(valueSymbol, out var cacheItem))
        {
            if (cacheItem.IsCalculating)
            {
                value = default;
                // Guard against reentrancy. Placed here so we catch if a CalculatedValue or calculation causes reentrancy
                throw new InvalidOperationException("Circular value source dependency.");
            }
            if (cacheItem.WasFound)
            {
                value = (T?)cacheItem.Value;
                return true;
            }
        }
        // !!! CRITICAL: All returns from this method should set the cache value to clear this pseudo-lock (use CacheAndReturn)
        SetIsCalculating(valueSymbol);

        if (valueSymbol is CalculatedValue calculatedValueSymbol
            && calculatedValueSymbol.TryGetValue(pipelineResult, out value))
        {
            return CacheAndReturn(valueSymbol, value, true);
        }
        if (valueSymbol is not CalculatedValue
            && pipelineResult.ParseResult?.GetValueResult(valueSymbol) is { } valueResult)
        {
            value = valueResult.GetValue<T>();
            return CacheAndReturn(valueSymbol, value, true);
        }
        if (valueSymbol.TryGetDefault(out ValueSource? defaultValueSource))
        {
            if (defaultValueSource is not ValueSource<T> typedDefaultValueSource)
            {
                throw new InvalidOperationException("Unexpected ValueSource type for default value.");
            }
            if (typedDefaultValueSource.TryGetTypedValue(pipelineResult, out value))
            {
                return CacheAndReturn(valueSymbol, value, true);
            }
        }
        // TODO: Determine if we should cache default. If so, additional design is needed to avoid first hit returning false, and remainder returning true
        value = default;
        return CacheAndReturn(valueSymbol, value, false);

        bool CacheAndReturn(CliValueSymbol valueSymbol, T? valueToCache, bool valueFound)
        {
            SetCachedValue(valueSymbol, valueToCache, valueFound);
            return valueFound;
        }
    }
}
