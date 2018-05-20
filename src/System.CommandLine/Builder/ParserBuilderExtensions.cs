// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace System.CommandLine.Builder
{
    public static class ParserBuilderExtensions
    {
        public static TBuilder AddCommand<TBuilder>(
            this TBuilder builder,
            string name,
            string description = null,
            Action<CommandDefinitionBuilder> symbols = null,
            Action<ArgumentDefinitionBuilder> arguments = null)
            where TBuilder : CommandDefinitionBuilder
        {
            var commandDefinitionBuilder = new CommandDefinitionBuilder(name, builder) {
                Description = description
            };

            symbols?.Invoke(commandDefinitionBuilder);

            arguments?.Invoke(commandDefinitionBuilder.Arguments);

            builder.CommandDefinitionBuilders.Add(commandDefinitionBuilder);

            return builder;
        }

        public static TBuilder AddOption<TBuilder>(
            this TBuilder builder,
            string[] aliases,
            string description = null,
            Action<ArgumentDefinitionBuilder> arguments = null)
            where TBuilder : CommandDefinitionBuilder
        {
            var optionDefinitionBuilder = new OptionDefinitionBuilder(aliases, builder) {
                Description = description
            };

            arguments?.Invoke(optionDefinitionBuilder.Arguments);

            builder.OptionDefinitionBuilders.Add(optionDefinitionBuilder);

            return builder;
        }

        public static TBuilder AddOption<TBuilder>(
            this TBuilder builder,
            string name,
            string description = null,
            Action<ArgumentDefinitionBuilder> arguments = null)
            where TBuilder : CommandDefinitionBuilder
        {
            return builder.AddOption(new[] { name }, description, arguments);
        }

        public static ParserBuilder EnablePosixBundling(
            this ParserBuilder builder,
            bool value = true)
        {
            builder.EnablePosixBundling = value;
            return builder;
        }

        public static ParserBuilder TreatUnmatchedTokensAsErrors(
            this ParserBuilder builder,
            bool value = true)
        {
            builder.TreatUnmatchedTokensAsErrors = value;
            return builder;
        }
        public static TBuilder AddArguments<TBuilder>(
            this TBuilder builder,
            Action<ArgumentDefinitionBuilder> action)
            where TBuilder : CommandDefinitionBuilder
        {
            action.Invoke(builder.Arguments);
            return builder;
        }

        public static ParserBuilder ParseResponseFileAs(
            this ParserBuilder builder,
            ResponseFileHandling responseFileHandling)
        {
            builder.ResponseFileHandling = responseFileHandling;
            return builder;
        }
    }
}
