// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections;
using System.IO;
using System.Linq;

namespace System.CommandLine
{
    public static class Define
    {
        public static ArgumentDefinitionBuilder Arguments()
        {
            return new ArgumentDefinitionBuilder();
        }

        #region arity

        public static ArgumentDefinition ExactlyOne(
            this ArgumentDefinitionBuilder builder,
            Func<Symbol, string> errorMessage = null)
        {
            builder.AddValidator(symbol =>
            {
                var argumentCount = symbol.Arguments.Count;

                if (argumentCount == 0)
                {
                    if (errorMessage == null)
                    {
                        return symbol is Command command
                                   ? ValidationMessages.RequiredArgumentMissingForCommand(command.Definition.ToString())
                                   : ValidationMessages.RequiredArgumentMissingForOption(symbol.SymbolDefinition.ToString());
                    }
                    else
                    {
                        return errorMessage(symbol);
                    }
                }

                if (argumentCount > 1)
                {
                    if (errorMessage == null)
                    {
                        return ValidationMessages.SymbolAcceptsOnlyOneArgument(symbol);
                    }
                    else
                    {
                        return errorMessage(symbol);
                    }
                }

                return null;
            });

            builder.ArgumentArity = ArgumentArity.One;

            return builder.Build();
        }

        public static ArgumentDefinition ZeroOrMore(
            this ArgumentDefinitionBuilder builder,
            Func<Option, string> errorMessage = null)
        {
            builder.ArgumentArity = ArgumentArity.Many;

            return builder.Build();
        }

        public static ArgumentDefinition ZeroOrOne(
            this ArgumentDefinitionBuilder builder,
            Func<Option, string> errorMessage = null)
        {
            builder.AddValidator(symbol =>
            {
                if (symbol.Arguments.Count > 1)
                {
                    return symbol is Command command
                               ? ValidationMessages.CommandAcceptsOnlyOneArgument(command.Definition.ToString(), command.Arguments.Count)
                               : ValidationMessages.OptionAcceptsOnlyOneArgument(symbol.SymbolDefinition.ToString(), symbol.Arguments.Count);
                }

                return null;
            });

            builder.ArgumentArity = ArgumentArity.One;

            return builder.Build();
        }

        public static ArgumentDefinition OneOrMore(
            this ArgumentDefinitionBuilder builder,
            Func<Symbol, string> errorMessage = null)
        {
            builder.AddValidator(o =>
            {
                var optionCount = o.Arguments.Count;

                if (optionCount != 0)
                {
                    return null;
                }

                if (errorMessage != null)
                {
                    return errorMessage(o);
                }

                return o.SymbolDefinition is CommandDefinition
                           ? ValidationMessages.RequiredArgumentMissingForCommand(o.SymbolDefinition.ToString())
                           : ValidationMessages.RequiredArgumentMissingForOption(o.SymbolDefinition.ToString());
            });

            builder.ArgumentArity = ArgumentArity.Many;

            return builder.Build();
        }

        #endregion

        #region set inclusion

        public static ArgumentDefinitionBuilder FromAmong(
            this ArgumentDefinitionBuilder builder,
            params string[] values)
        {
            builder.ValidTokens.UnionWith(values);

            builder.SuggestionSource.AddSuggestions(values);

            return builder;
        }

        #endregion

        #region files

        public static ArgumentDefinitionBuilder ExistingFilesOnly(
            this ArgumentDefinitionBuilder builder)
        {
            builder.AddValidator(symbol =>
            {
                return symbol.Arguments
                                   .Where(filePath => !File.Exists(filePath) &&
                                                      !Directory.Exists(filePath))
                                   .Select(ValidationMessages.FileDoesNotExist)
                                   .FirstOrDefault();
            });
            return builder;
        }

        public static ArgumentDefinitionBuilder LegalFilePathsOnly(
            this ArgumentDefinitionBuilder builder)
        {
            builder.AddValidator(symbol =>
            {
                foreach (var arg in symbol.Arguments)
                {
                    try
                    {
                        var fileInfo = new FileInfo(arg);
                    }
                    catch (NotSupportedException ex)
                    {
                        return ex.Message;
                    }
                    catch (ArgumentException ex)
                    {
                        return ex.Message;
                    }
                }

                return null;
            });

            return builder;
        }

        #endregion

        #region type / return value

        public static ArgumentDefinition ParseArgumentsAs<T>(
            this ArgumentDefinitionBuilder builder) =>
            ParseArgumentsAs<T>(
                builder,
                symbol => {
                    switch (typeof(T).DefaultArity())
                    {
                        case ArgumentArity.One:
                            return ArgumentConverter.Parse<T>(symbol.Arguments.Single());
                        case ArgumentArity.Many:
                            return ArgumentConverter.ParseMany<T>(symbol.Arguments);
                    }

                    return ArgumentParseResult.Failure("this still needs to be implemented");
                });

        public static ArgumentDefinition ParseArgumentsAs<T>(
            this ArgumentDefinitionBuilder builder,
            ConvertArgument convert,
            ArgumentArity? arity = null) =>
            ParseArgumentsAs(
                builder,
                typeof(T),
                convert,
                arity);

        private static ArgumentDefinition ParseArgumentsAs(
            this ArgumentDefinitionBuilder builder,
            Type type,
            ConvertArgument convert,
            ArgumentArity? arity)
        {
            arity = arity ?? type.DefaultArity();

            if (arity.Value == ArgumentArity.One)
            {
                var originalConvert = convert;
                convert = symbol => {
                    if (symbol.Arguments.Count != 1)
                    {
                        return ArgumentParseResult.Failure(ValidationMessages.SymbolAcceptsOnlyOneArgument(symbol));
                    }

                    return originalConvert(symbol);
                };
            }

            builder.ArgumentArity = arity.Value;

            builder.ConvertArguments = convert;

            return builder.Build();
        }

        public static ArgumentArity DefaultArity(this Type type) =>
            typeof(IEnumerable).IsAssignableFrom(type) &&
            type != typeof(string)
                ? ArgumentArity.Many
                : ArgumentArity.One;

        #endregion

        public static ArgumentDefinitionBuilder WithHelp(
            this ArgumentDefinitionBuilder builder,
            string name = null,
            string description = null)
        {
            builder.Help = new ArgumentsRuleHelp(name, description);

            return builder;
        }

        public static ArgumentDefinitionBuilder WithDefaultValue(
            this ArgumentDefinitionBuilder builder,
            Func<string> defaultValue)
        {
            builder.DefaultValue = defaultValue;

            return builder;
        }

        public static ArgumentDefinitionBuilder AddSuggestions(
            this ArgumentDefinitionBuilder builder,
            params string[] suggestions)
        {
            builder.SuggestionSource.AddSuggestions(suggestions);

            return builder;
        }

        public static ArgumentDefinitionBuilder AddSuggestionSource(
            this ArgumentDefinitionBuilder builder,
            Suggest suggest)
        {
            if (suggest == null)
            {
                throw new ArgumentNullException(nameof(suggest));
            }

            builder.SuggestionSource.AddSuggestionSource(suggest);

            return builder;
        }
    }
}
