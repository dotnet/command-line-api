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
        internal static async Task<int> InvokeAsync(ParseResult parseResult, IConsole? console = null, CancellationToken cancellationToken = default)
        {
            using InvocationContext context = new (parseResult, console, cancellationToken);

            try
            {
                if (context.Parser.Configuration.Middleware.Count == 0 && parseResult.Handler is not null)
                {
                    return await parseResult.Handler.InvokeAsync(context);
                }

                return await InvokeHandlerWithMiddleware(context);
            }
            catch (Exception ex) when (context.Parser.Configuration.ExceptionHandler is not null)
            {
                context.Parser.Configuration.ExceptionHandler(ex, context);
                return context.ExitCode;
            }

            static async Task<int> InvokeHandlerWithMiddleware(InvocationContext context)
            {
                InvocationMiddleware invocationChain = BuildInvocationChain(context, true);

                await invocationChain(context, _ => Task.CompletedTask);

                return GetExitCode(context);
            }
        }

        internal static int Invoke(ParseResult parseResult, IConsole? console = null)
        {
            using InvocationContext context = new (parseResult, console);

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

                invocationChain(context, static _ => Task.CompletedTask).ConfigureAwait(false).GetAwaiter().GetResult();

                return GetExitCode(context);
            }
        }

        private static InvocationMiddleware BuildInvocationChain(InvocationContext context, bool invokeAsync)
        {
            var invocations = new List<InvocationMiddleware>(context.Parser.Configuration.Middleware.Count + 1);
            invocations.AddRange(context.Parser.Configuration.Middleware);

            invocations.Add(async (invocationContext, _) =>
            {
                if (invocationContext.ParseResult.Handler is { } handler)
                {
                    context.ExitCode = invokeAsync
                                           ? await handler.InvokeAsync(invocationContext)
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
