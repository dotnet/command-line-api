// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Threading;
using System.Threading.Tasks;

namespace System.CommandLine.Invocation
{
    internal static class InvocationPipeline
    {
        internal static async Task<int> InvokeAsync(ParseResult parseResult, CancellationToken cancellationToken)
        {
            ProcessTerminationHandler? terminationHandler = null;
            using CancellationTokenSource cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);

            try
            {
                int actionResult = 0;
                int preActionResult = 0;

                if (parseResult.PreActions is not null)
                {
                    for (int i = 0; i < parseResult.PreActions.Count; i++)
                    {
                        var action = parseResult.PreActions[i];
                        var result = 0;

                        switch (action)
                        {
                            case SynchronousCommandLineAction syncAction:
                                result = syncAction.Invoke(parseResult);
                                break;
                            case AsynchronousCommandLineAction asyncAction:
                                result = await asyncAction.InvokeAsync(parseResult, cts.Token);
                                break;
                           
                        }

                        if (result != 0)
                        {
                            preActionResult = result;
                        }
                    }
                }

                if (parseResult.Action is null)
                {
                    return preActionResult != 0 ? preActionResult : ReturnCodeForMissingAction(parseResult);
                }

                switch (parseResult.Action)
                {
                    case SynchronousCommandLineAction syncAction:
                        actionResult = syncAction.Invoke(parseResult);
                        break;

                    case AsynchronousCommandLineAction asyncAction:
                        var timeout = parseResult.InvocationConfiguration.ProcessTerminationTimeout;

                        if (timeout.HasValue)
                        {
                            terminationHandler = new(cts, timeout.Value);
                        }

                        var startedInvocation = asyncAction.InvokeAsync(parseResult, cts.Token);

                        if (terminationHandler is null)
                        {
                            actionResult = await startedInvocation;
                        }
                        else
                        {
                            terminationHandler.StartedHandler = startedInvocation;
                            // Handlers may not implement cancellation.
                            // In such cases, when CancelOnProcessTermination is configured and user presses Ctrl+C,
                            // ProcessTerminationCompletionSource completes first, with the result equal to native exit code for given signal.
                            Task<int> firstCompletedTask = await Task.WhenAny(startedInvocation, terminationHandler.ProcessTerminationCompletionSource.Task);
                            actionResult = await firstCompletedTask; // return the result or propagate the exception
                        }
                        break;

                    default:
                        throw new ArgumentOutOfRangeException(nameof(parseResult.Action));
                }

                return preActionResult != 0 ? preActionResult : actionResult;
            }
            catch (Exception ex) when (parseResult.InvocationConfiguration.EnableDefaultExceptionHandler)
            {
                return DefaultExceptionHandler(ex, parseResult);
            }
            finally
            {
                terminationHandler?.Dispose();
            }
        }

        internal static int Invoke(ParseResult parseResult)
        {
            try
            {
                int preActionResult = 0;

                if (parseResult.PreActions is not null)
                {
                    for (var i = 0; i < parseResult.PreActions.Count; i++)
                    {
                        if (parseResult.PreActions[i] is SynchronousCommandLineAction syncPreAction)
                        {
                            int result = syncPreAction.Invoke(parseResult);
                            
                            if (result != 0)
                            {
                                preActionResult = result;
                            }
                        }
                    }
                }

                switch (parseResult.Action)
                {
                    case null:
                        return preActionResult != 0 ? preActionResult : ReturnCodeForMissingAction(parseResult);

                    case SynchronousCommandLineAction syncAction:
                        int actionResult = syncAction.Invoke(parseResult);
                        return preActionResult != 0 ? preActionResult : actionResult;

                    default:
                        throw new InvalidOperationException($"{nameof(AsynchronousCommandLineAction)} called within non-async invocation.");
                }
            }
            catch (Exception ex) when (parseResult.InvocationConfiguration.EnableDefaultExceptionHandler)
            {
                return DefaultExceptionHandler(ex, parseResult);
            }
        }

        private static int DefaultExceptionHandler(Exception exception, ParseResult parseResult)
        {
            if (exception is not OperationCanceledException)
            {
                ConsoleHelpers.ResetTerminalForegroundColor();
                ConsoleHelpers.SetTerminalForegroundRed();

                var error = parseResult.InvocationConfiguration.Error;

                error.Write(LocalizationResources.ExceptionHandlerHeader());
                error.WriteLine(exception.ToString());

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
