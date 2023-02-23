// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.CommandLine.IO;
using System.Threading;
using System.Threading.Tasks;

#pragma warning disable CS1591

namespace System.CommandLine.Invocation;

public abstract class CliAction : ICommandHandler
{
    private readonly ICommandHandler _innerHandler;
    private ParseResult? _parseResult;

    protected CliAction()
    {
    }

    internal CliAction(ICommandHandler innerHandler)
    {
        _innerHandler = innerHandler;
    }

    int ICommandHandler.Invoke(InvocationContext context) =>
        _innerHandler.Invoke(context);

    Task<int> ICommandHandler.InvokeAsync(InvocationContext context, CancellationToken cancellationToken) =>
        _innerHandler.InvokeAsync(context, cancellationToken);

    public async Task<int> RunAsync(
        IConsole? console = null,
        CancellationToken? cancellationToken = null)
    {
        var invocationContext = new InvocationContext(
            ParseResult,
            console ?? new SystemConsole());

        return await _innerHandler.InvokeAsync(
                   invocationContext,
                   cancellationToken ?? CancellationToken.None);
    }

    internal ParseResult ParseResult
    {
        get => _parseResult ?? ParseResult.Empty();
        set => _parseResult = value;
    }
}

internal class AnonymousCommandAction : CommandAction
{
    public AnonymousCommandAction(Action<InvocationContext> invoke) : base(new AnonymousCommandHandler(invoke))
    {
    }
}

public abstract class CommandAction : CliAction
{
    protected CommandAction()
    {
    }

    protected CommandAction(ICommandHandler innerHandler) : base(innerHandler)
    {
    }
}

public static class CommandExtensions
{
    public static void SetAction(this Command command, CommandAction handler)
    {
        command.Handler = handler;
    }

    public static void SetAction(this Command command, Action<InvocationContext> invoke)
    {
        command.SetAction(new AnonymousCommandAction(invoke));
    }
}