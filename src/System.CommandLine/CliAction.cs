// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

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
        public abstract int Invoke(ParseResult parseResult);

        /// <summary>
        /// Performs an action when the associated symbol is invoked on the command line.
        /// </summary>
        /// <param name="parseResult">Provides the parse results.</param>
        /// <param name="cancellationToken">The token to monitor for cancellation requests.</param>
        /// <returns>A value that can be used as the exit code for the process.</returns>
        public abstract Task<int> InvokeAsync(ParseResult parseResult, CancellationToken cancellationToken = default);
    }
}
