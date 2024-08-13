﻿// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.CommandLine.Subsystems;
using System.CommandLine.Subsystems.Annotations;

namespace System.CommandLine;

public static class ValueAnnotationExtensions
{
    /// <summary>
    /// Sets the default value annotation on the <paramref name="option"/>
    /// </summary>
    /// <typeparam name="TValue">The type of the option value</typeparam>
    /// <param name="option">The option</param>
    /// <param name="defaultValue">The default value for the option</param>
    /// <returns>The <paramref name="option">, to enable fluent construction of symbols with annotations.</returns>
    public static CliOption<TValue> WithDefaultValue<TValue> (this CliOption<TValue> option, TValue defaultValue)
    {
        option.SetDefaultValue(defaultValue);
        return option;
    }

    /// <summary>
    /// Sets the default value annotation on the <paramref name="option"/>
    /// </summary>
    /// <typeparam name="TValue">The type of the option value</typeparam>
    /// <param name="option">The option</param>
    /// <param name="defaultValue">The default value for the option</param>
    public static void SetDefaultValue<TValue>(this CliOption<TValue> option, TValue defaultValue)
    {
        option.SetAnnotation(ValueAnnotations.DefaultValue, defaultValue);
    }

    /// <summary>
    /// Get the default value annotation for the <paramref name="option"/>
    /// </summary>
    /// <typeparam name="TValue">The type of the option value</typeparam>
    /// <param name="option">The option</param>
    /// <returns>The option's default value annotation if any, otherwise <see langword="null"/></returns>
    /// <remarks>
    /// This is intended to be called by CLI authors. Subsystems should instead call <see cref="ValueSubsystem.GetValue{T}(CliOption)"/>,
    /// which calculates the actual default value, based on the default value annotation and default value calculation,
    /// whether directly stored on the symbol or from the subsystem's <see cref="IAnnotationProvider"/>.
    /// </remarks>
    public static TValue? GetDefaultValueAnnotation<TValue>(this CliOption<TValue> option)
    {
        if (option.TryGetAnnotation(ValueAnnotations.DefaultValue, out TValue? defaultValue))
        {
            return defaultValue;
        }
        return default;
    }

    /// <summary>
    /// Sets the default value annotation on the <paramref name="argument"/>
    /// </summary>
    /// <typeparam name="TValue">The type of the argument value</typeparam>
    /// <param name="argument">The argument</param>
    /// <param name="defaultValue">The default value for the argument</param>
    /// <returns>The <paramref name="argument">, to enable fluent construction of symbols with annotations.</returns>
    public static CliArgument<TValue> WithDefaultValue<TValue>(this CliArgument<TValue> argument, TValue defaultValue)
    {
        argument.SetDefaultValue(defaultValue);
        return argument;
    }

    /// <summary>
    /// Sets the default value annotation on the <paramref name="argument"/>
    /// </summary>
    /// <typeparam name="TValue">The type of the argument value</typeparam>
    /// <param name="argument">The argument</param>
    /// <param name="defaultValue">The default value for the argument</param>
    /// <returns>The <paramref name="argument">, to enable fluent construction of symbols with annotations.</returns>
    public static void SetDefaultValue<TValue>(this CliArgument<TValue> argument, TValue defaultValue)
    {
        argument.SetAnnotation(ValueAnnotations.DefaultValue, defaultValue);
    }

    /// <summary>
    /// Get the default value annotation for the <paramref name="argument"/>
    /// </summary>
    /// <typeparam name="TValue">The type of the argument value</typeparam>
    /// <param name="argument">The argument</param>
    /// <returns>The argument's default value annotation if any, otherwise <see langword="null"/></returns>
    /// <remarks>
    /// This is intended to be called by CLI authors. Subsystems should instead call <see cref="ValueSubsystem.GetValue{T}(CliArgument)"/>,
    /// which calculates the actual default value, based on the default value annotation and default value calculation,
    /// whether directly stored on the symbol or from the subsystem's <see cref="IAnnotationProvider"/>.
    /// </remarks>
    public static TValue? GetDefaultValueAnnotation<TValue>(this CliArgument<TValue> argument)
    {
        if (argument.TryGetAnnotation(ValueAnnotations.DefaultValue, out TValue? defaultValue))
        {
            return (TValue?)defaultValue;
        }
        return default;
    }

    /// <summary>
    /// Sets the default value calculation for the <paramref name="option"/>
    /// </summary>
    /// <typeparam name="TValue">The type of the option value</typeparam>
    /// <param name="option">The option</param>
    /// <param name="defaultValueCalculation">The default value calculation for the option</param>
    /// <returns>The <paramref name="option">, to enable fluent construction of symbols with annotations.</returns>
    public static CliOption<TValue> WithDefaultValueCalculation<TValue>(this CliOption<TValue> option, Func<TValue?> defaultValueCalculation)
    {
        option.SetDefaultValueCalculation(defaultValueCalculation);
        return option;
    }

    /// <summary>
    /// Sets the default value calculation for the <paramref name="option"/>
    /// </summary>
    /// <typeparam name="TValue">The type of the option value</typeparam>
    /// <param name="option">The option</param>
    /// <param name="defaultValueCalculation">The default value calculation for the option</param>
    public static void SetDefaultValueCalculation<TValue>(this CliOption<TValue> option, Func<TValue?> defaultValueCalculation)
    {
        option.SetAnnotation(ValueAnnotations.DefaultValueCalculation, defaultValueCalculation);
    }

    /// <summary>
    /// Get the default value calculation for the <paramref name="option"/>
    /// </summary>
    /// <typeparam name="TValue">The type of the option value</typeparam>
    /// <param name="option">The option</param>
    /// <returns>The option's default value calculation if any, otherwise <see langword="null"/></returns>
    /// <remarks>
    /// This is intended to be called by CLI authors. Subsystems should instead call <see cref="ValueSubsystem.GetValue{T}(CliOption)"/>,
    /// which calculates the actual default value, based on the default value annotation and default value calculation,
    /// whether directly stored on the symbol or from the subsystem's <see cref="IAnnotationProvider"/>.
    /// </remarks>
    public static Func<TValue?>? GetDefaultValueCalculation<TValue>(this CliOption<TValue> option)
    {
        if (option.TryGetAnnotation(ValueAnnotations.DefaultValueCalculation, out Func<TValue?>? defaultValueCalculation))
        {
            return defaultValueCalculation;
        }
        return default;
    }

    /// <summary>
    /// Sets the default value calculation for the <paramref name="argument"/>
    /// </summary>
    /// <typeparam name="TValue">The type of the argument value</typeparam>
    /// <param name="argument">The argument</param>
    /// <param name="defaultValueCalculation">The default value calculation for the argument</param>
    /// <returns>The <paramref name="option">, to enable fluent construction of symbols with annotations.</returns>
    public static CliArgument<TValue> WithDefaultValueCalculation<TValue>(this CliArgument<TValue> argument, Func<TValue?> defaultValueCalculation)
    {
        argument.SetDefaultValueCalculation(defaultValueCalculation);
        return argument;
    }

    /// <summary>
    /// Sets the default value calculation for the <paramref name="argument"/>
    /// </summary>
    /// <typeparam name="TValue">The type of the argument value</typeparam>
    /// <param name="argument">The argument</param>
    /// <param name="defaultValueCalculation">The default value calculation for the argument</param>
    /// <returns>The <paramref name="option">, to enable fluent construction of symbols with annotations.</returns>
    public static void SetDefaultValueCalculation<TValue>(this CliArgument<TValue> argument, Func<TValue?> defaultValueCalculation)
    {
        argument.SetAnnotation(ValueAnnotations.DefaultValueCalculation, defaultValueCalculation);
    }

    /// <summary>
    /// Get the default value calculation for the <paramref name="argument"/>
    /// </summary>
    /// <typeparam name="TValue">The type of the argument value</typeparam>
    /// <param name="argument">The argument</param>
    /// <returns>The argument's default value calculation if any, otherwise <see langword="null"/></returns>
    /// <remarks>
    /// This is intended to be called by CLI authors. Subsystems should instead call <see cref="ValueSubsystem.GetValue{T}(CliArgument)"/>,
    /// which calculates the actual default value, based on the default value annotation and default value calculation,
    /// whether directly stored on the symbol or from the subsystem's <see cref="IAnnotationProvider"/>.
    /// </remarks>
    public static Func<TValue?>? GetDefaultValueCalculation<TValue>(this CliArgument<TValue> argument)
    {
        if (argument.TryGetAnnotation(ValueAnnotations.DefaultValueCalculation, out Func<TValue?>? defaultValueCalculation))
        {
            return defaultValueCalculation;
        }
        return default;
    }
}
