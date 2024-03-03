// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace System.CommandLine.Subsystems.Annotations;

/// <summary>
/// IDs for well-known Version annotations.
/// </summary>
public static class VersionAnnotations
{
    // I made this public because we need an identifier for subsystem Kind, and I think this makes a very good one.
    public static string Prefix { get; } = "Version."; // TODO: Decide between const and property. Isn't a const baked in at the callsite? Would that be OK when this is public?
    public static AnnotationId<string> Version { get; } = new(Prefix + nameof(Version));
}
