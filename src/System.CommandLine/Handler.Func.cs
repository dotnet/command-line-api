// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.CommandLine.Invocation;
using System.Threading;
using System.Threading.Tasks;

namespace System.CommandLine;

/// <summary>
/// Provides methods for creating and working with command handlers.
/// </summary>
public static partial class Handler
{
    /// <summary>
    /// Sets a command's handler based on a <see cref="Func{Task,InvocationContext}"/>.
    /// </summary>
    public static void SetHandler(
        this Command command,
        Func<InvocationContext, CancellationToken, Task> handle) =>
        command.Handler = new AnonymousCommandHandler(handle);
}