// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using static Microsoft.DotNet.Cli.CommandLine.ValidationMessages;

namespace Microsoft.DotNet.Cli.CommandLine
{
    public class ArgumentRuleBuilder
    {
        private readonly List<Validate> validators = new List<Validate>();

        public ArgumentParser ArgumentParser { get; set; }

        public void AddValidator(Validate validator)
        {
            if (validator == null)
            {
                throw new ArgumentNullException(nameof(validator));
            }

            validators.Add(validator);
        }

        private string Validate(ParsedSymbol parsedOption)
        {
            if (parsedOption == null)
            {
                throw new ArgumentNullException(nameof(parsedOption));
            }

            return validators.Select(v => v(parsedOption))
                             .FirstOrDefault(e => e != null);
        }

        internal ArgumentsRule Build()
        {
            return new ArgumentsRule(Validate);
        }
    }

    public static class Define
    {
        public static ArgumentRuleBuilder Arguments()
        {
            return new ArgumentRuleBuilder();
        }

        #region arity

        public static ArgumentsRule None(
            this ArgumentRuleBuilder builder,
            Func<ParsedSymbol, string> errorMessage = null)
        {
            builder.AddValidator(o =>
            {
                if (!o.Arguments.Any())
                {
                    return null;
                }

                if (errorMessage == null)
                {
                    return NoArgumentsAllowed(o.Option.ToString());
                }
                else
                {
                    return errorMessage(o);
                }
            });

            return builder.Build();
        }

        public static ArgumentsRule ExactlyOne(
            this ArgumentRuleBuilder builder,
            Func<ParsedSymbol, string> errorMessage = null)
        {
            builder.AddValidator(o =>
            {
                var argumentCount = o.Arguments.Count;

                if (argumentCount == 0)
                {
                    if (errorMessage == null)
                    {
                        return o.Option.IsCommand
                                   ? RequiredArgumentMissingForCommand(o.Option.ToString())
                                   : RequiredArgumentMissingForOption(o.Option.ToString());
                    }
                    else
                    {
                        return errorMessage(o);
                    }
                }

                if (argumentCount > 1)
                {
                    if (errorMessage == null)
                    {
                        return o.Option.IsCommand
                                   ? CommandAcceptsOnlyOneArgument(o.Option.ToString(), argumentCount)
                                   : OptionAcceptsOnlyOneArgument(o.Option.ToString(), argumentCount);
                    }
                    else
                    {
                        return errorMessage(o);
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
            builder.AddValidator(o =>
            {
                if (o.Arguments.Count > 1)
                {
                    return o.Option.IsCommand
                               ? CommandAcceptsOnlyOneArgument(o.Option.ToString(), o.Arguments.Count)
                               : OptionAcceptsOnlyOneArgument(o.Option.ToString(), o.Arguments.Count);
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

                return
                    o.Option.IsCommand
                        ? RequiredArgumentMissingForCommand(o.Option.ToString())
                        : RequiredArgumentMissingForOption(o.Option.ToString());
            });
            return builder.Build();
        }

        #endregion

        #region set inclusion

        public static ArgumentRuleBuilder FromAmong(
            this ArgumentRuleBuilder builder,
            params string[] values)
        {
            builder.AddValidator(o =>
            {
                if (o.Arguments.Count == 0)
                {
                    return null;
                }

                var arg = o.Arguments.Single();

                return !values.Contains(arg, StringComparer.OrdinalIgnoreCase)
                           ? UnrecognizedArgument(arg, values)
                           : "";
            });

            return builder;
        }

        #endregion

        #region files

        public static ArgumentRuleBuilder ExistingFilesOnly(
            this ArgumentRuleBuilder builder)
        {
            builder.AddValidator(o => o.Arguments
                                       .Where(filePath => !File.Exists(filePath) &&
                                                          !Directory.Exists(filePath))
                                       .Select(FileDoesNotExist)
                                       .FirstOrDefault());
            return builder;
        }

        public static ArgumentRuleBuilder LegalFilePathsOnly(
            this ArgumentRuleBuilder builder)
        {
            builder.AddValidator(o =>
            {
                foreach (var arg in o.Arguments)
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
            ParseArgument<T> parse)
        {
            builder.ArgumentParser = new ArgumentParser<T>(parse);

            return builder;
        }

        #endregion

        public static ArgumentRuleBuilder Validate(
            this ArgumentRuleBuilder builder,
            Validate validate)
        {
            builder.AddValidator(validate);

            return builder;
        }
    }

    public delegate ArgumentParser<T>.Result ParseArgument<T>(string value);

    public class ArgumentParser
    {
    }

    public class ArgumentParser<T> : ArgumentParser
    {
        private readonly ParseArgument<T> parse;

        public ArgumentParser(ParseArgument<T> parse)
        {
            this.parse = parse ??
                         throw new ArgumentNullException(nameof(parse));
        }

        public Result TryParse(string value) => parse(value);

        public static Result Failure { get; } = new Result(false);

        public static Result Success(T value) => new Result(true, value);

        public struct Result
        {
            private readonly T value;

            public Result(bool successful, T value = default(T))
            {
                Successful = successful;
                this.value = value;
            }

            public bool Successful { get; }

            public T Value
            {
                get
                {
                    if (!Successful)
                    {
                        throw new InvalidOperationException($"Value \"{value}\" could not be parsed as a {typeof(T)}");
                    }

                    return value;
                }
            }
        }
    }
}
