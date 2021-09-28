// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Generic;
using System.CommandLine.Binding;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

namespace System.CommandLine.Parsing
{
    /// <summary>
    /// Describes the results of parsing a command line input based on a specific parser configuration.
    /// </summary>
    public class ParseResult
    {
        private readonly List<ParseError> _errors;
        private readonly RootCommandResult _rootCommandResult;
        private readonly IReadOnlyList<Token> _unparsedTokens;
        private readonly IReadOnlyList<Token> _unmatchedTokens;

        internal ParseResult(
            Parser parser,
            RootCommandResult rootCommandResult,
            CommandResult commandResult,
            IDirectiveCollection directives,
            TokenizeResult tokenizeResult,
            IReadOnlyList<Token> unparsedTokens,
            IReadOnlyList<Token> unmatchedTokens,
            List<ParseError>? errors = null,
            string? rawInput = null)
        {
            Parser = parser;
            _rootCommandResult = rootCommandResult;
            CommandResult = commandResult;
            Directives = directives;

            // skip the root command when populating Tokens property
            if (tokenizeResult.Tokens.Count > 1)
            {
                var tokens = new Token[tokenizeResult.Tokens.Count - 1];
                for (var i = 0; i < tokenizeResult.Tokens.Count - 1; i++)
                {
                    var token = tokenizeResult.Tokens[i + 1];
                    tokens[i] = token;
                }

                Tokens = tokens;
            }
            else
            {
                Tokens = Array.Empty<Token>();
            }

            _unparsedTokens = unparsedTokens;
            _unmatchedTokens = unmatchedTokens;

            RawInput = rawInput;

            _errors = errors ?? (parser.Configuration.RootCommand.TreatUnmatchedTokensAsErrors
                                     ? new List<ParseError>(unmatchedTokens.Count)
                                     : new List<ParseError>());

            if (parser.Configuration.RootCommand.TreatUnmatchedTokensAsErrors)
            {
                for (var i = 0; i < unmatchedTokens.Count; i++)
                {
                    var token = unmatchedTokens[i];
                    _errors.Add(new ParseError(parser.Configuration.Resources.UnrecognizedCommandOrArgument(token.Value), rootCommandResult));
                }
            }
        }

        /// <summary>
        /// A result indicating the command specified in the command line input.
        /// </summary>
        public CommandResult CommandResult { get; }

        /// <summary>
        /// The parser used to produce the parse result.
        /// </summary>
        public Parser Parser { get; }

        /// <summary>
        /// Gets the root command result.
        /// </summary>
        public CommandResult RootCommandResult => _rootCommandResult;

        /// <summary>
        /// Gets the parse errors found while parsing command line input.
        /// </summary>
        public IReadOnlyCollection<ParseError> Errors => _errors;

        /// <summary>
        /// Gets the directives found while parsing command line input.
        /// </summary>
        /// <remarks>If <see cref="CommandLineConfiguration.EnableDirectives"/> is set to <see langword="false"/>, then this collection will be empty.</remarks>
        public IDirectiveCollection Directives { get; }

        /// <summary>
        /// Gets the tokens identified while parsing command line input.
        /// </summary>
        public IReadOnlyList<Token> Tokens { get; }

        /// <summary>
        /// Holds the value of a complete command line input prior to splitting and tokenization, when provided.
        /// </summary>
        /// <remarks>This will not be set when the parser is called from <c>Program.Main</c>. It is primarily used when calculating suggestions via the <c>dotnet-suggest</c> tool.</remarks>
        internal string? RawInput { get; }

        /// <summary>
        /// Gets the list of tokens used on the command line that were not matched by the parser.
        /// </summary>
        public IReadOnlyList<string> UnmatchedTokens => _unmatchedTokens.Select(t => t.Value).ToArray();

        /// <summary>
        /// Gets the list of tokens used on the command line that were ignored by the parser.
        /// </summary>
        /// <remarks>This list will contain all of the tokens following the first occurrence of a <c>--</c> token if <see cref="CommandLineConfiguration.EnableLegacyDoubleDashBehavior"/> is set to <see langword="true"/>.</remarks>
        public IReadOnlyList<string> UnparsedTokens => _unparsedTokens.Select(t => t.Value).ToArray();

        [return: MaybeNull]
        internal T GetValueFor<T>(IValueDescriptor<T> symbol) =>
            symbol switch
            {
                Argument<T> argument => GetValueForArgument(argument),
                Option<T> option => GetValueForOption(option),
                _ => throw new ArgumentOutOfRangeException()
            };

        /// <inheritdoc cref="GetValueForOption"/>
        [Obsolete(
            "This method is obsolete and will be removed in a future version. Please use ParseResult.GetValueForOption<T>(Option<T>) instead. For details see https://github.com/dotnet/command-line-api/issues/1127")]
        public object? ValueForOption(string alias) =>
            ValueForOption<object?>(alias);

        /// <inheritdoc cref="GetValueForOption"/>
        [Obsolete(
            "This method is obsolete and will be removed in a future version. Please use ParseResult.GetValueForOption<T>(IOption) instead. For details see https://github.com/dotnet/command-line-api/issues/1127")]
        public object? ValueForOption(Option option) =>
            GetValueForOption<object?>(option);

        /// <summary>
        /// Gets the parsed or default value for the specified option.
        /// </summary>
        /// <param name="option">The option for which to get a value.</param>
        /// <returns>The parsed value or a configured default.</returns>
        public object? GetValueForOption(Option option) =>
            GetValueForOption<object?>(option);

        /// <inheritdoc cref="GetValueForArgument"/>
        [Obsolete(
            "This method is obsolete and will be removed in a future version. Please use ParseResult.GetValueForArgument<T>(Argument<T>) instead. For details see https://github.com/dotnet/command-line-api/issues/1127")]
        public object? ValueForArgument(string alias) =>
            ValueForArgument<object?>(alias);

        /// <inheritdoc cref="GetValueForArgument"/>
        [Obsolete(
            "This method is obsolete and will be removed in a future version. Please use ParseResult.GetValueForArgument<T>(Argument) instead. For details see https://github.com/dotnet/command-line-api/issues/1127")]
        public object? ValueForArgument(Argument argument) =>
            GetValueForArgument<object?>(argument);

        /// <summary>
        /// Gets the parsed or default value for the specified argument.
        /// </summary>
        /// <param name="argument">The argument for which to get a value.</param>
        /// <returns>The parsed value or a configured default.</returns>
        public object? GetValueForArgument(IArgument argument) =>
            GetValueForArgument<object?>(argument);

        /// <inheritdoc cref="GetValueForArgument"/>
        [Obsolete(
            "This method is obsolete and will be removed in a future version. Please use ParseResult.GetValueForArgument<T>(Argument<T>) instead. For details see https://github.com/dotnet/command-line-api/issues/1127")]
        [return: MaybeNull]
        public T ValueForArgument<T>(Argument<T> argument) =>
            GetValueForArgument(argument);

        /// <inheritdoc cref="GetValueForArgument"/>
        [return: MaybeNull]
        public T GetValueForArgument<T>(Argument<T> argument)
        {
            if (FindResultFor(argument) is { } result &&
                result.GetValueOrDefault<T>() is { } t)
            {
                return t;
            }

            return (T)Binder.GetDefaultValue(argument.ValueType)!;
        }

        /// <inheritdoc cref="GetValueForArgument"/>
        [Obsolete(
            "This method is obsolete and will be removed in a future version. Please use ParseResult.GetValueForArgument<T>(IArgument) instead. For details see https://github.com/dotnet/command-line-api/issues/1127")]
        [return: MaybeNull]
        internal T ValueForArgument<T>(Argument argument) =>
            GetValueForArgument<T>(argument);

        /// <inheritdoc cref="GetValueForArgument"/>
        [return: MaybeNull]
        public T GetValueForArgument<T>(IArgument argument)
        {
            if (FindResultFor(argument) is { } result &&
                result.GetValueOrDefault<T>() is { } t)
            {
                return t;
            }

            return (T)Binder.GetDefaultValue(argument.ValueType)!;
        }

        /// <inheritdoc cref="GetValueForArgument"/>
        [return: MaybeNull]
        [Obsolete(
            "This method is obsolete and will be removed in a future version. Please use ParseResult.GetValueForArgument<T>(Option<T>) instead. For details see https://github.com/dotnet/command-line-api/issues/1127")]
        public T ValueForArgument<T>(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                throw new ArgumentException("Value cannot be null or whitespace.", nameof(name));
            }

            if (CommandResult.Children.GetByAlias(name) is ArgumentResult argumentResult)
            {
                return argumentResult.GetValueOrDefault<T>();
            }
            else
            {
                return default;
            }
        }

        /// <inheritdoc cref="GetValueForOption"/>
        [Obsolete(
            "This method is obsolete and will be removed in a future version. Please use ParseResult.GetValueForOption<T>(Option<T>) instead. For details see https://github.com/dotnet/command-line-api/issues/1127")]
        [return: MaybeNull]
        public T ValueForOption<T>(Option<T> option) =>
            GetValueForOption(option);

        /// <inheritdoc cref="GetValueForOption"/>
        [return: MaybeNull]
        public T GetValueForOption<T>(Option<T> option)
        {
            if (FindResultFor(option) is { } result &&
                result.GetValueOrDefault<T>() is { } t)
            {
                return t;
            }

            return (T)Binder.GetDefaultValue(option.Argument.ValueType)!;
        }

        /// <inheritdoc cref="GetValueForOption"/>
        [return: MaybeNull]
        public T GetValueForOption<T>(IOption option)
        {
            if (FindResultFor(option) is { } result)
            {
                if (result.GetValueOrDefault<T>() is { } t)
                {
                    return t;
                }
            }

            return (T)Binder.GetDefaultValue(option.Argument.ValueType)!;
        }

        /// <inheritdoc cref="GetValueForOption"/>
        [Obsolete(
            "This method is obsolete and will be removed in a future version. Please use ParseResult.GetValueForOption<T>(Option<T>) instead. For details see https://github.com/dotnet/command-line-api/issues/1127")]
        [return: MaybeNull]
        public T ValueForOption<T>(string alias)
        {
            if (string.IsNullOrWhiteSpace(alias))
            {
                throw new ArgumentException("Value cannot be null or whitespace.", nameof(alias));
            }

            if (CommandResult.Children.GetByAlias(alias) is OptionResult optionResult)
            {
                return optionResult.GetValueOrDefault<T>();
            }
            else
            {
                return default;
            }
        }

        /// <inheritdoc />
        public override string ToString() => $"{nameof(ParseResult)}: {this.Diagram()}";

        /// <summary>
        /// Gets the result, if any, for the specified argument.
        /// </summary>
        /// <param name="argument">The argument for which to find a result.</param>
        /// <returns>A result for the specified argument, or <see langword="null"/> if it was not provided and no default was configured.</returns>
        public ArgumentResult? FindResultFor(IArgument argument) =>
            _rootCommandResult.FindResultFor(argument);

        /// <summary>
        /// Gets the result, if any, for the specified command.
        /// </summary>
        /// <param name="command">The command for which to find a result.</param>
        /// <returns>A result for the specified command, or <see langword="null"/> if it was not provided.</returns>
        public CommandResult? FindResultFor(ICommand command) =>
            _rootCommandResult.FindResultFor(command);

        /// <summary>
        /// Gets the result, if any, for the specified option.
        /// </summary>
        /// <param name="option">The option for which to find a result.</param>
        /// <returns>A result for the specified option, or <see langword="null"/> if it was not provided and no default was configured.</returns>
        public OptionResult? FindResultFor(IOption option) =>
            _rootCommandResult.FindResultFor(option);

        /// <summary>
        /// Gets the result, if any, for the specified symbol.
        /// </summary>
        /// <param name="symbol">The symbol for which to find a result.</param>
        /// <returns>A result for the specified symbol, or <see langword="null"/> if it was not provided and no default was configured.</returns>
        public SymbolResult? FindResultFor(ISymbol symbol) =>
            symbol switch
            {
                IArgument argument => FindResultFor(argument),
                ICommand command => FindResultFor(command),
                IOption option => FindResultFor(option),
                _ => throw new ArgumentOutOfRangeException(nameof(symbol))
            };
    }
}