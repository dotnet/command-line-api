// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Diagnostics.CodeAnalysis;

namespace System.CommandLine.Subsystems.Annotations;

partial class AnnotationStorageExtensions
{
    class AnnotationStorage : IAnnotationProvider
    {
        record struct AnnotationKey(CliSymbol symbol, string prefix, string id)
        {
            public static AnnotationKey Create<TAnnotation> (CliSymbol symbol, AnnotationId<TAnnotation> annotationId)
                => new (symbol, annotationId.Prefix, annotationId.Id);
        }

        readonly Dictionary<AnnotationKey, object> annotations = [];

        public bool TryGet<TValue>(CliSymbol symbol, AnnotationId<TValue> annotationId, [NotNullWhen(true)] out TValue? value)
        {
            if (annotations.TryGetValue(AnnotationKey.Create(symbol, annotationId), out var obj))
            {
                value = (TValue)obj;
                return true;
            }

            value = default;
            return false;
        }

        public void Set<TValue>(CliSymbol symbol, AnnotationId<TValue> annotationId, TValue value)
        {
            var key = AnnotationKey.Create(symbol, annotationId);
            if (value is not null)
            {
                annotations[key] = value;
            }
            else
            {
                annotations.Remove(key);
            }
        }
    }
}
