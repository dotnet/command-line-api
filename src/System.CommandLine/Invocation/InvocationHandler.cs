// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.CommandLine.IO;
using System.Threading;
using System.Threading.Tasks;

namespace System.CommandLine.Invocation;

public class CliAction : ICommandHandler
{
    private readonly ICommandHandler _innerHandler;
    private ParseResult? _parseResult;

    public CliAction(ICommandHandler innerHandler)
    {
        _innerHandler = innerHandler;
    }

    int ICommandHandler.Invoke(InvocationContext context) =>
        _innerHandler.Invoke(context);

    Task<int> ICommandHandler.InvokeAsync(InvocationContext context, CancellationToken cancellationToken = default) =>
        _innerHandler.InvokeAsync(context, cancellationToken);

    public static ICommandHandler Create(Action<InvocationContext> invoke)
    {
        return new CliAction(new AnonymousCommandHandler(invoke));
    }

    public async Task<int> RunAsync(IConsole? console = null, CancellationToken? cancellationToken = null)
    {
        var invocationContext = new InvocationContext(
            ParseResult,
            console ?? new SystemConsole());

        return await _innerHandler.InvokeAsync(invocationContext);
    }

    internal ParseResult ParseResult
    {
        get => _parseResult ?? ParseResult.Empty();
        set => _parseResult = value;
    }
}

public static class CommandExtensions
{
    public static void SetHandler(this Command command, ICommandHandler handler)
    {
        command.Handler = handler;
    }
}