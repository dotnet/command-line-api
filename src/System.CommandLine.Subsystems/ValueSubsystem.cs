// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.CommandLine.Parsing;
using System.CommandLine.Subsystems;
using System.CommandLine.Subsystems.Annotations;

namespace System.CommandLine;

public class ValueSubsystem : CliSubsystem
{
    // @mhutch: Is the TryGet on the sparse dictionaries how we should handle a case where the annotations will be sparse to support lazy? If so, should we have another method on
    // the annotation wrapper, or an alternative struct when there a TryGet makes sense? This API needs review, maybe next Tuesday.
    //private PipelineContext? pipelineContext = null;
    private Dictionary<CliSymbol, object?> cachedValues = new();
    private ParseResult? parseResult = null;

    public ValueSubsystem(IAnnotationProvider? annotationProvider = null)
        : base(ValueAnnotations.Prefix, SubsystemKind.Version, annotationProvider)
    { }

    //internal void SetDefaultValue(CliSymbol symbol, object? defaultValue)
    //    => SetAnnotation(symbol, ValueAnnotations.DefaultValue, defaultValue);
    //internal object? GetDefaultValue(CliSymbol symbol)
    //  => TryGetAnnotation(symbol, ValueAnnotations.DefaultValue, out var defaultValue)
    //            ? defaultValue
    //            : "";
    private bool TryGetDefaultValue<T>(CliSymbol symbol, out T? defaultValue)
    {
        if (TryGetAnnotation(symbol, ValueAnnotations.DefaultValue, out var objectValue))
        {
            defaultValue = (T)objectValue;
            return true;
        }
        defaultValue = default;
        return false;
    }
    public AnnotationAccessor<object?> DefaultValue
      => new(this, ValueAnnotations.DefaultValue);

    internal void SetDefaultValueCalculation(CliSymbol symbol, Func<object?> factory)
        => SetAnnotation(symbol, ValueAnnotations.DefaultValueCalculation, factory);
    internal Func<object?>? GetDefaultValueCalculation(CliSymbol symbol)
        => TryGetAnnotation<Func<object?>?>(symbol, ValueAnnotations.DefaultValueCalculation, out var value)
                    ? value
                    : null;
    public AnnotationAccessor<Func<object?>?> DefaultValueCalculation
      => new(this, ValueAnnotations.DefaultValueCalculation);

    protected internal override bool GetIsActivated(ParseResult? parseResult)
    {
        this.parseResult = parseResult;
        return true;
    }

    protected internal override CliExit Execute(PipelineContext pipelineContext)
    {
        parseResult ??= pipelineContext.ParseResult;
        return base.Execute(pipelineContext);
    }

    // TODO: Do it! Consider using a simple dictionary instead of the annotation (@mhutch) because with is not useful here
    private void SetValue<T>(CliSymbol symbol, object? value)
        => cachedValues.Add(symbol, value);
    private bool TryGetValue<T>(CliSymbol symbol, out T? value)
    {
        if (cachedValues.TryGetValue(symbol, out var objectValue))
        {
            value = objectValue is null
                ? default
                :(T)objectValue;
            return true;
        }
        value = default;
        return false;
    }

    public T? GetValue<T>(CliOption option)
        => GetValueInternal<T>(option);
    public T? GetValue<T>(CliArgument argument)
    => GetValueInternal<T>(argument);

    private T? GetValueInternal<T>(CliSymbol? symbol)
        => symbol switch
        {
            not null when TryGetValue<T>(symbol, out var value)
                => value, // It has already been retrieved at least once
            CliArgument  argument when parseResult?.GetValueResult(argument) is { } valueResult  // GetValue not used because it  would always return a value
                => UseValue(symbol, valueResult.GetValue<T>()), // Value was supplied during parsing, 
            CliOption option when parseResult?.GetValueResult(option) is {} valueResult  // GetValue not used because it would always return a value
                => UseValue(symbol, valueResult.GetValue<T>()), // Value was supplied during parsing
            // Value was not supplied during parsing, determine default now
            not null when DefaultValueCalculation.TryGet(symbol, out var  defaultValueCalculation)
                => UseValue(symbol, CalculatedDefault<T>(symbol, defaultValueCalculation)),
            not null when TryGetDefaultValue<T>(symbol, out var explicitValue) 
                => UseValue(symbol, explicitValue),
            //not null when GetDefaultFromEnvironmentVariable<T>(symbol, out var envName)
            //    => UseValue(symbol, GetEnvByName(envName)),
            null => throw new ArgumentNullException(nameof(symbol)),
            _ => UseValue(symbol, default(T))
        };

    private static T? CalculatedDefault<T>(CliSymbol symbol, Func<object?> defaultValueCalculation)
    {
        var objectValue = defaultValueCalculation();
        var value = objectValue is null
            ? default
            : (T)objectValue;
        return value;
    }

    private T? UseValue<T>(CliSymbol symbol, T? value)
    {
        SetValue<T>(symbol, value);
        return value;
    }
}
