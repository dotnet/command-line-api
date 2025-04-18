// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

namespace System.CommandLine.Invocation
{
    internal static class InvocationPipeline
    {
        internal static async Task<int> InvokeAsync(ParseResult parseResult, CancellationToken cancellationToken)
        {
            using var invokeActivity = Activities.ActivitySource.StartActivity(DiagnosticsStrings.InvokeMethod);
            if (invokeActivity is not null)
            {
                invokeActivity.DisplayName = parseResult.CommandResult.FullCommandName();
                invokeActivity.AddTag(DiagnosticsStrings.Command, parseResult.CommandResult.Command.Name);
                invokeActivity.AddTag(DiagnosticsStrings.InvokeType, DiagnosticsStrings.Async);
            }

            if (parseResult.Action is null)
            {
                invokeActivity?.SetStatus(Diagnostics.ActivityStatusCode.Error);
                return ReturnCodeForMissingAction(parseResult);
            }

            ProcessTerminationHandler? terminationHandler = null;
            using CancellationTokenSource cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);

            try
            {
                if (parseResult.PreActions is not null)
                {
                    for (int i = 0; i < parseResult.PreActions.Count; i++)
                    {
                        var action = parseResult.PreActions[i];

                        switch (action)
                        {
                            case SynchronousCommandLineAction syncAction:
                                syncAction.Invoke(parseResult);
                                break;
                            case AsynchronousCommandLineAction asyncAction:
                                await asyncAction.InvokeAsync(parseResult, cts.Token);
                                break;
                        }
                    }
                }

                switch (parseResult.Action)
                {
                    case SynchronousCommandLineAction syncAction:
                        var syncResult = syncAction.Invoke(parseResult);
                        invokeActivity?.SetExitCode(syncResult);
                        return syncResult;

                    case AsynchronousCommandLineAction asyncAction:
                        var startedInvocation = asyncAction.InvokeAsync(parseResult, cts.Token);
                        if (parseResult.Configuration.ProcessTerminationTimeout.HasValue)
                        {
                            terminationHandler = new(cts, startedInvocation, parseResult.Configuration.ProcessTerminationTimeout.Value);
                        }

                        if (terminationHandler is null)
                        {
                            var asyncResult = await startedInvocation;
                            invokeActivity?.SetExitCode(asyncResult);
                            return asyncResult;
                        }
                        else
                        {
                            // Handlers may not implement cancellation.
                            // In such cases, when CancelOnProcessTermination is configured and user presses Ctrl+C,
                            // ProcessTerminationCompletionSource completes first, with the result equal to native exit code for given signal.
                            Task<int> firstCompletedTask = await Task.WhenAny(startedInvocation, terminationHandler.ProcessTerminationCompletionSource.Task);
                            var asyncResult = await firstCompletedTask;  // return the result or propagate the exception
                            invokeActivity?.SetExitCode(asyncResult);
                            return asyncResult;
                        }

                    default:
                        var error = new ArgumentOutOfRangeException(nameof(parseResult.Action));
                        invokeActivity?.Error(error);
                        throw error;
                }
            }
            catch (Exception ex) when (parseResult.Configuration.EnableDefaultExceptionHandler)
            {
                invokeActivity?.Error(ex);
                return DefaultExceptionHandler(ex, parseResult.Configuration);
            }
            finally
            {
                terminationHandler?.Dispose();
            }
        }

        internal static int Invoke(ParseResult parseResult)
        {
            using var invokeActivity = Activities.ActivitySource.StartActivity(DiagnosticsStrings.InvokeMethod);
            if (invokeActivity is not null)
            {
                invokeActivity.DisplayName = parseResult.CommandResult.FullCommandName();
                invokeActivity.AddTag(DiagnosticsStrings.Command, parseResult.CommandResult.Command.Name);
                invokeActivity.AddTag(DiagnosticsStrings.InvokeType, DiagnosticsStrings.Sync);
            }

            switch (parseResult.Action)
            {
                case null:
                    invokeActivity?.Error();
                    return ReturnCodeForMissingAction(parseResult);

                case SynchronousCommandLineAction syncAction:
                    try
                    {
                        if (parseResult.PreActions is not null)
                        {
#if DEBUG
                            for (var i = 0; i < parseResult.PreActions.Count; i++)
                            {
                                var action = parseResult.PreActions[i];

                                if (action is not SynchronousCommandLineAction)
                                {
                                    parseResult.Configuration.EnableDefaultExceptionHandler = false;
                                    throw new Exception(
                                        $"This should not happen. An instance of {nameof(AsynchronousCommandLineAction)} ({action}) was called within {nameof(InvocationPipeline)}.{nameof(Invoke)}. This is supposed to be detected earlier resulting in a call to {nameof(InvocationPipeline)}{nameof(InvokeAsync)}");
                                }
                            }
#endif

                            for (var i = 0; i < parseResult.PreActions.Count; i++)
                            {
                                if (parseResult.PreActions[i] is SynchronousCommandLineAction syncPreAction)
                                {
                                    syncPreAction.Invoke(parseResult);
                                }
                            }
                        }

                        var result = syncAction.Invoke(parseResult);
                        invokeActivity?.SetExitCode(result);
                        return result;
                    }
                    catch (Exception ex) when (parseResult.Configuration.EnableDefaultExceptionHandler)
                    {
                        invokeActivity?.Error(ex);
                        return DefaultExceptionHandler(ex, parseResult.Configuration);
                    }

                default:
                    var error = new InvalidOperationException($"{nameof(AsynchronousCommandLineAction)} called within non-async invocation.");
                    invokeActivity?.Error(error);
                    throw error;
            }
        }

        private static int DefaultExceptionHandler(Exception exception, CommandLineConfiguration config)
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

        private static void Succeed(this Diagnostics.Activity activity)
        {
            activity.SetStatus(Diagnostics.ActivityStatusCode.Ok);
            activity.AddTag(DiagnosticsStrings.ExitCode, 0);
        }
        private static void Error(this Diagnostics.Activity activity, int statusCode)
        {
            activity.SetStatus(Diagnostics.ActivityStatusCode.Error);
            activity.AddTag(DiagnosticsStrings.ExitCode, statusCode);
        }

        private static void Error(this Diagnostics.Activity activity, Exception? exception = null)
        {
            activity.SetStatus(Diagnostics.ActivityStatusCode.Error);
            activity.AddTag(DiagnosticsStrings.ExitCode, 1);
            if (exception is not null)
            {
                var tagCollection = new Diagnostics.ActivityTagsCollection
                {
                    { DiagnosticsStrings.Exception, exception.ToString() }
                };
                var evt = new Diagnostics.ActivityEvent(DiagnosticsStrings.Exception, tags: tagCollection);
                activity.AddEvent(evt);
            }
        }

        private static void SetExitCode(this Diagnostics.Activity activity, int exitCode)
        {
            if (exitCode == 0)
            {
                activity.Succeed();
            }
            else
            {
                activity.Error(exitCode);
            }
        }
    }
}
