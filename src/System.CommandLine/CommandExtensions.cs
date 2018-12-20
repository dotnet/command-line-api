// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Generic;
using System.CommandLine.Invocation;
using System.Linq;
using System.Reflection;

namespace System.CommandLine
{
    public static class CommandExtensions
    {
        public static void ConfigureFromMethod(
            this Command command,
            MethodInfo method,
            object target = null) =>
            command.ConfigureFromMethod(method, () => target);

        public static void ConfigureFromMethod(
            this Command command,
            MethodInfo method,
            Func<object> target)
        {
            if (command == null)
            {
                throw new ArgumentNullException(nameof(command));
            }

            if (method == null)
            {
                throw new ArgumentNullException(nameof(method));
            }

            var handler = new MethodBinder(method, target);

            foreach (var option in handler.BuildOptions())
            {
                command.AddOption(option);
            }

            command.Handler = handler;
        }

        public static void ConfigureFrom(this Command command, IOptionBuilder optionBuilder)
        {
            foreach (var option in optionBuilder.BuildOptions())
            {
                command.AddOption(option);
            }
        }

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
