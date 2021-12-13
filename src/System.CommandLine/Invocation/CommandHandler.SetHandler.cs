// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace System.CommandLine.Invocation;

public static partial class CommandHandler
{
    public static ICommandHandler SetHandler<T1, T2, T3>(
        this Command command,
        object symbol1,
        object symbol2,
        Action<T1, T2> handle) =>
        command.Handler = new AnonymousCommandHandler(
            context => { });
}