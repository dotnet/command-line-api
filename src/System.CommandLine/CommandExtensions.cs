// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Generic;
using System.CommandLine.Invocation;
using System.Linq;
using System.Threading.Tasks;

namespace System.CommandLine
{
    public static class CommandExtensions
    {
        public static TCommand Subcommand<TCommand>(
            this TCommand command,
            string name)
            where TCommand : ICommand
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                throw new ArgumentException("Value cannot be null or whitespace.", nameof(name));
            }

            return command.Children
                          .OfType<TCommand>()
                          .Single(c => c.Name == name);
        }

        public static async Task<int> InvokeAsync(
            this Command command,
            string commandLine,
            IConsole console = null)
        {
            var parser = new Parser(command);
            return await new InvocationPipeline(parser, parser.Parse(commandLine))
                       .InvokeAsync(console);
        }

        public static async Task<int> InvokeAsync(
            this Command command,
            string[] args,
            IConsole console = null)
        {
            var parser = new Parser(command);
            return await new InvocationPipeline(parser, parser.Parse(args))
                       .InvokeAsync(console);
        }

        public static ParseResult Parse(
            this Command command,
            params string[] args) =>
            new Parser(new[] { command }).Parse(args);

        public static ParseResult Parse(
            this Command command,
            string commandLine,
            IReadOnlyCollection<char> delimiters = null) =>
            new Parser(new[] { command }).Parse(commandLine);
    }
}
