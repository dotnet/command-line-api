// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.CommandLine.Subsystems.Annotations;
using System.CommandLine.ValueSources;
using System.Diagnostics.CodeAnalysis;

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
            if (defaultValueSource is not ValueSource<T> typedValueSource)
            {
                throw new InvalidOperationException("Unexpected ValueSource type");
            }
            (var success, var defaultValue) = typedValueSource.GetTypedValue(pipelineResult);
            if (success)
            {
                return UseValue(valueSymbol, defaultValue);
            }
        }
        return UseValue(valueSymbol, default(T));

        // TODO: The following logic is definitely WRONG. If there is a relative or env variable, it does not continue if it is not found
        // TODO: Replace this method with an AggregateValueSource
        // NOTE: We use the subsystem's TryGetAnnotation here instead of the GetDefaultValue etc
        // extension methods, as the subsystem's TryGetAnnotation respects its annotation provider
        //return valueSymbol switch
        //{
        //    { } when TryGetValue<T>(valueSymbol, out var value)
        //        => value, // It has already been retrieved at least once
        //    { } when parseResult?.GetValueResult(valueSymbol) is { } valueResult  // GetValue not used because it  would always return a value
        //        => UseValue(valueSymbol, valueResult.GetValue<T>()), // Value was supplied during parsing, 
        //    // Value was not supplied during parsing, determine default now
        //    // configuration values go here in precedence
        //    //not null when GetDefaultFromEnvironmentVariable<T>(symbol, out var envName)
        //    //    => UseValue(symbol, GetEnvByName(envName)),
        //    { } when valueSymbol.TryGetAnnotation(ValueAnnotations.DefaultValueCalculation, out Func<T?>? defaultValueCalculation)
        //        => UseValue(valueSymbol, CalculatedDefault<T>(valueSymbol, (Func<T?>)defaultValueCalculation)),
        //    { } when valueSymbol.TryGetAnnotation(ValueAnnotations.DefaultValue, out T? explicitValue)
        //        => UseValue(valueSymbol, explicitValue),
        //    null => throw new ArgumentNullException(nameof(valueSymbol)),
        //    _ => UseValue(valueSymbol, default(T))
        //};

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
