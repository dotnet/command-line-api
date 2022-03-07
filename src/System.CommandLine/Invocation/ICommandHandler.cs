// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Threading.Tasks;

namespace System.CommandLine.Invocation
{
    /// <summary>
    /// Defines the behavior of a command.
    /// </summary>
    public interface ICommandHandler
    {
        /// <summary>
        /// Performs an action when the associated command is invoked on the command line.
        /// </summary>
        /// <param name="context">Provides context for the invocation, including parse results and binding support.</param>
        /// <returns>A value that can be used as the exit code for the process.</returns>
        int Invoke(InvocationContext context);

        /// <summary>
        /// Performs an action when the associated command is invoked on the command line.
        /// </summary>
        /// <param name="context">Provides context for the invocation, including parse results and binding support.</param>
        /// <returns>A value that can be used as the exit code for the process.</returns>
        Task<int> InvokeAsync(InvocationContext context);
    }
}
