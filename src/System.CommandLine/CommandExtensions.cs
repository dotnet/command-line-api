// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Generic;
using System.CommandLine.Binding;
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
            IConsole console = null )
        {
            return GetInvocationPipeline(command, args).Invoke(console);
        }

        public static int Invoke(
            this Command command,
            string commandLine,
            IConsole console = null ) =>
            command.Invoke(CommandLineStringSplitter.Instance.Split(commandLine).ToArray(), console);

        public static async Task<int> InvokeAsync(
            this Command command,
            string[] args,
            IConsole console = null )
        {
            return await GetInvocationPipeline(command, args).InvokeAsync(console);
        }

        public static Task<int> InvokeAsync(
            this Command command,
            string commandLine,
            IConsole console = null) =>
            command.InvokeAsync(CommandLineStringSplitter.Instance.Split(commandLine).ToArray(), console);

        private static InvocationPipeline GetInvocationPipeline(Command command, string[] args )
        {
            var parser = command.ImplicitParser ?? new CommandLineBuilder( command )
                .UseDefaults()
                .Build();

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

        /// <summary>
        /// Defines a callback to be used to define a ModelBinder instance
        /// which can be used to map options and arguments to properties
        /// directly rather than by matching aliases to property names.
        /// </summary>
        /// <param name="command">the <see cref="Command"/> instance being configured</param>
        /// <param name="modelBinderFactory">the callback method to define a <see cref="ModelBinder{TModel}"/></param>
        /// <returns>the CommandLineBuilder instance being configured</returns>
        public static Command UseObjectBinding(
            this Command command,
            Func<List<IOption>, List<IArgument>, ModelBinder> modelBinderFactory )
        {
            command.ModelBinderFactory = modelBinderFactory;
            return command;
        }

    }
}
