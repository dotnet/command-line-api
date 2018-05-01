// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Microsoft.DotNet.Cli.CommandLine
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
                        return parsedSymbol.Symbol is Command
                                   ? ValidationMessages.CommandAcceptsOnlyOneArgument(parsedSymbol.Symbol.ToString(), argumentCount)
                                   : ValidationMessages.OptionAcceptsOnlyOneArgument(parsedSymbol.Symbol.ToString(), argumentCount);
                    }
                    else
                    {
                        return errorMessage(parsedSymbol);
                    }
                }

                return null;
            });

            return builder.Build();
        }

        public static ArgumentsRule ZeroOrMore(
            this ArgumentRuleBuilder builder,
            Func<ParsedOption, string> errorMessage = null)
        {
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
            return builder.Build();
        }

        #endregion

        #region set inclusion

        public static ArgumentRuleBuilder FromAmong(
            this ArgumentRuleBuilder builder,
            params string[] values)
        {
            builder.AddValidator(parsedSymbol =>
            {
                if (parsedSymbol.Arguments.Count == 0)
                {
                    return null;
                }

                foreach (var arg in parsedSymbol.Arguments)
                {
                    // TODO: Is case-insensitive really what we want here?
                    if (!values.Any(value => string.Equals(arg, value, StringComparison.OrdinalIgnoreCase)))
                    {
                        return ValidationMessages.UnrecognizedArgument(arg, values);
                    }
                }

                return null;
            });

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

        #region type

        public static ArgumentRuleBuilder OfType<T>(
            this ArgumentRuleBuilder builder,
            ConvertArgument parse)
        {
            //builder.ArgumentParser = new ArgumentParser<T>(parse);

            return builder;
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

        public static ArgumentRuleBuilder WithSuggestions(
            this ArgumentRuleBuilder builder,
            params string[] suggestions)
        {
            return builder;
        }

        public static ArgumentRuleBuilder WithSuggestions(
            this ArgumentRuleBuilder builder,
            Func<string, IEnumerable<string>> suggest)
        {
            return builder;
        }
    }
}
