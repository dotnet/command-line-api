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

    public static AnnotationId<object?> DefaultValue { get; } = new(Prefix, nameof(DefaultValue));
    public static AnnotationId<Func<object?>?> DefaultValueCalculation { get; } = new(Prefix, nameof(DefaultValueCalculation));
    public static AnnotationId<object?> Value { get; } = new(Prefix, nameof(Value));
}
