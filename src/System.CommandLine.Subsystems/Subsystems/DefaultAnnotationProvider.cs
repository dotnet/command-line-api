// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Diagnostics.CodeAnalysis;

namespace System.CommandLine.Subsystems;

/// <summary>
/// Default storage for annotations
/// </summary>
class DefaultAnnotationProvider : IAnnotationProvider
{
    record struct AnnotationKey(CliSymbol symbol, string annotationId);

    readonly Dictionary<AnnotationKey, object> annotations = [];

    public bool TryGet<TValue>(CliSymbol symbol, AnnotationId<TValue> id, [NotNullWhen(true)] out TValue? value)
    {
        if (annotations.TryGetValue(new AnnotationKey(symbol, id.Id), out var obj))
        {
            value = (TValue)obj;
            return true;
        }

        value = default;
        return false;
    }

    public void Set<TValue>(CliSymbol symbol, AnnotationId<TValue> id, TValue value)
    {
        if (value is not null)
        {
            annotations[new AnnotationKey(symbol, id.Id)] = value;
        }
        else
        {
            annotations.Remove(new AnnotationKey(symbol, id.Id));
        }
    }
}
