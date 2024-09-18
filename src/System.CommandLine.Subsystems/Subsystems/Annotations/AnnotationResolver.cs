// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Diagnostics.CodeAnalysis;

namespace System.CommandLine.Subsystems.Annotations;

/// <summary>
/// Provides a resolved annotation value for a given <see cref="CliSymbol"/>,
/// taking into account both the values stored directly on the symbol via extension methods
/// and any values from <see cref="IAnnotationProvider"/> providers.
/// </summary>
/// <remarks>
/// The <paramref name="providers"/> will be enumerated each time an annotation value is requested.
/// It may be modified after the resolver is created.
/// </remarks>
public class AnnotationResolver(ICollection<IAnnotationProvider> providers)
{
    private readonly IEnumerable<IAnnotationProvider> providers = providers ?? throw new ArgumentNullException(nameof(providers));

    /// <summary>
    /// Attempt to retrieve the <paramref name="symbol"/>'s value for the annotation <paramref name="id"/>. This will check any
    /// annotation providers that were passed to the constructor, and the internal per-symbol annotation storage.
    /// </summary>
    /// <typeparam name="TValue">
    /// The expected type of the annotation value. If the type does not match, a <see cref="AnnotationTypeException"/> will be thrown.
    /// If the annotation allows multiple types for its values, and a type parameter cannot be determined statically,
    /// use <see cref="TryGetAnnotation(CliSymbol, AnnotationId, out object?)"/> to access the annotation value without checking its type.
    /// </typeparam>
    /// <param name="symbol">The symbol the value is attached to</param>
    /// <param name="id">
    /// The identifier for the annotation value to be retrieved.
    /// For example, the annotation identifier for the help description is <see cref="HelpAnnotations.Description">.
    /// </param>
    /// <param name="value">An out parameter to contain the result</param>
    /// <returns>True if successful</returns>
    /// <remarks>
    /// This is intended for use by developers defining custom annotation IDs. Anyone defining an annotation
    /// ID should also define an accessor extension method on <see cref="AnnotationResolver"/> extension method
    /// on <see cref="CliSymbol"/> that subsystem authors can use to access the annotation value, such as
    /// <see cref="HelpAnnotationExtensions.GetDescription{TSymbol}(AnnotationResolver, TSymbol)"/> .
    /// <para>
    /// If the annotation value does not have a single expected type for this symbol, use the
    /// <see cref="TryGet(CliSymbol, AnnotationId, out object?)"/> overload instead.
    /// </para>
    /// </remarks>
    public bool TryGet<TValue>(CliSymbol symbol, AnnotationId annotationId, [NotNullWhen(true)] out TValue? value)
    {
        foreach (var provider in providers)
        {
            if (provider.TryGet(symbol, annotationId, out object? rawValue))
            {
                if (rawValue is TValue expectedTypeValue)
                {
                    value = expectedTypeValue;
                    return true;
                }
                throw new AnnotationTypeException(annotationId, typeof(TValue), rawValue?.GetType(), provider);
            }
        }

        return symbol.TryGetAnnotation(annotationId, out value);
    }

    /// <summary>
    /// Attempt to retrieve the <paramref name="symbol"/>'s value for the annotation <paramref name="id"/>. This will check any
    /// annotation providers that were passed to the constructor, and the internal per-symbol annotation storage.
    /// </summary>
    /// <param name="symbol">The symbol the value is attached to</param>
    /// <param name="id">
    /// The identifier for the annotation value to be retrieved.
    /// For example, the annotation identifier for the help description is <see cref="HelpAnnotations.Description">.
    /// </param>
    /// <param name="value">An out parameter to contain the result</param>
    /// <returns>True if successful</returns>
    /// <remarks>
    /// This is intended for use by developers defining custom annotation IDs. Anyone defining an annotation
    /// ID should also define an accessor extension method on <see cref="AnnotationResolver"/> extension method
    /// on <see cref="CliSymbol"/> that subsystem authors can use to access the annotation value, such as
    /// <see cref="HelpAnnotationExtensions.GetDescription{TSymbol}(AnnotationResolver, TSymbol)"/> .
    /// <para>
    /// If the expected type of the annotation value is known, use the
    /// <see cref="TryGet{TValue}(CliSymbol, AnnotationId, out TValue?)"/> overload instead.
    /// </para>
    /// </remarks>
    public bool TryGet(CliSymbol symbol, AnnotationId annotationId, [NotNullWhen(true)] out object? value)
    {
        foreach (var provider in providers)
        {
            if (provider.TryGet(symbol, annotationId, out value))
            {
                return true;
            }
        }

        return symbol.TryGetAnnotation(annotationId, out value);
    }

    /// <summary>
    /// Attempt to retrieve the <paramref name="symbol"/>'s value for the annotation <paramref name="id"/>. This will check any
    /// annotation providers that were passed to the constructor, and the internal per-symbol annotation storage. If the
    /// annotation value is not found, the default value for <typeparamref name="TValue"/> will be returned.
    /// </summary>
    /// <typeparam name="TValue">The type of the annotation value</typeparam>
    /// <param name="symbol">The symbol that is annotated</param>
    /// <param name="annotationId">
    /// The identifier for the annotation. For example, the annotation identifier for the help description
    /// is <see cref="HelpAnnotations.Description">.
    /// </param>
    /// <returns>The annotation value, if successful, otherwise <c>default</c></returns>
    /// <remarks>
    /// This is intended for use by developers defining custom annotation IDs. Anyone defining an annotation
    /// ID should also define an accessor extension method on <see cref="AnnotationResolver"/> extension method
    /// on <see cref="CliSymbol"/> that subsystem authors can use to access the annotation value, such as
    /// <see cref="HelpAnnotationExtensions.GetDescription{TSymbol}(AnnotationResolver, TSymbol)"/> .
    /// </remarks>
    public TValue? GetAnnotationOrDefault<TValue>(CliSymbol symbol, AnnotationId annotationId)
    {
        if (TryGet(symbol, annotationId, out TValue? value))
        {
            return value;
        }

        return default;
    }
}