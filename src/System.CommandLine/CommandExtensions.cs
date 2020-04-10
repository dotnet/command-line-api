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
    public static class CommandExtensions
    {
        public static int Invoke(
            this Command command,
            string[] args,
            IConsole console = null, 
            CommandLineBuilder builder = null )
        {
            return GetInvocationPipeline(command, args, builder).Invoke(console);
        }

        public static int Invoke(
            this Command command,
            string commandLine,
            IConsole console = null, 
            CommandLineBuilder builder = null ) =>
            command.Invoke(CommandLineStringSplitter.Instance.Split(commandLine).ToArray(), console, builder);

        public static async Task<int> InvokeAsync(
            this Command command,
            string[] args,
            IConsole console = null, 
            CommandLineBuilder builder = null )
        {
            return await GetInvocationPipeline(command, args, builder).InvokeAsync(console);
        }

        public static Task<int> InvokeAsync(
            this Command command,
            string commandLine,
            IConsole console = null) =>
            command.InvokeAsync(CommandLineStringSplitter.Instance.Split(commandLine).ToArray(), console);

        private static InvocationPipeline GetInvocationPipeline(Command command, string[] args, CommandLineBuilder builder = null)
        {
            builder ??= new CommandLineBuilder( command )
                .UseDefaults();

            var parser = command.ImplicitParser ?? builder.Build();

            var parseResult = parser.Parse(args);

            return new InvocationPipeline(parseResult);
        }

        public static ParseResult Parse(
            this Command command,
            params string[] args) =>
            new Parser(command).Parse(args);

        public static ParseResult Parse(
            this Command command,
            string commandLine,
            IReadOnlyCollection<char> delimiters = null) =>
            new Parser(command).Parse(commandLine);
    }
}
