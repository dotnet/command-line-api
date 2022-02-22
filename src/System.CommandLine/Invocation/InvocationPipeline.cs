// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Generic;
using System.CommandLine.Parsing;
using System.Linq;
using System.Threading.Tasks;

namespace System.CommandLine.Invocation
{
    internal class InvocationPipeline
    {
        private readonly ParseResult parseResult;

        public InvocationPipeline(ParseResult parseResult)
        {
            this.parseResult = parseResult ?? throw new ArgumentNullException(nameof(parseResult));
        }

        public Task<int> InvokeAsync(IConsole? console = null)
        {
            var context = new InvocationContext(parseResult, console);

            if (context.Parser.Configuration.Middleware.Count == 0
                && context.ParseResult.CommandResult.Command.Handler is ICommandHandler handler)
            {
                return handler.InvokeAsync(context);
            }

            return FullInvocationChainAsync(context);

            static async Task<int> FullInvocationChainAsync(InvocationContext context)
            {
                InvocationMiddleware invocationChain = BuildInvocationChain(context, true);

                await invocationChain(context, _ => Task.CompletedTask);

                return GetExitCode(context);
            }
        }

        public int Invoke(IConsole? console = null)
        {
            var context = new InvocationContext(parseResult, console);

            if (context.Parser.Configuration.Middleware.Count == 0
                && context.ParseResult.CommandResult.Command.Handler is ICommandHandler handler)
            {
                return handler.Invoke(context);
            }

            return FullInvocationChain(context); // kept in a separate method to avoid JITting

            static int FullInvocationChain(InvocationContext context)
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
                if (invocationContext
                    .ParseResult
                    .CommandResult
                    .Command is Command command)
                {
                    var handler = command.Handler;

                    if (handler is not null)
                    {
                        context.ExitCode = invokeAsync
                            ? await handler.InvokeAsync(invocationContext)
                            : handler.Invoke(invocationContext);
                    }
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
            context.InvocationResult?.Apply(context);

            return context.ExitCode;
        }
    }
}
