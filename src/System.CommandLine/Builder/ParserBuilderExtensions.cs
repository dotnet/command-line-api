// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

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

        public static CommandDefinitionBuilder OnExecute(
            this CommandDefinitionBuilder builder,
          Action action)
        {
            var methodBinder = new MethodBinder(action);
            builder.ExecutionHandler = methodBinder;
            return builder;
        }

        public static CommandDefinitionBuilder OnExecute<T>(
            this CommandDefinitionBuilder builder,
            Action<T> action, string optionAlias)
        {
            var methodBinder = new MethodBinder(action, optionAlias);
            builder.ExecutionHandler = methodBinder;
            return builder;
        }

        public static CommandDefinitionBuilder OnExecute<T1, T2>(
            this CommandDefinitionBuilder builder,
            Action<T1, T2> action, string optionAlias1, string optionAlias2)
        {
            var methodBinder = new MethodBinder(action, optionAlias1, optionAlias2);
            builder.ExecutionHandler = methodBinder;
            return builder;
        }

        public static CommandDefinitionBuilder OnExecute(
            this CommandDefinitionBuilder builder,
            Delegate action, params string[] optionAliases)
        {
            var methodBinder = new MethodBinder(action, optionAliases);
            builder.ExecutionHandler = methodBinder;
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

        public static TBuilder UsePrefixes<TBuilder>(this TBuilder builder, IReadOnlyCollection<string> prefixes)
            where TBuilder : ParserBuilder
        {
            builder.Prefixes = prefixes;
            return builder;
        }
    }
}
