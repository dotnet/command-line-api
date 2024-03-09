// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.CommandLine.Parsing;

namespace System.CommandLine.Subsystems.Annotations;

/// <summary>
/// IDs for well-known Version annotations.
/// </summary>
public static class ValueAnnotations
{
    public static string Prefix { get; } = nameof(SubsystemKind.Value);

    public static AnnotationId<object> Explicit { get; } = new(Prefix, nameof(Explicit));
    public static AnnotationId<Func<ValueResult, object?>> Calculated { get; } = new(Prefix, nameof(Calculated));
}
