// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace System.CommandLine.Subsystems.Annotations;

/// <summary>
/// IDs for well-known Version annotations.
/// </summary>
public static class InvocationAnnotations
{
    internal static string Prefix { get; } = nameof(SubsystemKind.Invocation);

}
