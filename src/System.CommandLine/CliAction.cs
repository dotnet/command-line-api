// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.CommandLine.Invocation;
using System.Threading;
using System.Threading.Tasks;

namespace System.CommandLine
{
    /// <summary>
    /// Defines the behavior of a symbol.
    /// </summary>
    public abstract class CliAction
    {
        public bool Exclusive { get; protected set; } = true;

        /// <summary>
        /// Performs an action when the associated symbol is invoked on the command line.
        /// </summary>
        /// <param name="parseResult">Provides the parse results.</param>
        /// <returns>A value that can be used as the exit code for the process.</returns>
        protected abstract int Invoke(ParseResult parseResult);

        /// <summary>
        /// Performs an action when the associated symbol is invoked on the command line.
        /// </summary>
        /// <param name="parseResult">Provides the parse results.</param>
        /// <param name="cancellationToken">The token to monitor for cancellation requests.</param>
        /// <returns>A value that can be used as the exit code for the process.</returns>
        protected abstract Task<int> InvokeAsync(ParseResult parseResult, CancellationToken cancellationToken = default);

        internal static async Task<int> InvokePipelineAsync(ParseResult parseResult, CancellationToken cancellationToken)
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
                    for (int i = 0; i < parseResult.NonexclusiveActions.Count; i++)
                    {
                        await parseResult.NonexclusiveActions[i].InvokeAsync(parseResult, cts.Token);
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

        internal static int InvokePipeline(ParseResult parseResult)
        {
            if (parseResult.Action is null)
            {
                return ReturnCodeForMissingAction(parseResult);
            }

            try
            {
                if (parseResult.NonexclusiveActions is not null)
                {
                    for (var i = 0; i < parseResult.NonexclusiveActions.Count; i++)
                    {
                        parseResult.NonexclusiveActions[i].Invoke(parseResult);
                    }
                }

                return parseResult.Action.Invoke(parseResult);
            }
            catch (Exception ex) when (parseResult.Configuration.EnableDefaultExceptionHandler)
            {
                return DefaultExceptionHandler(ex, parseResult.Configuration);
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
