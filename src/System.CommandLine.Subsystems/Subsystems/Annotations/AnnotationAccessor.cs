// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Diagnostics.CodeAnalysis;

namespace System.CommandLine.Subsystems.Annotations;

/// <summary>
/// Allows associating an annotation with a <see cref="CliSymbol"/>. The annotation will be stored by the accessor's owner <see cref="CliSubsystem"/>.
/// </summary>
/// <remarks>
/// The annotation will be stored by the accessor's owner <see cref="CliSubsystem"/>. 
/// </summary>
/// <typeparam name="TValue">The type of value to be stored</typeparam>
/// <param name="owner">The subsystem that this annotation store data for.</param>
/// <param name="id">The identifier for this annotation, since subsystems may have multiple annotations.</param>
/// <param name="defaultValue">The default value to return if the annotation is not set.</param>
public struct AnnotationAccessor<TValue>(CliSubsystem owner, AnnotationId<TValue> id, TValue? defaultValue = default)
{
    /// <summary>
    /// The identifier for this annotation, since subsystems may have multiple annotations.
    /// </summary>
    public AnnotationId<TValue> Id { get; } = id;

    /// <summary>
    /// Store a value for the annotation and symbol
    /// </summary>
    /// <param name="symbol">The CliSymbol the value is for.</param>
    /// <param name="value">The value to store.</param>
    public readonly void Set(CliSymbol symbol, TValue value) => owner.SetAnnotation(symbol, Id, value);

    /// <summary>
    /// Retrieve the value for the annotation and symbol
    /// </summary>
    /// <param name="symbol">The CliSymbol the value is for.</param>
    /// <param name="value">The value to retrieve/</param>
    /// <returns>True if the value was found, false otherwise.</returns>
    public readonly bool TryGet(CliSymbol symbol, [NotNullWhen(true)] out TValue? value) => owner.TryGetAnnotation(symbol, Id, out value);

    /// <summary>
    /// Retrieve the value for the annotation and symbol
    /// </summary>
    /// <param name="symbol">The CliSymbol the value is for.</param>
    /// <returns>The retrieved value or <see cref="defaultValue"/> if the value was not found.</returns>
    public readonly TValue? Get(CliSymbol symbol)
    {
        if (TryGet(symbol, out var value))
        {
            return value;
        }
        return defaultValue;
    }
}
