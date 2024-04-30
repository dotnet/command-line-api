// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Diagnostics.CodeAnalysis;

namespace System.CommandLine.Subsystems.Annotations;

/// <summary>
/// Allows associating an annotation with a <see cref="CliSymbol"/>. The annotation will be stored by the accessor's owner <see cref="CliSubsystem"/>.
/// </summary>
public struct AnnotationAccessor<TValue>(CliSubsystem owner, AnnotationId<TValue> id, TValue? defaultValue = default)
{
    /// <summary>
    /// The ID of the annotation
    /// </summary>
    public AnnotationId<TValue> Id { get; }
    public readonly void Set(CliSymbol symbol, TValue value) => owner.SetAnnotation(symbol, id, value);
    public readonly bool TryGet(CliSymbol symbol, [NotNullWhen(true)] out TValue? value) => owner.TryGetAnnotation(symbol, id, out value);
    public readonly TValue? Get(CliSymbol symbol)
    {
        if (TryGet(symbol, out var value))
        {
            return value ?? defaultValue;
        }
        return defaultValue;
    }
}
