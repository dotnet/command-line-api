// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace System.CommandLine.Subsystems.Annotations;

/// <summary>
/// IDs for well-known help annotations.
/// </summary>
public static class HelpAnnotations
{
    public static string Prefix { get; } = nameof(SubsystemKind.Help);

    public static AnnotationId<string> Description { get; } = new(Prefix, nameof(Description));
}
