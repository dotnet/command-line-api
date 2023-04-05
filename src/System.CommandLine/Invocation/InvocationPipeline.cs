// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace System.CommandLine.Invocation
{
    internal static class InvocationPipeline
    {
        internal static async Task<int> InvokeAsync(ParseResult parseResult, CancellationToken cancellationToken)
        {
            if (parseResult.Action is null)
            {
                return ReturnCodeForMissingAction(parseResult);
            }

            ProcessTerminationHandler? terminationHandler = null;
            using CancellationTokenSource cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);

            try
            {
                if (parseResult.NonexclusiveActions is not null)
                {
                    for (var i = 0; i < parseResult.NonexclusiveActions.Count; i++)
                    {
                        var action = parseResult.NonexclusiveActions[i];
                        await action.InvokeAsync(parseResult, cts.Token);
                    }
                }

                Task<int> startedInvocation = parseResult.Action.InvokeAsync(parseResult, cts.Token);

                if (parseResult.Configuration.ProcessTerminationTimeout.HasValue)
                {
                    terminationHandler = new(cts, startedInvocation, parseResult.Configuration.ProcessTerminationTimeout.Value);
                }

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
            catch (Exception ex) when (parseResult.Configuration.EnableDefaultExceptionHandler)
            {
                return DefaultExceptionHandler(ex, parseResult.Configuration);
            }
            finally
            {
                terminationHandler?.Dispose();
            }
        }

        internal static int Invoke(ParseResult parseResult)
        {
            if (parseResult.Action is null)
            {
                return ReturnCodeForMissingAction(parseResult);
            }

            if (parseResult.NonexclusiveActions is not null)
            {
                for (var i = 0; i < parseResult.NonexclusiveActions.Count; i++)
                {
                    var action = parseResult.NonexclusiveActions[i];
                    var result = TryInvokeAction(parseResult, action);
                    if (!result.success)
                    {
                        return result.returnCode;
                    }
                }
            }

            return TryInvokeAction(parseResult, parseResult.Action).returnCode;

            static (int returnCode, bool success) TryInvokeAction(ParseResult parseResult, CliAction action)
            {
                try
                {
                    return (action.Invoke(parseResult), true);
                }
                catch (Exception ex) when (parseResult.Configuration.EnableDefaultExceptionHandler)
                {
                    return (DefaultExceptionHandler(ex, parseResult.Configuration), false);
                }
            }
        }

        private static int DefaultExceptionHandler(Exception exception, CliConfiguration config)
        {
            if (exception is not OperationCanceledException)
            {
                ConsoleHelpers.ResetTerminalForegroundColor();
                ConsoleHelpers.SetTerminalForegroundRed();

                config.Error.Write(LocalizationResources.ExceptionHandlerHeader());
                config.Error.WriteLine(exception.ToString());

                ConsoleHelpers.ResetTerminalForegroundColor();
            }
            return 1;
        }

        private static int ReturnCodeForMissingAction(ParseResult parseResult)
        {
            if (parseResult.Errors.Count > 0)
            {
                return 1;
            }
            else
            {
                return 0;
            }
        }
    }
}
