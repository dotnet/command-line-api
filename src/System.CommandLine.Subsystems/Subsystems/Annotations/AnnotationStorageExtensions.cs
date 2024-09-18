// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

namespace System.CommandLine.Subsystems.Annotations;

/// <summary>
/// Handles storage of annotations associated with <see cref="CliSymbol"/> instances.
/// </summary>
public static partial class AnnotationStorageExtensions
{
    // CliSymbol does not offer any PropertyBag-like storage of arbitrary annotations, so the only way to allow setting
    // subsystem-specific annotations on CliSymbol instances (such as help description, default value, etc) via simple
    // extension methods is to use a static field with a dictionary that associates annotations with CliSymbol instances.
    //
    // Using ConditionalWeakTable for this dictionary ensures that the symbols and annotations can be collected when the
    // symbols are no longer reachable. Although this is unlikely to happen in a CLI app, it is important not to create
    // unexpected, unfixable, unbounded memory leaks in apps that construct multiple grammars/pipelines.
    //
    // The main use case for System.CommandLine is for a CLI app to construct a single annotated grammar in its entry point,
    // construct a pipeline using that grammar, and use the pipeline/grammar only once to parse its arguments. However, it
    // is important to have well defined and reasonable threading behavior so that System.CommandLine does not behave in
    // surprising ways when used in more advanced cases:
    //
    // * There may be multiple threads constructing and using completely independent grammars/pipelines. This happens in
    //   our own unit tests, but might happen e.g. in a multithreaded data processing app or web service that uses
    //   System.CommandLine to process inputs.
    //
    // * The grammar/pipeline are reentrant; they do not store they do not store internal state, and may be used to parse
    //   input multiple times. As this is the case, it is reasonable to expect a grammar/pipeline instance to be
    //   constructed in one thread then used in multiple threads. This might be done by the aforementioned web service or
    //   data processing app.
    //
    // The thread-safe behavior of ConditionalWeakTable ensures this works as expected without us having to worry about
    // taking locks directly, even though the instance is on a static field and shared between all threads. Note that
    // thread local storage is not useful for this, as that would create unexpected behaviors where a grammar constructed
    // in one thread would be missing its annotations when used in another thread.
    //
    // However, while getting values from ConditionalWeakTable is lock free, setting values internally uses an expensive
    // lock, so it is not ideal to store all individual annotations directly in the ConditionalWeakTable. This is especially
    // true as we do not want the common case of the CLI app entrypoint to have its performance impacted by multithreading
    // support more than absolutely necessary.
    //
    // Instead, we have a single static ConditionalWeakTable that maps each CliSymbol to an AnnotationStorage dictionary,
    // which is lazily created and added to the ConditionalWeakTable a single time for each CliSymbol. The individual
    // annotations are stored in the AnnotationStorage dictionary, which uses no locks, so is fast, but is not safe to be
    // modified from multiple threads.
    //
    // This is fine, as we will have the following well-defined threading behavior: an annotated grammar and pipeline may
    // only be constructed/modified from a single thread. Once the grammar/pipeline instance is fully constructed, it may
    // be safely used from multiple threads, but is not safe to further modify once in use.

    static readonly ConditionalWeakTable<CliSymbol, AnnotationStorage> symbolToAnnotationStorage = new();

    /// <summary>
    /// Sets the value for the annotation <paramref name="id"/> associated with the <paramref name="symbol"/> in the internal annotation storage.
    /// </summary>
    /// <param name="symbol">The symbol that is annotated</param>
    /// <param name="id">
    /// The identifier for the annotation. For example, the annotation identifier for the help description is <see cref="HelpAnnotations.Description">.
    /// </param>
    /// <param name="value">The annotation value</param>
    public static void SetAnnotation(this CliSymbol symbol, AnnotationId annotationId, object? value)
    {
        var storage = symbolToAnnotationStorage.GetValue(symbol, static (CliSymbol _) => new AnnotationStorage());
        storage.Set(symbol, annotationId, value);
    }

    /// <summary>
    /// Sets the value for the annotation <paramref name="id"/> associated with the <paramref name="symbol"/> in the internal annotation storage,
    /// and returns the <paramref name="symbol"> to enable fluent construction of symbols with annotations.
    /// </summary>
    /// <param name="symbol">The symbol that is annotated</param>
    /// <param name="id">
    /// The identifier for the annotation. For example, the annotation identifier for the help description is <see cref="HelpAnnotations.Description">.
    /// </param>
    /// <param name="value">The annotation value</param>
    /// <returns>
    /// The <paramref name="symbol">, to enable fluent construction of symbols with annotations.
    /// </returns>
    public static TSymbol WithAnnotation<TSymbol>(this TSymbol symbol, AnnotationId annotationId, object? value) where TSymbol : CliSymbol
    {
        symbol.SetAnnotation(annotationId, value);
        return symbol;
    }

    /// <summary>
    /// Attempts to get the value for the annotation <paramref name="annotationId"/> associated with the <paramref name="symbol"/> in the internal annotation
    /// storage used to store values set via <see cref="SetAnnotation(CliSymbol, AnnotationId, object?)"/>.
    /// </summary>
    /// <typeparam name="TValue">
    /// The expected type of the annotation value. If the type does not match, a <see cref="AnnotationTypeException"/> will be thrown.
    /// If the annotation allows multiple types for its values, and a type parameter cannot be determined statically,
    /// use <see cref="TryGetAnnotation(CliSymbol, AnnotationId, out object?)"/> to access the annotation value without checking its type.
    /// </typeparam>
    /// <param name="symbol">The symbol that is annotated</param>
    /// <param name="annotationId">
    /// The identifier for the annotation. For example, the annotation identifier for the help description is <see cref="HelpAnnotations.Description">.
    /// </param>
    /// <param name="value">The annotation value, if successful, otherwise <c>default</c></param>
    /// <returns>True if successful</returns>
    /// <remarks>
    /// If the annotation value does not have a single expected type for this symbol, use the <see cref="TryGetAnnotation(CliSymbol, AnnotationId, out object?)"/> overload instead.
    /// <para>
    /// This is intended to be called by the implementation of specialized ID-specific accessors for CLI authors such as <see cref="HelpAnnotationExtensions.GetDescription{TSymbol}(TSymbol)"/>.
    /// </para>
    /// <para>
    /// Subsystems should not call this directly, as it does not account for values from the pipeline's <see cref="IAnnotationProvider"/>.
    /// They should instead access annotations from the see cref="Pipeline.Annotations"/> property using
    /// <see cref="AnnotationResolver.TryGet{TValue}(CliSymbol, AnnotationId, out TValue?)"/> or an ID-specific
    /// extension method such as <see cref="HelpAnnotationExtensions.GetDescription{TSymbol}(TSymbol)" />.
    /// </para>
    /// </remarks>
    public static bool TryGetAnnotation<TValue>(this CliSymbol symbol, AnnotationId annotationId, [NotNullWhen(true)] out TValue? value)
    {
        if (TryGetAnnotation(symbol, annotationId, out object? rawValue))
        {
            if (rawValue is TValue expectedTypeValue)
            {
                value = expectedTypeValue;
                return true;
            }
            throw new AnnotationTypeException(annotationId, typeof(TValue), rawValue?.GetType());
        }

        value = default;
        return false;
    }

    /// <summary>
    /// Attempts to get the value for the annotation <paramref name="annotationId"/> associated with the
    /// <paramref name="symbol"/> in the internal annotation storage used to store values set via
    /// <see cref="SetAnnotation(CliSymbol, AnnotationId, object?)"/>.
    /// </summary>
    /// <param name="symbol">The symbol that is annotated</param>
    /// <param name="annotationId">
    /// The identifier for the annotation. For example, the annotation identifier for the help
    /// description is <see cref="HelpAnnotations.Description">.
    /// </param>
    /// <param name="value">The annotation value, if successful, otherwise <c>default</c></param>
    /// <returns>True if successful</returns>
    /// <remarks>
    /// If the expected type of the annotation value is known, use the
    /// <see cref="TryGetAnnotation{TValue}(CliSymbol, AnnotationId, out TValue?)"/> overload instead.
    /// <para>
    /// This is intended to be called by the implementation of specialized ID-specific accessors for
    /// CLI authors such as <see cref="HelpAnnotationExtensions.GetDescription{TSymbol}(TSymbol)"/>.
    /// </para>
    /// <para>
    /// Subsystems should not call this directly, as it does not account for values from the pipeline's <see cref="IAnnotationProvider"/>.
    /// They should instead access annotations from the see cref="Pipeline.Annotations"/> property using
    /// <see cref="AnnotationResolver.TryGet{TValue}(CliSymbol, AnnotationId, out TValue?)"/> or an ID-specific
    /// extension method such as <see cref="HelpAnnotationExtensions.GetDescription{TSymbol}(TSymbol)" />.
    /// </para>
    /// </remarks>
    public static bool TryGetAnnotation(this CliSymbol symbol, AnnotationId annotationId, [NotNullWhen(true)] out object? value)
    {
        if (symbolToAnnotationStorage.TryGetValue(symbol, out var storage) && storage.TryGet(symbol, annotationId, out value))
        {
            return true;
        }

        value = default;
        return false;
    }

    /// <summary>
    /// Attempts to get the value for the annotation <paramref name="annotationId"/> associated with the
    /// <paramref name="symbol"/> in the internal annotation storage used to store values set via
    /// <see cref="SetAnnotation{TValue}(CliSymbol, AnnotationId, TValue)"/>.
    /// </summary>
    /// <typeparam name="TValue">The type of the annotation value</typeparam>
    /// <param name="symbol">The symbol that is annotated</param>
    /// <param name="annotationId">
    /// The identifier for the annotation. For example, the annotation identifier for the help description
    /// is <see cref="HelpAnnotations.Description">.
    /// </param>
    /// <returns>The annotation value, if successful, otherwise <c>default</c></returns>
    /// <remarks>
    /// Subsystems should not call this directly, as it does not account for values from the pipeline's <see cref="IAnnotationProvider"/>.
    /// They should instead access annotations from the see cref="Pipeline.Annotations"/> property using
    /// <see cref="AnnotationResolver.TryGet{TValue}(CliSymbol, AnnotationId, out TValue?)"/> or an ID-specific
    /// extension method such as <see cref="HelpAnnotationExtensions.GetDescription{TSymbol}(TSymbol)" />.
    /// </remarks>
    public static TValue? GetAnnotationOrDefault<TValue>(this CliSymbol symbol, AnnotationId annotationId)
    {
        if (symbol.TryGetAnnotation(annotationId, out TValue? value))
        {
            return value;
        }

        return default;
    }
}
