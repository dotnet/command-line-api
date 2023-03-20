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
            if (parseResult.Action is null)
            {
                return 0;
            }

            InvocationContext context = new (parseResult, console);
            ProcessTerminationHandler? terminationHandler = null;
            using CancellationTokenSource cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);

            try
            {
                Task<int> startedInvocation = parseResult.Action.InvokeAsync(context, cts.Token);

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
        }

        internal static int Invoke(ParseResult parseResult, IConsole? console = null)
        {
            if (parseResult.Action is null)
            {
                return 0;
            }

            InvocationContext context = new (parseResult, console);

            try
            {
                return parseResult.Action.Invoke(context);
            }
            catch (Exception ex) when (parseResult.Configuration.ExceptionHandler is not null)
            {
                return parseResult.Configuration.ExceptionHandler(ex, context);
            }
        }
    }
}
