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