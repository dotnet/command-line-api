// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.CommandLine.Subsystems;
using System.CommandLine.Subsystems.Annotations;
using System.CommandLine.ValueSources;
using System.Diagnostics.CodeAnalysis;

namespace System.CommandLine;

public static class ValueAnnotationExtensions
{
    /// <summary>
    /// Get the default value annotation for the <paramref name="option"/>
    /// </summary>
    /// <typeparam name="TValue">The type of the option value</typeparam>
    /// <param name="option">The option</param>
    /// <returns>The option's default value annotation if any, otherwise <see langword="null"/></returns>
    /// <remarks>
    /// This is intended to be called by CLI authors. Subsystems should instead call <see cref="PipelineResult.GetValue{T}(CliOption)"/>,
    /// which calculates the actual default value, based on the default value annotation and default value calculation,
    /// whether directly stored on the symbol or from the subsystem's <see cref="IAnnotationProvider"/>.
    /// </remarks>
    public static bool TryGetDefault(this CliValueSymbol valueSymbol, out ValueSource? defaultValueSource)
        => valueSymbol.TryGetAnnotation(ValueAnnotations.DefaultValue, out defaultValueSource);

    /// <summary>
    /// Sets the default value annotation on the <paramref name="option"/>
    /// </summary>
    /// <typeparam name="TValue">The type of the option value</typeparam>
    /// <param name="option">The option</param>
    /// <param name="defaultValue">The default value for the option</param>
    public static void SetDefault<T>(this CliValueSymbol valueSymbol, ValueSource<T> defaultValue)
        => valueSymbol.SetAnnotation(ValueAnnotations.DefaultValue, defaultValue);

}
