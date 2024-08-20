// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.CommandLine.Subsystems;
using System.CommandLine.Subsystems.Annotations;

namespace System.CommandLine;

internal class ValueProvider
{
    private Dictionary<CliSymbol, object?> cachedValues = [];
    private ParseResult? parseResult = null;

    public ValueProvider(ParseResult parseResult)
    {
        this.parseResult = parseResult;
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

    private T? GetValueInternal<T>(CliSymbol? symbol)
    {
        // NOTE: We use the subsystem's TryGetAnnotation here instead of the GetDefaultValue etc
        // extension methods, as the subsystem's TryGetAnnotation respects its annotation provider
        return symbol switch
        {
            not null when TryGetValue<T>(symbol, out var value)
                => value, // It has already been retrieved at least once
            CliArgument argument when parseResult?.GetValueResult(argument) is { } valueResult  // GetValue not used because it  would always return a value
                => UseValue(symbol, valueResult.GetValue<T>()), // Value was supplied during parsing, 
            CliOption option when parseResult?.GetValueResult(option) is { } valueResult  // GetValue not used because it would always return a value
                => UseValue(symbol, valueResult.GetValue<T>()), // Value was supplied during parsing
            // Value was not supplied during parsing, determine default now
            // configuration values go here in precedence
            //not null when GetDefaultFromEnvironmentVariable<T>(symbol, out var envName)
            //    => UseValue(symbol, GetEnvByName(envName)),
            not null when symbol.TryGetAnnotation(ValueAnnotations.DefaultValueCalculation, out Func<T?>? defaultValueCalculation)
                => UseValue(symbol, CalculatedDefault<T>(symbol, (Func<T?>)defaultValueCalculation)),
            not null when symbol.TryGetAnnotation(ValueAnnotations.DefaultValue, out T? explicitValue)
                => UseValue(symbol, explicitValue),
            null => throw new ArgumentNullException(nameof(symbol)),
            _ => UseValue(symbol, default(T))
        };

        TValue? UseValue<TValue>(CliSymbol symbol, TValue? value)
        {
            SetValue(symbol, value);
            return value;
        }
    }

    private static T? CalculatedDefault<T>(CliSymbol symbol, Func<T?> defaultValueCalculation)
    {
        var objectValue = defaultValueCalculation();
        var value = objectValue is null
            ? default
            : (T)objectValue;
        return value;
    }
}
