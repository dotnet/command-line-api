// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Diagnostics.CodeAnalysis;

namespace System.CommandLine.Subsystems.Annotations;

partial class AnnotationStorageExtensions
{
    class AnnotationStorage
    {
        record struct AnnotationKey(CliSymbol symbol, string prefix, string id)
        {
            public static AnnotationKey Create (CliSymbol symbol, AnnotationId annotationId)
                => new (symbol, annotationId.Prefix, annotationId.Id);
        }

        readonly Dictionary<AnnotationKey, object> annotations = [];

        public bool TryGet(CliSymbol symbol, AnnotationId annotationId, [NotNullWhen(true)] out object? value)
            => annotations.TryGetValue(AnnotationKey.Create(symbol, annotationId), out value);

        public void Set(CliSymbol symbol, AnnotationId annotationId, object? value)
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
