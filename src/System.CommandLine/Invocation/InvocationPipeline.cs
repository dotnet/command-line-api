// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace System.CommandLine.Invocation
{
    internal static class InvocationPipeline
    {
        internal static async Task<int> InvokeAsync(ParseResult parseResult, IConsole? console, CancellationToken cancellationToken)
        {
            if (parseResult.Action is null && parseResult.Configuration.Middleware.Count == 0)
            {
                return 0;
            }

            InvocationContext context = new (parseResult, console);
            ProcessTerminationHandler? terminationHandler = null;
            using CancellationTokenSource cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);

            try
            {
                Task<int> startedInvocation = parseResult.Action is not null && parseResult.Configuration.Middleware.Count == 0
                    ? parseResult.Action.InvokeAsync(context, cts.Token)
                    : InvokeHandlerWithMiddleware(context, cts.Token);

                if (parseResult.Configuration.ProcessTerminationTimeout.HasValue)
                    terminationHandler = new(cts, startedInvocation, parseResult.Configuration.ProcessTerminationTimeout.Value);

                if (terminationHandler is null)
                {
                    return await startedInvocation;
                }
                else
                {
                    // Handlers may not implement cancellation.
                    // In such cases, when CancelOnProcessTermination is configured and user presses Ctrl+C,
                    // ProcessTerminationCompletionSource completes first, with the result equal to native exit code for given signal.
                    Task<int> firstCompletedTask = await Task.WhenAny(startedInvocation, terminationHandler.ProcessTerminationCompletionSource.Task);
                    return await firstCompletedTask; // return the result or propagate the exception
                }
            }
            catch (Exception ex) when (parseResult.Configuration.ExceptionHandler is not null)
            {
                return parseResult.Configuration.ExceptionHandler(ex, context);
            }
            finally
            {
                terminationHandler?.Dispose();
            }

            static async Task<int> InvokeHandlerWithMiddleware(InvocationContext context, CancellationToken token)
            {
                int exitCode = 0;
                InvocationMiddleware invocationChain = BuildInvocationChain(context);
                await invocationChain(context, token, async (ctx, token) =>
                {
                    if (ctx.ParseResult.Action is { } handler)
                    {
                        exitCode = await handler.InvokeAsync(ctx, token);
                    }
                });

                return exitCode;
            }
        }

        internal static int Invoke(ParseResult parseResult, IConsole? console = null)
        {
            if (parseResult.Action is null && parseResult.Configuration.Middleware.Count == 0)
            {
                return 0;
            }

            InvocationContext context = new (parseResult, console);

            try
            {
                if (parseResult.Configuration.Middleware.Count == 0 && parseResult.Action is not null)
                {
                    return parseResult.Action.Invoke(context);
                }

                return InvokeHandlerWithMiddleware(context); // kept in a separate method to avoid JITting
            }
            catch (Exception ex) when (parseResult.Configuration.ExceptionHandler is not null)
            {
                return parseResult.Configuration.ExceptionHandler(ex, context);
            }

            static int InvokeHandlerWithMiddleware(InvocationContext context)
            {
                int exitCode = 0;
                InvocationMiddleware invocationChain = BuildInvocationChain(context);
                invocationChain(context, CancellationToken.None, (ctx, token) =>
                {
                    if (ctx.ParseResult.Action is { } handler)
                    {
                        exitCode = handler.Invoke(ctx);
                    }

                    return Task.CompletedTask;
                }).GetAwaiter().GetResult();
                return exitCode;
            }
        }

        private static InvocationMiddleware BuildInvocationChain(InvocationContext context)
        {
            return context.ParseResult.Configuration.Middleware.Aggregate(
                (first, second) =>
                    (ctx, token, next) =>
                        first(ctx, token,
                              (c, t) => second(c, t, next)));
        }
    }
}
