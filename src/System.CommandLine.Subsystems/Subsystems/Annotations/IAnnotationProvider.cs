// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Diagnostics.CodeAnalysis;

namespace System.CommandLine.Subsystems.Annotations;

/// <summary>
/// Alternative storage of annotations, enabling lazy loading and dynamic annotations.
/// </summary>
public interface IAnnotationProvider
{
    /// <summary>
    /// Try to get the value of the annotation with the given <paramref name="id"/> for the <paramref name="symbol"/>.
    /// </summary>
    /// <param name="context">Additional context that may be used when resolving the annotation value.</param>
    /// <param name="symbol">The symbol</param>
    /// <param name="annotationId">The annotation identifier</param>
    /// <param name="value">The annotation value</param>
    /// <returns><see langword="true"> if the  symbol was resolved, otherwise <see langword="false"></returns>
    bool TryGet(CliSymbol symbol, AnnotationId annotationId, AnnotationResolveContext context, [NotNullWhen(true)] out object? value);
}
