// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace System.CommandLine.Subsystems.Annotations;

/// <summary>
/// IDs for well-known Version annotations.
/// </summary>
public static class ValueAnnotations
{
    internal static string Prefix { get; } = nameof(SubsystemKind.Value);

    /// <summary>
    /// Provides Set and Get for default values
    /// </summary>
    public static AnnotationId<object?> DefaultValue { get; } = new(Prefix, nameof(DefaultValue));

    /// <summary>
    /// Provides Set and Get for default value calculations
    /// </summary>
    public static AnnotationId<Func<object?>> DefaultValueCalculation { get; } = new(Prefix, nameof(DefaultValueCalculation));
}
