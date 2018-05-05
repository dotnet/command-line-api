// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace System.CommandLine
{
    public static class Define
    {
        public static ArgumentRuleBuilder Arguments()
        {
            return new ArgumentRuleBuilder();
        }

        #region arity

        public static ArgumentsRule ExactlyOne(
            this ArgumentRuleBuilder builder,
            Func<ParsedSymbol, string> errorMessage = null)
        {
            builder.AddValidator(parsedSymbol =>
            {
                var argumentCount = parsedSymbol.Arguments.Count;

                if (argumentCount == 0)
                {
                    if (errorMessage == null)
                    {
                        return parsedSymbol.Symbol is Command
                                   ? ValidationMessages.RequiredArgumentMissingForCommand(parsedSymbol.Symbol.ToString())
                                   : ValidationMessages.RequiredArgumentMissingForOption(parsedSymbol.Symbol.ToString());
                    }
                    else
                    {
                        return errorMessage(parsedSymbol);
                    }
                }

                if (argumentCount > 1)
                {
                    if (errorMessage == null)
                    {
                        return ValidationMessages.SymbolAcceptsOnlyOneArgument(parsedSymbol);
                    }
                    else
                    {
                        return errorMessage(parsedSymbol);
                    }
                }

                return null;
            });

            builder.ArgumentArity = ArgumentArity.One;

            return builder.Build();
        }

        public static ArgumentsRule ZeroOrMore(
            this ArgumentRuleBuilder builder,
            Func<ParsedOption, string> errorMessage = null)
        {
            builder.ArgumentArity = ArgumentArity.Many;

            return builder.Build();
        }

        public static ArgumentsRule ZeroOrOne(
            this ArgumentRuleBuilder builder,
            Func<ParsedOption, string> errorMessage = null)
        {
            builder.AddValidator(parsedSymbol =>
            {
                if (parsedSymbol.Arguments.Count > 1)
                {
                    return parsedSymbol.Symbol is Command
                               ? ValidationMessages.CommandAcceptsOnlyOneArgument(parsedSymbol.Symbol.ToString(), parsedSymbol.Arguments.Count)
                               : ValidationMessages.OptionAcceptsOnlyOneArgument(parsedSymbol.Symbol.ToString(), parsedSymbol.Arguments.Count);
                }

                return null;
            });

            builder.ArgumentArity = ArgumentArity.One;

            return builder.Build();
        }

        public static ArgumentsRule OneOrMore(
            this ArgumentRuleBuilder builder,
            Func<ParsedSymbol, string> errorMessage = null)
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

                return o.Symbol is Command
                           ? ValidationMessages.RequiredArgumentMissingForCommand(o.Symbol.ToString())
                           : ValidationMessages.RequiredArgumentMissingForOption(o.Symbol.ToString());
            });

            builder.ArgumentArity = ArgumentArity.Many;

            return builder.Build();
        }

        #endregion

        #region set inclusion

        public static ArgumentRuleBuilder FromAmong(
            this ArgumentRuleBuilder builder,
            params string[] values)
        {
            builder.ValidTokens.UnionWith(values);

            builder.SuggestionSource.AddSuggestions(values);

            return builder;
        }

        #endregion

        #region files

        public static ArgumentRuleBuilder ExistingFilesOnly(
            this ArgumentRuleBuilder builder)
        {
            builder.AddValidator(parsedSymbol =>
            {
                return parsedSymbol.Arguments
                                   .Where(filePath => !File.Exists(filePath) &&
                                                      !Directory.Exists(filePath))
                                   .Select(ValidationMessages.FileDoesNotExist)
                                   .FirstOrDefault();
            });
            return builder;
        }

        public static ArgumentRuleBuilder LegalFilePathsOnly(
            this ArgumentRuleBuilder builder)
        {
            builder.AddValidator(parsedSymbol =>
            {
                foreach (var arg in parsedSymbol.Arguments)
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

        public static ArgumentsRule ParseAs<T>(
            this ArgumentRuleBuilder builder,
            ConvertArgument convert,
            ArgumentArity? arity = null)
        {
            if (arity == null)
            {
                if (typeof(IEnumerable).IsAssignableFrom(typeof(T)) && 
                    typeof(T) != typeof(string))
                {
                    arity = ArgumentArity.Many;
                }
                else
                {
                    arity = ArgumentArity.One;

                    var originalConvert = convert;
                    convert = symbol =>
                    {
                        if (symbol.Arguments.Count != 1)
                        {
                           return ArgumentParseResult.Failure(ValidationMessages.SymbolAcceptsOnlyOneArgument(symbol));
                        }

                        return originalConvert(symbol);
                    };
                }
            }

            builder.ArgumentArity = arity.Value;

            builder.ConvertArguments = convert;

            return builder.Build();
        }

        #endregion

        public static ArgumentRuleBuilder WithHelp(
            this ArgumentRuleBuilder builder,
            string name = null,
            string description = null)
        {
            builder.Help = new ArgumentsRuleHelp(name, description);

            return builder;
        }

        public static ArgumentRuleBuilder WithDefaultValue(
            this ArgumentRuleBuilder builder,
            Func<string> defaultValue)
        {
            builder.DefaultValue = defaultValue;

            return builder;
        }

        public static ArgumentRuleBuilder AddSuggestions(
            this ArgumentRuleBuilder builder,
            params string[] suggestions)
        {
            builder.SuggestionSource.AddSuggestions(suggestions);

            return builder;
        }

        public static ArgumentRuleBuilder AddSuggestionSource(
            this ArgumentRuleBuilder builder,
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
