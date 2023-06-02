// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Threading;
using System.Threading.Tasks;

namespace System.CommandLine.Invocation;

// FIX: separate files

public abstract class CliAction
{
    private protected CliAction()
    {
    }

    // FIX: (CliAction) change this to accommodate the fact that the core action isn't "exclusive" either, and some actions might eventually be created that run after it.
    public bool Exclusive { get; protected set; } = true;
}

/// <summary>
/// Defines the behavior of a symbol.
/// </summary>
public abstract class SynchronousCliAction : CliAction
{
    /// <summary>
    /// Performs an action when the associated symbol is invoked on the command line.
    /// </summary>
    /// <param name="parseResult">Provides the parse results.</param>
    /// <returns>A value that can be used as the exit code for the process.</returns>
    public abstract int Invoke(ParseResult parseResult);
}

public abstract class AsynchronousCliAction : CliAction
{
    /// <summary>
    /// Performs an action when the associated symbol is invoked on the command line.
    /// </summary>
    /// <param name="parseResult">Provides the parse results.</param>
    /// <param name="cancellationToken">The token to monitor for cancellation requests.</param>
    /// <returns>A value that can be used as the exit code for the process.</returns>
    public abstract Task<int> InvokeAsync(ParseResult parseResult, CancellationToken cancellationToken = default);
}

public class SynchronousCliCommandAction : SynchronousCliAction
{
    private readonly Func<ParseResult, int> _syncAction;

    internal SynchronousCliCommandAction(Func<ParseResult, int> action)
        => _syncAction = action;

    public override int Invoke(ParseResult parseResult) =>
        _syncAction(parseResult);
}

public class AsynchronousCliCommandAction : AsynchronousCliAction
{
    private readonly Func<ParseResult, CancellationToken, Task<int>> _asyncAction;

    internal AsynchronousCliCommandAction(Func<ParseResult, CancellationToken, Task<int>> action)
        => _asyncAction = action;

    public override Task<int> InvokeAsync(ParseResult parseResult, CancellationToken cancellationToken) =>
        _asyncAction(parseResult, cancellationToken);
}