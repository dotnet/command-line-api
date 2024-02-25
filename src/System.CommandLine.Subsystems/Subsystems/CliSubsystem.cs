// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.CommandLine.Subsystems.Annotations;
using System.Diagnostics.CodeAnalysis;

namespace System.CommandLine.Subsystems;

/// <summary>
/// Base class for CLI subsystems. Implements storage of annotations.
/// </summary>
/// <param name="annotationProvider"></param>
public abstract class CliSubsystem(IAnnotationProvider? annotationProvider)
{
    DefaultAnnotationProvider? _defaultProvider;
    readonly IAnnotationProvider? _annotationProvider = annotationProvider;

    protected internal bool TryGetAnnotation<TValue>(CliSymbol symbol, AnnotationId<TValue> id, [NotNullWhen(true)] out TValue? value)
    {
        if (_defaultProvider is not null && _defaultProvider.TryGet(symbol, id, out value))
        {
            return true;
        }
        if (_annotationProvider is not null && _annotationProvider.TryGet(symbol, id, out value))
        {
            return true;
        }
        value = default;
        return false;
    }

    protected internal void SetAnnotation<T>(CliSymbol symbol, AnnotationId<T> id, T value)
    {
        (_defaultProvider ??= new DefaultAnnotationProvider ()).Set(symbol, id, value);
    }
}
