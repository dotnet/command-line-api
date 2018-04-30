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

        public void AddValidator(Validate validator)
        {
            if (validator == null)
            {
                throw new ArgumentNullException(nameof(validator));
            }

            validators.Add(validator);
        }

        public ArgumentsRuleHelp Help { get; set; }

        public Func<string> DefaultValue { get; set; }

        public TypeConversion TypeConversion { get; set; }

        protected virtual ArgumentParser GetArgumentParser()
            => new ArgumentParser<string>(TypeConversion ?? (symbol => Result.Success(symbol.Token)));

        public ArgumentsRule Build()
        {
            return new ArgumentsRule(GetArgumentParser(), DefaultValue, Help);
        }
    }

    public class ArgumentRuleBuilder<T> : ArgumentRuleBuilder
    {
        public ArgumentRuleBuilder()
            : this(FigureMeOut())
        { }

        private static TypeConversion FigureMeOut()
        {
            //TODO: Jump table
            if (typeof(T) == typeof(string))
            {
                return symbol => Result.Success(symbol.Token);
            }
            throw new NotImplementedException();
        }

        public ArgumentRuleBuilder(TypeConversion typeConversion)
        {
            ArgumentParser = new ArgumentParser<T>(typeConversion);
        }

        protected override ArgumentParser GetArgumentParser()
            => ArgumentParser;

        public ArgumentParser<T> ArgumentParser { get; set; }
    }

    public static class Define
    {
        public static ArgumentRuleBuilder Arguments()
        {
            return new ArgumentRuleBuilder();
        }

        #region arity

        public static ArgumentsRule ExactlyOne<T>(
            this ArgumentRuleBuilder<T> builder,
            Func<ParsedSymbol, string> errorMessage = null)
        {
            builder.ArgumentParser.AddValidator((value, parsedSymbol) =>
            {
                var argumentCount = parsedSymbol.Arguments.Count;

                if (argumentCount == 0)
                {
                    if (errorMessage == null)
                    {
                        return Result.Failure(parsedSymbol.Symbol is Command
                            ? RequiredArgumentMissingForCommand(parsedSymbol.Symbol.ToString())
                            : RequiredArgumentMissingForOption(parsedSymbol.Symbol.ToString()));
                    }
                    return Result.Failure(errorMessage(parsedSymbol));
                }

                if (argumentCount > 1)
                {
                    if (errorMessage == null)
                    {
                        return Result.Failure(parsedSymbol.Symbol is Command
                            ? CommandAcceptsOnlyOneArgument(parsedSymbol.Symbol.ToString(), argumentCount)
                            : OptionAcceptsOnlyOneArgument(parsedSymbol.Symbol.ToString(), argumentCount));
                    }

                    return Result.Failure(errorMessage(parsedSymbol));
                }

                return Result.Success(value);
            });

            return builder.Build();
        }

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
                                   ? RequiredArgumentMissingForCommand(parsedSymbol.Symbol.ToString())
                                   : RequiredArgumentMissingForOption(parsedSymbol.Symbol.ToString());
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
                                   ? CommandAcceptsOnlyOneArgument(parsedSymbol.Symbol.ToString(), argumentCount)
                                   : OptionAcceptsOnlyOneArgument(parsedSymbol.Symbol.ToString(), argumentCount);
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
            builder.AddValidator(o =>
            {
                if (o.Arguments.Count > 1)
                {
                    return o.Symbol is Command
                               ? CommandAcceptsOnlyOneArgument(o.Symbol.ToString(), o.Arguments.Count)
                               : OptionAcceptsOnlyOneArgument(o.Symbol.ToString(), o.Arguments.Count);
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
                    o.Symbol is Command
                        ? RequiredArgumentMissingForCommand(o.Symbol.ToString())
                        : RequiredArgumentMissingForOption(o.Symbol.ToString());
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
            TypeConversion parse)
        {
            //builder.ArgumentParser = new ArgumentParser<T>(parse);

            return builder;
        }

        #endregion

        public static ArgumentRuleBuilder WithHelp(this ArgumentRuleBuilder builder,
            string name = null, string description = null)
        {
            builder.Help = new ArgumentsRuleHelp(name, description);
            return builder;
        }

        public static ArgumentRuleBuilder WithDefaultValue(this ArgumentRuleBuilder builder,
            Func<string> defaultValue)
        {
            builder.DefaultValue = defaultValue;
            return builder;
        }

        public static ArgumentRuleBuilder Validate(
            this ArgumentRuleBuilder builder,
            Validate validate)
        {
            builder.AddValidator(validate);

            return builder;
        }

        public static ArgumentRuleBuilder WithSuggestions(this ArgumentRuleBuilder builder,
            params string[] suggestions)
        {
            return builder;
        }
    }

    public delegate Result TypeConverter(ParsedSymbol symbol);

    public delegate IEnumerable<string> SuggestionSource(ParseResult parseResult, int? position);

    public abstract class ArgumentParser
    {
        private readonly List<SuggestionSource> suggestionSources = new List<SuggestionSource>();

        public void AddSuggetions(SuggestionSource suggestionSource)
        {
            suggestionSources.Add(suggestionSource);
        }

        public virtual IEnumerable<string> Suggest(
            ParseResult parseResult,
            int? position = null)
        {
            foreach (SuggestionSource suggestionSource in suggestionSources)
            {
                foreach (string suggestion in suggestionSource(parseResult, position))
                {
                    yield return suggestion;
                }
            }
        }


        public abstract Result Parse(ParsedSymbol value);
    }

    public delegate Result Validate<in T>(T value, ParsedSymbol symbol);

    public delegate Result TypeConversion(ParsedSymbol symbol);

    public class ArgumentParser<T> : ArgumentParser
    {
        private readonly List<Validate<T>> validations = new List<Validate<T>>();
        private readonly TypeConversion typeConversion;

        public ArgumentParser()
        {

        }

        public ArgumentParser(TypeConversion typeConversion)
        {
            this.typeConversion = typeConversion ??
                         throw new ArgumentNullException(nameof(typeConversion));
        }

        public void AddValidator(Validate<T> validator)
        {
            validations.Add(validator);
        }

        private Result Validate(T value, ParsedSymbol symbol)
        {
            Result result = null;
            foreach (Validate<T> validator in validations)
            {
                result = validator(value, symbol);
                if (result is SuccessfulResult<T> successResult)
                {
                    value = successResult.Value;
                }
                else
                {
                    return result;
                }
            }
            return result ?? Result.Success(value);
        }

        //string -> parsed symbol -> type conversion -> (type checking) -> validation

        public override Result Parse(ParsedSymbol symbol)
        {
            Result typeResult = typeConversion(symbol);
            if (typeResult is SuccessfulResult<T> successfulResult)
            {
                return Validate(successfulResult.Value, symbol);
            }
            return typeResult;
        }
    }

    public class SuccessfulResult<T> : Result
    {
        public SuccessfulResult(T value = default(T))
        {
            Value = value;
        }

        public T Value { get; }

        public override bool Successful { get; } = true;
    }

    public class FailedResult : Result
    {
        public string Error { get; }

        public FailedResult(string error)
        {
            Error = error;
        }

        public override bool Successful { get; } = false;
    }

    public abstract class Result
    {
        public abstract bool Successful { get; }

        public static FailedResult Failure(string error) => new FailedResult(error);

        public static SuccessfulResult<T> Success<T>(T value) => new SuccessfulResult<T>(value);
    }
}
