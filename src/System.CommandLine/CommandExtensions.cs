// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.CommandLine.Invocation;
using System.CommandLine.Parsing;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace System.CommandLine
{
    /// <summary>
    /// Provides extension methods for <see cref="Command" />.
    /// </summary>
    public static class CommandExtensions
    {
        /// <summary>
        /// Parses and invokes a command.
        /// </summary>
        /// <param name="command">The command to invoke.</param>
        /// <param name="args">The arguments to parse.</param>
        /// <param name="console">The console to which output is written during invocation.</param>
        /// <returns>The exit code for the invocation.</returns>
        public static int Invoke(
            this Command command,
            string[] args,
            IConsole? console = null)
        {
            ParseResult parseResult = command.Parse(args);

            return InvocationPipeline.Invoke(parseResult, console);
        }

        /// <summary>
        /// Parses and invokes a command.
        /// </summary>
        /// <remarks>The command line string input will be split into tokens as if it had been passed on the command line.</remarks>
        /// <param name="command">The command to invoke.</param>
        /// <param name="commandLine">The command line to parse.</param>
        /// <param name="console">The console to which output is written during invocation.</param>
        /// <returns>The exit code for the invocation.</returns>
        public static int Invoke(
            this Command command,
            string commandLine,
            IConsole? console = null) =>
            command.Invoke(Parser.Split(commandLine).ToArray(), console);

        /// <summary>
        /// Parses and invokes a command.
        /// </summary>
        /// <param name="command">The command to invoke.</param>
        /// <param name="args">The arguments to parse.</param>
        /// <param name="console">The console to which output is written during invocation.</param>
        /// <param name="cancellationToken">A token that can be used to cancel the invocation.</param>
        /// <returns>The exit code for the invocation.</returns>
        public static Task<int> InvokeAsync(
            this Command command,
            string[] args,
            IConsole? console = null, 
            CancellationToken cancellationToken = default)
        {
            ParseResult parseResult = command.Parse(args);

            return InvocationPipeline.InvokeAsync(parseResult, console, cancellationToken);
        }

        /// <summary>
        /// Parses and invokes a command.
        /// </summary>
        /// <remarks>The command line string input will be split into tokens as if it had been passed on the command line.</remarks>
        /// <param name="command">The command to invoke.</param>
        /// <param name="commandLine">The command line to parse.</param>
        /// <param name="console">The console to which output is written during invocation.</param>
        /// <param name="cancellationToken">A token that can be used to cancel the invocation.</param>
        /// <returns>The exit code for the invocation.</returns>
        public static Task<int> InvokeAsync(
            this Command command,
            string commandLine,
            IConsole? console = null,
            CancellationToken cancellationToken = default) =>
            command.InvokeAsync(Parser.Split(commandLine).ToArray(), console, cancellationToken);
    }
}