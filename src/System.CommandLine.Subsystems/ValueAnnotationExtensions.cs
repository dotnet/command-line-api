// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.CommandLine.Subsystems.Annotations;

namespace System.CommandLine;

public static class ValueAnnotationExtensions
{
    public static CliOption<TValue> WithDefaultValue<TValue> (this CliOption<TValue> option, TValue defaultValue)
    {
        option.SetDefaultValue(defaultValue);
        return option;
    }

    public static void SetDefaultValue<TValue>(this CliOption<TValue> option, TValue defaultValue)
    {
        option.SetAnnotation(ValueAnnotations.DefaultValue, defaultValue);
    }

    public static TValue? GetDefaultValue<TValue>(this CliOption<TValue> option)
    {
        if (option.TryGetAnnotation(ValueAnnotations.DefaultValue, out var defaultValue))
        {
            return (TValue?)defaultValue;
        }
        return default;
    }

    public static CliArgument<TValue> WithDefaultValue<TValue>(this CliArgument<TValue> argument, TValue defaultValue)
    {
        argument.SetDefaultValue(defaultValue);
        return argument;
    }

    public static void SetDefaultValue<TValue>(this CliArgument<TValue> argument, TValue defaultValue)
    {
        argument.SetAnnotation(ValueAnnotations.DefaultValue, defaultValue);
    }

    public static TValue? GetDefaultValue<TValue>(this CliArgument<TValue> argument)
    {
        if (argument.TryGetAnnotation(ValueAnnotations.DefaultValue, out var defaultValue))
        {
            return (TValue?)defaultValue;
        }
        return default;
    }

    public static CliOption<TValue> WithDefaultValueCalculation<TValue>(this CliOption<TValue> option, Func<TValue?> defaultValueCalculation)
    {
        option.SetDefaultValueCalculation(defaultValueCalculation);
        return option;
    }

    public static void SetDefaultValueCalculation<TValue>(this CliOption<TValue> option, Func<TValue?> defaultValueCalculation)
    {
        option.SetAnnotation(ValueAnnotations.DefaultValueCalculation, defaultValueCalculation);
    }

    public static Func<TValue?>? GetDefaultValueCalculation<TValue>(this CliOption<TValue> option)
    {
        if (option.TryGetAnnotation(ValueAnnotations.DefaultValueCalculation, out var defaultValueCalculation))
        {
            return (Func<TValue?>)defaultValueCalculation;
        }
        return default;
    }

    public static CliArgument<TValue> WithDefaultValueCalculation<TValue>(this CliArgument<TValue> argument, Func<TValue?> defaultValueCalculation)
    {
        argument.SetDefaultValueCalculation(defaultValueCalculation);
        return argument;
    }

    public static void SetDefaultValueCalculation<TValue>(this CliArgument<TValue> argument, Func<TValue?> defaultValueCalculation)
    {
        argument.SetAnnotation(ValueAnnotations.DefaultValueCalculation, defaultValueCalculation);
    }

    public static Func<TValue?>? GetDefaultValueCalculation<TValue>(this CliArgument<TValue> argument)
    {
        if (argument.TryGetAnnotation(ValueAnnotations.DefaultValueCalculation, out var defaultValueCalculation))
        {
            return (Func<TValue?>)defaultValueCalculation;
        }
        return default;
    }
}
