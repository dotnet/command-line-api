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
        // https://tldp.org/LDP/abs/html/exitcodes.html - 130 - script terminated by ctrl-c
        private const int SIGINT_EXIT_CODE = 130, SIGTERM_EXIT_CODE = 143;

        internal static async Task<int> InvokeAsync(ParseResult parseResult, IConsole? console, CancellationToken cancellationToken)
        {
            InvocationContext context = new (parseResult, console);
            TimeSpan? processTerminationTimeout = parseResult.Parser.Configuration.ProcessTerminationTimeout;
            TaskCompletionSource<int>? processTerminationCompletionSource = processTerminationTimeout.HasValue ? new () : null;

            using CancellationTokenSource cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);

            Task<int> startedInvocation = parseResult.Handler is not null && context.Parser.Configuration.Middleware.Count == 0
                ? parseResult.Handler.InvokeAsync(context, cts.Token)
                : InvokeHandlerWithMiddleware(context, cts.Token);

            if (processTerminationTimeout.HasValue)
            {
                Console.CancelKeyPress += OnCancelKeyPress;
                AppDomain.CurrentDomain.ProcessExit += OnProcessExit;
            }

            try
            {
                if (processTerminationCompletionSource is null)
                {
                    return await startedInvocation;
                }
                else
                {
                    return await (await Task.WhenAny(startedInvocation, processTerminationCompletionSource.Task));
                }
            }
            catch (Exception ex) when (context.Parser.Configuration.ExceptionHandler is not null)
            {
                context.Parser.Configuration.ExceptionHandler(ex, context);
                return context.ExitCode;
            }
            finally
            {
                if (processTerminationTimeout.HasValue)
                {
                    Console.CancelKeyPress -= OnCancelKeyPress;
                    AppDomain.CurrentDomain.ProcessExit -= OnProcessExit;
                }
            }

            static async Task<int> InvokeHandlerWithMiddleware(InvocationContext context, CancellationToken token)
            {
                InvocationMiddleware invocationChain = BuildInvocationChain(context, token, true);

                await invocationChain(context, _ => Task.CompletedTask);

                return GetExitCode(context);
            }

            // Windows: user presses Ctrl+C
            // Unix: the same + kill(SIGINT)
            void OnCancelKeyPress(object? sender, ConsoleCancelEventArgs e)
            {
                e.Cancel = true;

                Cancel(SIGINT_EXIT_CODE);
            }

            // Windows: user closes console app windows
            // Unix: kill(SIGTERM)
            void OnProcessExit(object? sender, EventArgs e) => Cancel(SIGTERM_EXIT_CODE);

            void Cancel(int forcedTerminationExitCode)
            {
                cts.Cancel();

                if (!startedInvocation.Wait(processTerminationTimeout.Value))
                {
                    processTerminationCompletionSource!.SetResult(forcedTerminationExitCode);
                }
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
                InvocationMiddleware invocationChain = BuildInvocationChain(context, CancellationToken.None, false);

                invocationChain(context, static _ => Task.CompletedTask).ConfigureAwait(false).GetAwaiter().GetResult();

                return GetExitCode(context);
            }
        }

        private static InvocationMiddleware BuildInvocationChain(InvocationContext context, CancellationToken cancellationToken, bool invokeAsync)
        {
            var invocations = new List<InvocationMiddleware>(context.Parser.Configuration.Middleware.Count + 1);
            invocations.AddRange(context.Parser.Configuration.Middleware);

            invocations.Add(async (invocationContext, _) =>
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
                    (ctx, next) =>
                        first(ctx,
                              c => second(c, next)));
        }

        private static int GetExitCode(InvocationContext context)
        {
            context.InvocationResult?.Invoke(context);

            return context.ExitCode;
        }
    }
}
