// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace System.CommandLine.Subsystems.Annotations;

/// <summary>
/// IDs for well-known Version annotations.
/// </summary>
public static class ValueConditionAnnotations
{
    // TODO: @mhutch What do you want the prefix to be for AnnotationIds that are not bound to a subsystem?
    internal static string Prefix { get; } = "";

    /// <summary>
    /// Value conditions for a symbol
    /// </summary>
    public static AnnotationId ValueConditions { get; } = new(Prefix, nameof(ValueConditions));
}
