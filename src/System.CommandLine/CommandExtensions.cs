// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Generic;
using System.CommandLine.Builder;
using System.CommandLine.Invocation;
using System.CommandLine.Parsing;
using System.Linq;
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
            return GetInvocationPipeline(command, args).Invoke(console);
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
            command.Invoke(CommandLineStringSplitter.Instance.Split(commandLine).ToArray(), console);

        /// <summary>
        /// Parses and invokes a command.
        /// </summary>
        /// <param name="command">The command to invoke.</param>
        /// <param name="args">The arguments to parse.</param>
        /// <param name="console">The console to which output is written during invocation.</param>
        /// <returns>The exit code for the invocation.</returns>
        public static async Task<int> InvokeAsync(
            this Command command,
            string[] args,
            IConsole? console = null)
        {
            return await GetInvocationPipeline(command, args).InvokeAsync(console);
        }

        /// <summary>
        /// Parses and invokes a command.
        /// </summary>
        /// <remarks>The command line string input will be split into tokens as if it had been passed on the command line.</remarks>
        /// <param name="command">The command to invoke.</param>
        /// <param name="commandLine">The command line to parse.</param>
        /// <param name="console">The console to which output is written during invocation.</param>
        /// <returns>The exit code for the invocation.</returns>
        public static Task<int> InvokeAsync(
            this Command command,
            string commandLine,
            IConsole? console = null) =>
            command.InvokeAsync(CommandLineStringSplitter.Instance.Split(commandLine).ToArray(), console);

        private static InvocationPipeline GetInvocationPipeline(Command command, string[] args)
        {
            var parser = command.ImplicitParser ??
                         new CommandLineBuilder(command)
                             .UseDefaults()
                             .Build();

            var parseResult = parser.Parse(args);

            return new InvocationPipeline(parseResult);
        }

        /// <summary>
        /// Parses an array strings using the specified command.
        /// </summary>
        /// <param name="command">The command to use to parse the command line input.</param>
        /// <param name="args">The string arguments to parse.</param>
        /// <returns>A parse result describing the outcome of the parse operation.</returns>
        public static ParseResult Parse(
            this Command command,
            params string[] args) =>
            new Parser(command).Parse(args);

        /// <summary>
        /// Parses a command line string value using the specified command.
        /// </summary>
        /// <remarks>The command line string input will be split into tokens as if it had been passed on the command line.</remarks>
        /// <param name="command">The command to use to parse the command line input.</param>
        /// <param name="commandLine">A command line string to parse, which can include spaces and quotes equivalent to what can be entered into a terminal.</param>
        /// <returns>A parse result describing the outcome of the parse operation.</returns>
        public static ParseResult Parse(
            this Command command,
            string commandLine) =>
            new Parser(command).Parse(commandLine);

        private const string _messageForWhenGeneratorIsNotInUse =
            "This overload should not be called. You should reference the System.CommandLine.Generator package which will generate a more specific overload for your delegate.";

        /// <summary>
        /// Sets a command handler.
        /// </summary>
        /// <remarks>Currently, this method only works with C# source generators.</remarks>
        /// <param name="command">The command on which to set the handler.</param>
        /// <param name="delegate">A delegate implementing the handler for the command.</param>
        /// <param name="symbols">The symbols used to bind the handler's parameters.</param>
        public static void SetHandler<TDelegate>(
            this Command command,
            TDelegate @delegate,
            params ISymbol[] symbols)
        {
            throw new InvalidOperationException(_messageForWhenGeneratorIsNotInUse);
        }
    }
}