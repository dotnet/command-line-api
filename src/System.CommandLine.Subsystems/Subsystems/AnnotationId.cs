// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace System.CommandLine.Subsystems;

/// <summary>
/// Describes the ID and type of an annotation.
/// </summary>
public record struct AnnotationId<TValue>(string Prefix, string Id)
{
    public override readonly string ToString() => $"{Prefix}.{Id}";
}
