// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.CommandLine.Subsystems.Annotations;
using System.CommandLine.Subsystems;

namespace System.CommandLine;

internal class ValueProvider
{
    private Dictionary<CliSymbol, object?> cachedValues = [];
    private readonly ParseResult? parseResult = null;

    // Do not use primary constructor as this really truly cannot be changed
    internal ValueProvider(ParseResult parseResult)
    {
        this.parseResult = parseResult;
    }

    private void SetValue(CliSymbol symbol, object? value)
    {
        cachedValues[symbol] = value;
    }

    private bool TryGetValue(CliSymbol symbol, out object? value)
    {
        if (cachedValues.TryGetValue(symbol, out var objectValue))
        {
            value = objectValue;
            return true;
        }
        value = default;
        return false;
    }

    // These throw. Consider TryGet variations
    public T? GetValue<T>(CliDataSymbol dataSymbol)
        => (T?)(GetValueInternal(dataSymbol) ?? default(T));

    public object? GetValue(CliDataSymbol option)
        => GetValueInternal(option);

    private object? GetValueInternal(CliSymbol? symbol)
    {
        // NOTE: We use the subsystem's TryGetAnnotation here instead of the GetDefaultValue etc
        // extension methods, as the subsystem's TryGetAnnotation respects its annotation provider
        return symbol switch
        {
            not null when TryGetValue(symbol, out var value)
                => value, // It has already been retrieved at least once

            CliArgument argument when parseResult?.GetValueResult(argument) is { } valueResult  // GetValue not used because it  would always return a value
                => UseValue(symbol, valueResult.GetValue()), // Value was supplied during parsing, 
            CliOption option when parseResult?.GetValueResult(option) is { } valueResult  // GetValue not used because it would always return a value
                => UseValue(symbol, valueResult.GetValue()), // Value was supplied during parsing

            // Value was not supplied during parsing, determine default now
            // configuration values go here in precedence
            //not null when GetDefaultFromEnvironmentVariable<T>(symbol, out var envName)
            //    => UseValue(symbol, GetEnvByName(envName)),
            not null when symbol.TryGetAnnotation(ValueAnnotations.DefaultValueCalculation, out var defaultValueCalculation)
                => UseValue(symbol, CalculatedDefault(symbol, (Func<object?>)defaultValueCalculation)),
            not null when symbol.TryGetAnnotation(ValueAnnotations.DefaultValue, out var explicitValue)
                => UseValue(symbol, explicitValue),
            null => throw new ArgumentNullException(nameof(symbol)),
            _ => UseValue(symbol, default)
        };

        object? UseValue(CliSymbol symbol, object? value)
        {
            SetValue(symbol, value);
            return value;
        }
    }

    private static object? CalculatedDefault(CliSymbol symbol, Func<object?> defaultValueCalculation)
    {
        var objectValue = defaultValueCalculation();
        var value = objectValue;
        return value;
    }
}
