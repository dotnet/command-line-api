﻿// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Generic;
using System.CommandLine.Binding;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

namespace System.CommandLine.Parsing
{
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
                    _errors.Add(new ParseError(parser.Configuration.Resources.UnrecognizedCommandOrArgument(token.Value)));
                }
            }
        }

        public CommandResult CommandResult { get; }

        public Parser Parser { get; }

        public CommandResult RootCommandResult => _rootCommandResult;

        public IReadOnlyCollection<ParseError> Errors => _errors;

        public IDirectiveCollection Directives { get; }

        public IReadOnlyList<Token> Tokens { get; }

        internal string? RawInput { get; }

        public IReadOnlyList<string> UnmatchedTokens => _unmatchedTokens.Select(t => t.Value).ToArray();

        public IReadOnlyList<string> UnparsedTokens => _unparsedTokens.Select(t => t.Value).ToArray();

        [return: MaybeNull]
        internal T GetValueFor<T>(IValueDescriptor<T> symbol) =>
            symbol switch
            {
                Argument<T> argument => GetValueForArgument(argument),
                Option<T> option => GetValueForOption(option),
                _ => throw new ArgumentOutOfRangeException()
            };

        [Obsolete("This method is obsolete and will be removed in a future version. Please use ParseResult.GetValueForOption<T>(Option<T>) instead. For details see https://github.com/dotnet/command-line-api/issues/1127")]
        public object? ValueForOption(string alias) =>
            ValueForOption<object?>(alias);
        
        [Obsolete("This method is obsolete and will be removed in a future version. Please use ParseResult.GetValueForOption<T>(IOption) instead. For details see https://github.com/dotnet/command-line-api/issues/1127")]
        public object? ValueForOption(Option option) =>
            GetValueForOption<object?>(option);

        public object? GetValueForOption(Option option) =>
            GetValueForOption<object?>(option);

        [Obsolete("This method is obsolete and will be removed in a future version. Please use ParseResult.GetValueForArgument<T>(Argument<T>) instead. For details see https://github.com/dotnet/command-line-api/issues/1127")]
        public object? ValueForArgument(string alias) =>
            ValueForArgument<object?>(alias);

        [Obsolete("This method is obsolete and will be removed in a future version. Please use ParseResult.GetValueForArgument<T>(Argument) instead. For details see https://github.com/dotnet/command-line-api/issues/1127")]
         public object? ValueForArgument(Argument argument) =>
            GetValueForArgument<object?>(argument);

         public object? GetValueForArgument(IArgument argument) =>
            GetValueForArgument<object?>(argument);

         [Obsolete(
             "This method is obsolete and will be removed in a future version. Please use ParseResult.GetValueForArgument<T>(Argument<T>) instead. For details see https://github.com/dotnet/command-line-api/issues/1127")]
         [return: MaybeNull]
         public T ValueForArgument<T>(Argument<T> argument) => 
             GetValueForArgument(argument);
       
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

        [Obsolete(
            "This method is obsolete and will be removed in a future version. Please use ParseResult.GetValueForArgument<T>(IArgument) instead. For details see https://github.com/dotnet/command-line-api/issues/1127")]
        [return: MaybeNull]
        internal T ValueForArgument<T>(Argument argument) => 
            GetValueForArgument<T>(argument);
        
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

        [return: MaybeNull]
        [Obsolete("This method is obsolete and will be removed in a future version. Please use ParseResult.GetValueForArgument<T>(Option<T>) instead. For details see https://github.com/dotnet/command-line-api/issues/1127")]
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

        [Obsolete("This method is obsolete and will be removed in a future version. Please use ParseResult.GetValueForOption<T>(Option<T>) instead. For details see https://github.com/dotnet/command-line-api/issues/1127")]
        [return: MaybeNull]
        public T ValueForOption<T>(Option<T> option) => 
            GetValueForOption(option);

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

        [return: MaybeNull]
        public T GetValueForOption<T>(IOption option)
        {
            if (FindResultFor(option) is { } result &&
                result.GetValueOrDefault<T>() is { } t)
            {
                return t;
            }

            return (T)Binder.GetDefaultValue(option.Argument.ValueType)!;
        }

        [Obsolete("This method is obsolete and will be removed in a future version. Please use ParseResult.GetValueForOption<T>(Option<T>) instead. For details see https://github.com/dotnet/command-line-api/issues/1127")]
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

        public ArgumentResult? FindResultFor(IArgument argument) =>
            _rootCommandResult.FindResultFor(argument);

        public CommandResult? FindResultFor(ICommand command) =>
            _rootCommandResult.FindResultFor(command);

        public OptionResult? FindResultFor(IOption option) =>
            _rootCommandResult.FindResultFor(option);

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
