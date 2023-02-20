// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Threading;
using System.Threading.Tasks;

namespace System.CommandLine.Parsing
{
    /// <summary>
    /// Provides extension methods for parsers.
    /// </summary>
    public static class CommandLineConfigurationExtensions
    {
        /// <summary>
        /// Parses a command line string value and invokes the handler for the indicated command.
        /// </summary>
        /// <returns>The exit code for the invocation.</returns>
        /// <remarks>The command line string input will be split into tokens as if it had been passed on the command line.</remarks>
        public static int Invoke(
            this CommandLineConfiguration configuration,
            string commandLine,
            IConsole? console = null) =>
            configuration.RootCommand.Parse(commandLine, configuration).Invoke(console);

        /// <summary>
        /// Parses a command line string array and invokes the handler for the indicated command.
        /// </summary>
        /// <returns>The exit code for the invocation.</returns>
        public static int Invoke(
            this CommandLineConfiguration configuration,
            string[] args,
            IConsole? console = null) =>
            configuration.RootCommand.Parse(args, configuration).Invoke(console);

        /// <summary>
        /// Parses a command line string value and invokes the handler for the indicated command.
        /// </summary>
        /// <returns>The exit code for the invocation.</returns>
        /// <remarks>The command line string input will be split into tokens as if it had been passed on the command line.</remarks>
        public static Task<int> InvokeAsync(
            this CommandLineConfiguration configuration,
            string commandLine,
            IConsole? console = null,
            CancellationToken cancellationToken = default) =>
            configuration.RootCommand.Parse(commandLine, configuration).InvokeAsync(console, cancellationToken);

        /// <summary>
        /// Parses a command line string array and invokes the handler for the indicated command.
        /// </summary>
        /// <returns>The exit code for the invocation.</returns>
        public static Task<int> InvokeAsync(
            this CommandLineConfiguration configuration,
            string[] args,
            IConsole? console = null,
            CancellationToken cancellationToken = default) =>
            configuration.RootCommand.Parse(args, configuration).InvokeAsync(console, cancellationToken);
    }
}