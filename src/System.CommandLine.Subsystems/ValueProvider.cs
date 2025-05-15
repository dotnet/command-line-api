// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.CommandLine.Subsystems.Annotations;
using System.CommandLine.ValueSources;
using static System.Net.Mime.MediaTypeNames;

namespace System.CommandLine;

internal class ValueProvider
{
    private record struct CacheItem(object? Value, bool WasFound, bool IsCalculating)
    { }

    private Dictionary<CliSymbol, CacheItem> cachedValues = [];
    private PipelineResult pipelineResult;

    public ValueProvider(PipelineResult pipelineResult)
    {
        this.pipelineResult = pipelineResult;
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

        if (pipelineResult.ParseResult?.GetValueResult(valueSymbol) is { } valueResult)
        {
            value = valueResult.GetValue<T>();
            return CacheAndReturn(valueSymbol, value, true);
        }
        if (valueSymbol.TryGetAnnotation(ValueAnnotations.DefaultValue, out var defaultValueSource))
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
        value = default;
        return CacheAndReturn(valueSymbol, value, false);

        bool CacheAndReturn(CliValueSymbol valueSymbol, T? valueToCache, bool valueFound)
        {
            cachedValues[valueSymbol] = new CacheItem(valueToCache, valueFound, false);
            return valueFound;
        }
    }
}
