// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace System.CommandLine.Invocation
{
    internal static class InvocationPipeline
    {
        internal static async Task<int> InvokeAsync(ParseResult parseResult, IConsole? console, CancellationToken cancellationToken)
        {
            InvocationContext context = new (parseResult, console);

            using CancellationTokenSource cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);

            Task<int> startedInvocation = parseResult.Handler is not null && context.Parser.Configuration.Middleware.Count == 0
                ? parseResult.Handler.InvokeAsync(context, cts.Token)
                : InvokeHandlerWithMiddleware(context, cts.Token);

            ProcessTerminationHandler? terminationHandler = parseResult.Parser.Configuration.ProcessTerminationTimeout.HasValue
                ? new (cts, startedInvocation, parseResult.Parser.Configuration.ProcessTerminationTimeout.Value)
                : null;

            try
            {
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
            catch (Exception ex) when (context.Parser.Configuration.ExceptionHandler is not null)
            {
                context.Parser.Configuration.ExceptionHandler(ex, context);
                return context.ExitCode;
            }
            finally
            {
                terminationHandler?.Dispose();
            }

            static async Task<int> InvokeHandlerWithMiddleware(InvocationContext context, CancellationToken token)
            {
                InvocationMiddleware invocationChain = BuildInvocationChain(context, true);

                await invocationChain(context, token, (_, _) => Task.CompletedTask);

                return GetExitCode(context);
            }
        }

        internal static int Invoke(ParseResult parseResult, IConsole? console = null)
        {
            InvocationContext context = new (parseResult, console);

            try
            {
                if (context.Parser.Configuration.Middleware.Count == 0 && parseResult.Handler is not null)
                {
                    return parseResult.Handler.Invoke(context);
                }

                return InvokeHandlerWithMiddleware(context); // kept in a separate method to avoid JITting
            }
            catch (Exception ex) when (context.Parser.Configuration.ExceptionHandler is not null)
            {
                context.Parser.Configuration.ExceptionHandler(ex, context);
                return context.ExitCode;
            }

            static int InvokeHandlerWithMiddleware(InvocationContext context)
            {
                InvocationMiddleware invocationChain = BuildInvocationChain(context, false);

                invocationChain(context, CancellationToken.None, static (_, _) => Task.CompletedTask).ConfigureAwait(false).GetAwaiter().GetResult();

                return GetExitCode(context);
            }
        }

        private static InvocationMiddleware BuildInvocationChain(InvocationContext context, bool invokeAsync)
        {
            var invocations = new List<InvocationMiddleware>(context.Parser.Configuration.Middleware.Count + 1);
            invocations.AddRange(context.Parser.Configuration.Middleware);

            invocations.Add(async (invocationContext, cancellationToken, _) =>
            {
                if (invocationContext.ParseResult.Handler is { } handler)
                {
                    context.ExitCode = invokeAsync
                                           ? await handler.InvokeAsync(invocationContext, cancellationToken)
                                           : handler.Invoke(invocationContext);
                }
            });

            return invocations.Aggregate(
                (first, second) =>
                    (ctx, token, next) =>
                        first(ctx, token,
                              (c, t) => second(c, t, next)));
        }

        private static int GetExitCode(InvocationContext context)
        {
            context.InvocationResult?.Invoke(context);

            return context.ExitCode;
        }
    }
}
