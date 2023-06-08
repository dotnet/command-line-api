// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Threading;
using System.Threading.Tasks;

namespace System.CommandLine.Invocation;

internal sealed class AnonymousAsynchronousCliAction : AsynchronousCliAction
{
    private readonly Func<ParseResult, CancellationToken, Task<int>> _asyncAction;

    internal AnonymousAsynchronousCliAction(Func<ParseResult, CancellationToken, Task<int>> action)
        => _asyncAction = action;

    /// <inheritdoc />
    public override Task<int> InvokeAsync(ParseResult parseResult, CancellationToken cancellationToken = default) =>
        _asyncAction(parseResult, cancellationToken);
}