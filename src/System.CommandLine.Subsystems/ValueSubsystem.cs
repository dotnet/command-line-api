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
    private PipelineContext? pipelineContext = null;

    public ValueSubsystem(IAnnotationProvider? annotationProvider = null)
        : base(ValueAnnotations.Prefix, SubsystemKind.Version, annotationProvider)
    { }

    internal void SetExplicitDefault(CliSymbol symbol, object? defaultValue)
        => SetAnnotation(symbol, ValueAnnotations.ExplicitDefault, defaultValue);
    internal object? GetExplicitDefault(CliSymbol symbol)
      => TryGetAnnotation(symbol, ValueAnnotations.ExplicitDefault, out var defaultValue)
                ? defaultValue
                : "";
    internal bool TryGetExplicitDefault<T>(CliSymbol symbol, out T? defaultValue)
    {
        if (TryGetAnnotation(symbol, ValueAnnotations.Value, out var objectValue))
        {
            defaultValue = (T)objectValue;
            return true;
        }
        defaultValue = default;
        return false;
    }
    public AnnotationAccessor<object?> ExplicitDefault
      => new(this, ValueAnnotations.ExplicitDefault);

    internal void SetDefaultCalculation(CliSymbol symbol, Func<object?> factory)
        => SetAnnotation(symbol, ValueAnnotations.DefaultCalculation, factory);
    internal Func<object?>? GetDefaultCalculation(CliSymbol symbol)
        => TryGetAnnotation<Func<object?>?>(symbol, ValueAnnotations.DefaultCalculation, out var value)
                    ? value
                    : null;
    public AnnotationAccessor<Func<object?>?> DefaultCalculation
      => new(this, ValueAnnotations.DefaultCalculation);

    private void SetValue(CliSymbol symbol, object? value)
        => SetAnnotation(symbol, ValueAnnotations.Value, value);
    // TODO: Consider putting the logic in the generic version here
    // TODO: Consider using a simple dictionary instead of the annotation (@mhutch)
    // TODO: GetValue should call TryGetValue, not another call to TryGetAnnotation.
    // TODO: Should we provide an untyped value? 
    private object? GetValue(CliSymbol symbol)
        => TryGetAnnotation<object?>(symbol, ValueAnnotations.Value, out var value)
                    ? value
                    : null;
    private bool TryGetValue<T>(CliSymbol symbol, out T? value)
    {
        if (TryGetAnnotation(symbol, ValueAnnotations.Value, out var objectValue))
        {
            value = (T)objectValue;
            return true;
        }
        value = default;
        return false;
    }
    // TODO: Is fluent style meaningful for Value?
    //public AnnotationAccessor<object?> Value
    //  => new(this, ValueAnnotations.Value);

    protected internal override bool GetIsActivated(ParseResult? parseResult)
        => true;

    protected internal override CliExit Execute(PipelineContext pipelineContext)
    {
        this.pipelineContext = pipelineContext;
        return CliExit.NotRun(pipelineContext.ParseResult);
    }

    // @mhutch: I find this more readable than the if conditional version below. There will be at least two more blocks. Look good?
    public T? GetValue<T>(CliSymbol symbol)
        => symbol switch
        {
            { } when TryGetValue<T>(symbol, out var value)
                => value, // It has already been retrieved at least once
            { } when pipelineContext?.ParseResult?.GetValueResult(symbol) is ValueResult valueResult
                => UseValue(symbol, valueResult.GetValue<T>()), // Value was supplied during parsing
            // Value was not supplied during parsing, determine default now
            { } when GetDefaultCalculation(symbol) is { } defaultValueCalculation
                => UseValue(symbol, CalculatedDefault<T>(symbol, defaultValueCalculation)),
            { } when TryGetExplicitDefault<T>(symbol, out var explicitValue) => UseValue(symbol, explicitValue),
            null => throw new ArgumentNullException(nameof(symbol)),
            _ => UseValue(symbol, default(T))
        };

    public T? GetValue2<T>(CliSymbol symbol)
    {
        if (TryGetValue<T>(symbol, out var value))
        {
            // It has already been retrieved at least once
            return value;
        }
        if (pipelineContext?.ParseResult?.GetValueResult(symbol) is ValueResult valueResult)
        {
            // Value was supplied during parsing
            return UseValue(symbol, valueResult.GetValue<T>());
        }
        // Value was not supplied during parsing, determine default now
        if (GetDefaultCalculation(symbol) is { } defaultValueCalculation)
        {
            return UseValue(symbol, CalculatedDefault(symbol, defaultValueCalculation));
        }
        if (TryGetExplicitDefault<T>(symbol, out var explicitValue))
        {
            return UseValue(symbol, value);
        }
        value = default;
        SetValue(symbol, value);
        return value;

        static T? CalculatedDefault(CliSymbol symbol, Func<object?> defaultValueCalculation)
        {
            var objectValue = defaultValueCalculation();
            var value = objectValue is null
                ? default
                : (T)objectValue;
            return value;
        }
    }


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
        SetValue(symbol, value);
        return value;
    }


}
