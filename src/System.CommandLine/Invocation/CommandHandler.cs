// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Threading.Tasks;

namespace System.CommandLine.Invocation
{
    /// <summary>
    /// Provides methods for creating and working with command handlers.
    /// </summary>
    public static partial class CommandHandler
    {
        private class AnonymousCommandHandler : ICommandHandler
        {
            private readonly Func<InvocationContext, Task> _getResult;

            public AnonymousCommandHandler(Func<InvocationContext, Task> getResult)
            {
                _getResult = getResult;
            }

            public AnonymousCommandHandler(Action<InvocationContext> getResult)
            {
                _getResult = GetResult;

                Task GetResult(InvocationContext context)
                {
                    getResult(context);
                    return Task.FromResult(0);
                }
            }

            public Task<int> InvokeAsync(InvocationContext context) =>
                GetExitCodeAsync(_getResult(context), context);
        }

        internal static async Task<int> GetExitCodeAsync(object returnValue, InvocationContext context)
        {
            switch (returnValue)
            {
                case Task<int> exitCodeTask:
                    return await exitCodeTask;
                case Task task:
                    await task;
                    return context.ExitCode;
                case int exitCode:
                    return exitCode;
                case null:
                    return context.ExitCode;
                default:
                    throw new NotSupportedException();
            }
        }
    }
}