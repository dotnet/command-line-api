// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.CommandLine.Subsystems.Annotations;
using System.Diagnostics.CodeAnalysis;

namespace System.CommandLine.Subsystems;

/// <summary>
/// Alternative storage of annotations, enabling lazy loading and dynamic annotations.
/// </summary>
public interface IAnnotationProvider
{
    bool TryGet(CliSymbol symbol, AnnotationId id, [NotNullWhen(true)] out object? value);
}
