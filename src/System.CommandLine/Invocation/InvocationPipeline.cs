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

        public async Task<int> InvokeAsync(IConsole? console = null)
        {
            var context = new InvocationContext(parseResult, console);

            InvocationMiddleware invocationChain = BuildInvocationChain(context);

            await invocationChain(context, invocationContext => Task.CompletedTask);

            return GetExitCode(context);
        }

        public int Invoke(IConsole? console = null)
        {
            var context = new InvocationContext(parseResult, console);

            InvocationMiddleware invocationChain = BuildInvocationChain(context);

            Task.Run(() => invocationChain(context, invocationContext => Task.CompletedTask)).GetAwaiter().GetResult();

            return GetExitCode(context);
        }

        private static InvocationMiddleware BuildInvocationChain(InvocationContext context)
        {
            var invocations = new List<InvocationMiddleware>(context.Parser.Configuration.Middleware.Count + 1);
            invocations.AddRange(context.Parser.Configuration.Middleware);

            invocations.Add(async (invocationContext, next) =>
            {
                if (invocationContext
                    .ParseResult
                    .CommandResult
                    .Command is Command command)
                {
                    var handler = command.Handler;

                    if (handler != null)
                    {
                        context.ExitCode = await handler.InvokeAsync(invocationContext);
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
