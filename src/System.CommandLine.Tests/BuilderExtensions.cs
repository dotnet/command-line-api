// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.CommandLine.Builder;

namespace System.CommandLine.Tests
{
    public static class BuilderExtensions
    {
        public static TBuilder AddCommand<TBuilder>(
            this TBuilder builder,
            string name,
            string description,
            Action<CommandBuilder> symbols = null,
            Action<ArgumentBuilder> arguments = null,
            IHelpBuilder helpBuilder = null)
            where TBuilder : CommandBuilder
        {
            var commandBuilder = new CommandBuilder(new Command(name, description))
                                 {
                                     HelpBuilder = helpBuilder ?? builder.HelpBuilder
                                 };

            if (symbols != null)
            {
                symbols.Invoke(commandBuilder);
            }

            var command = commandBuilder.Command;

            if (arguments != null)
            {
                var argumentBuilder = new ArgumentBuilder();

                arguments.Invoke(argumentBuilder);

                command.Argument = argumentBuilder.Build();
            }

            builder.AddCommand(command);

            return builder;
        }

        public static TBuilder AddCommand<TBuilder>(
            this TBuilder builder,
            string name,
            string description = null,
            Argument argument = null,
            IHelpBuilder helpBuilder = null)
            where TBuilder : CommandBuilder
        {
            var command = new Command(name, helpBuilder: helpBuilder ?? builder.HelpBuilder)
                          {
                              Description = description
                          };

            if (argument != null)
            {
                command.Argument = argument;
            }

            builder.AddCommand(command);

            return builder;
        }
        
        public static Argument ExactlyOne(
            this ArgumentBuilder builder)
        {
            builder.Configure(argument => argument.Arity = ArgumentArity.ExactlyOne);

            return builder.Build();
        }

        public static Argument None(
            this ArgumentBuilder builder)
        {
            builder.Configure(argument => argument.Arity = ArgumentArity.Zero);

            return builder.Build();
        }

        public static Argument ZeroOrMore(
            this ArgumentBuilder builder)
        {
            builder.Configure(argument => argument.Arity = ArgumentArity.ZeroOrMore);

            return builder.Build();
        }

        public static Argument ZeroOrOne(
            this ArgumentBuilder builder)
        {
            builder.Configure(argument => argument.Arity = ArgumentArity.ZeroOrOne);

            return builder.Build();
        }

        public static Argument OneOrMore(
            this ArgumentBuilder builder)
        {
            builder.Configure(argument => argument.Arity = ArgumentArity.OneOrMore);

            return builder.Build();
        }

        public static ArgumentBuilder WithHelp(
            this ArgumentBuilder builder,
            string name = null,
            string description = null)
        {
            builder.Configure(a => a.WithHelp(name, description));

            return builder;
        }
    }
}
