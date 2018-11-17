// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace System.CommandLine.Invocation
{
    internal class InvocationPipeline
    {
        private readonly ParseResult parseResult;
        private readonly Parser parser;

        public InvocationPipeline(
            Parser parser,
            ParseResult parseResult)
        {
            this.parser = parser ?? throw new ArgumentNullException(nameof(parser));
            this.parseResult = parseResult ?? throw new ArgumentNullException(nameof(parseResult));
        }

        public async Task<int> InvokeAsync(IConsole console = null)
        {
            var context = new InvocationContext(parseResult,
                                                parser,
                                                console);

            var invocations = new List<InvocationMiddleware>(context.Parser.Configuration.Middleware);

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
                        context.ResultCode = await handler.InvokeAsync(invocationContext);
                    }
                }
            });

            var invocationChain = invocations.Aggregate(
                (first, second) =>
                    (ctx, next) =>
                        first(ctx,
                              c => second(c, next)));

            await invocationChain(context, invocationContext => Task.CompletedTask);

            context.InvocationResult?.Apply(context);

            return context.ResultCode;
        }
    }
}
