// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.CommandLine.Invocation;

namespace System.CommandLine;

/// <summary>
/// Provides methods for creating and working with command handlers.
/// </summary>
public static partial class Handler
{
    /// <summary>
    /// Sets a command's handler based on an <see cref="Action{InvocationContext}"/>.
    /// </summary>
    public static void SetHandler(
        this Command command,
        Action<InvocationContext> handle) =>
        command.Handler = new AnonymousCommandHandler(handle);
}