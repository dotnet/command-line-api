// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace System.CommandLine.Invocation;

internal sealed class AnonymousSynchronousCommandLineAction : SynchronousCommandLineAction
{
    private readonly Func<ParseResult, int> _syncAction;

    internal AnonymousSynchronousCommandLineAction(Func<ParseResult, int> action)
        => _syncAction = action;

    /// <inheritdoc />
    public override int Invoke(ParseResult parseResult) =>
        _syncAction(parseResult);
}