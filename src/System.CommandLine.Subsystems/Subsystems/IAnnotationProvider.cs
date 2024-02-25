// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Diagnostics.CodeAnalysis;

namespace System.CommandLine.Subsystems;

/// <summary>
/// Alternative storage of annotations, enabling lazy loading and dynamic annotations.
/// </summary>
public interface IAnnotationProvider
{
    bool TryGet<TValue>(CliSymbol symbol, AnnotationId<TValue> id, [NotNullWhen(true)] out TValue? value);
}
