// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Generic;
using System.CommandLine.Binding;
using System.CommandLine.Completions;
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
        private CompletionContext? _completionContext;

        internal ParseResult(
            Parser parser,
            RootCommandResult rootCommandResult,
            CommandResult commandResult,
            DirectiveCollection directives,
            TokenizeResult tokenizeResult,
            IReadOnlyList<Token>? unparsedTokens,
            IReadOnlyList<Token>? unmatchedTokens,
            List<ParseError>? errors,
            string? commandLineText = null)
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

            _unparsedTokens = unparsedTokens ?? Array.Empty<Token>();
            _errors = errors ?? new List<ParseError>();
            CommandLineText = commandLineText;

            if (unmatchedTokens is null)
            {
                _unmatchedTokens = Array.Empty<Token>();
            }
            else
            {
                _unmatchedTokens = unmatchedTokens;

                if (parser.Configuration.RootCommand.TreatUnmatchedTokensAsErrors)
                {
                    for (var i = 0; i < _unmatchedTokens.Count; i++)
                    {
                        var token = _unmatchedTokens[i];
                        _errors.Add(new ParseError(parser.Configuration.LocalizationResources.UnrecognizedCommandOrArgument(token.Value), rootCommandResult));
                    }
                }
            }
        }

        internal static ParseResult Empty() => new RootCommand().Parse(Array.Empty<string>());

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
        public IReadOnlyList<ParseError> Errors => _errors;

        /// <summary>
        /// Gets the directives found while parsing command line input.
        /// </summary>
        /// <remarks>If <see cref="CommandLineConfiguration.EnableDirectives"/> is set to <see langword="false"/>, then this collection will be empty.</remarks>
        public DirectiveCollection Directives { get; }

        /// <summary>
        /// Gets the tokens identified while parsing command line input.
        /// </summary>
        public IReadOnlyList<Token> Tokens { get; }

        /// <summary>
        /// Holds the value of a complete command line input prior to splitting and tokenization, when provided.
        /// </summary>
        /// <remarks>This will not be set when the parser is called from <c>Program.Main</c>. It is primarily used when calculating suggestions via the <c>dotnet-suggest</c> tool.</remarks>
        internal string? CommandLineText { get; }

        /// <summary>
        /// Gets the list of tokens used on the command line that were not matched by the parser.
        /// </summary>
        public IReadOnlyList<string> UnmatchedTokens => _unmatchedTokens.Select(t => t.Value).ToArray();

        /// <summary>
        /// Gets the list of tokens used on the command line that were ignored by the parser.
        /// </summary>
        /// <remarks>This list will contain all of the tokens following the first occurrence of a <c>--</c> token if <see cref="CommandLineConfiguration.EnableLegacyDoubleDashBehavior"/> is set to <see langword="true"/>.</remarks>
        public IReadOnlyList<string> UnparsedTokens => _unparsedTokens.Select(t => t.Value).ToArray();

        /// <summary>
        /// Gets the completion context for the parse result.
        /// </summary>
        public CompletionContext GetCompletionContext() =>
            _completionContext ??=
                CommandLineText is null
                    ? new TokenCompletionContext(this)
                    : new TextCompletionContext(this, CommandLineText);

        internal T? GetValueFor<T>(IValueDescriptor<T> symbol) =>
            symbol switch
            {
                Argument<T> argument => GetValueForArgument(argument),
                Option<T> option => GetValueForOption(option),
                _ => throw new ArgumentOutOfRangeException()
            };

        /// <summary>
        /// Gets the parsed or default value for the specified option.
        /// </summary>
        /// <param name="option">The option for which to get a value.</param>
        /// <returns>The parsed value or a configured default.</returns>
        public object? GetValueForOption(Option option) =>
            RootCommandResult.GetValueForOption(option);

        /// <summary>
        /// Gets the parsed or default value for the specified argument.
        /// </summary>
        /// <param name="argument">The argument for which to get a value.</param>
        /// <returns>The parsed value or a configured default.</returns>
        public object? GetValueForArgument(Argument argument) =>
            RootCommandResult.GetValueForArgument(argument);

        /// <inheritdoc cref="GetValueForArgument"/>
        public T GetValueForArgument<T>(Argument<T> argument)
            => RootCommandResult.GetValueForArgument(argument);
        
        /// <inheritdoc cref="GetValueForOption"/>
        public T? GetValueForOption<T>(Option<T> option)
            => RootCommandResult.GetValueForOption(option);

        /// <inheritdoc />
        public override string ToString() => $"{nameof(ParseResult)}: {this.Diagram()}";

        /// <summary>
        /// Gets the result, if any, for the specified argument.
        /// </summary>
        /// <param name="argument">The argument for which to find a result.</param>
        /// <returns>A result for the specified argument, or <see langword="null"/> if it was not provided and no default was configured.</returns>
        public ArgumentResult? FindResultFor(Argument argument) =>
            _rootCommandResult.FindResultFor(argument);

        /// <summary>
        /// Gets the result, if any, for the specified command.
        /// </summary>
        /// <param name="command">The command for which to find a result.</param>
        /// <returns>A result for the specified command, or <see langword="null"/> if it was not provided.</returns>
        public CommandResult? FindResultFor(Command command) =>
            _rootCommandResult.FindResultFor(command);

        /// <summary>
        /// Gets the result, if any, for the specified option.
        /// </summary>
        /// <param name="option">The option for which to find a result.</param>
        /// <returns>A result for the specified option, or <see langword="null"/> if it was not provided and no default was configured.</returns>
        public OptionResult? FindResultFor(Option option) =>
            _rootCommandResult.FindResultFor(option);

        /// <summary>
        /// Gets the result, if any, for the specified symbol.
        /// </summary>
        /// <param name="symbol">The symbol for which to find a result.</param>
        /// <returns>A result for the specified symbol, or <see langword="null"/> if it was not provided and no default was configured.</returns>
        public SymbolResult? FindResultFor(Symbol symbol) =>
            symbol switch
            {
                Argument argument => FindResultFor(argument),
                Command command => FindResultFor(command),
                Option option => FindResultFor(option),
                _ => throw new ArgumentOutOfRangeException(nameof(symbol))
            };

        /// <summary>
        /// Gets completions based on a given parse result.
        /// </summary>
        /// <param name="position">The position at which completions are requested.</param>
        /// <returns>A set of completions for completion.</returns>
        public IEnumerable<CompletionItem> GetCompletions(
            int? position = null)
        {
            var currentSymbolResult = SymbolToComplete(position);

            var currentSymbol = currentSymbolResult.Symbol;

            var context = GetCompletionContext();

            if (position is not null &&
                context is TextCompletionContext tcc)
            {
                context = tcc.AtCursorPosition(position.Value);
            }

            var completions =
                currentSymbol is ICompletionSource currentCompletionSource
                    ? currentCompletionSource.GetCompletions(context)
                    : Array.Empty<CompletionItem>();

            completions =
                completions.Where(item => OptionsWithArgumentLimitReached(currentSymbolResult).All(s => s != item.Label));

            return completions;

            static IEnumerable<string> OptionsWithArgumentLimitReached(SymbolResult symbolResult) =>
                symbolResult
                    .Children
                    .Where(c => c.IsArgumentLimitReached)
                    .OfType<OptionResult>()
                    .Select(o => o.Symbol)
                    .OfType<IdentifierSymbol>()
                    .SelectMany(c => c.Aliases);
        }

        private SymbolResult SymbolToComplete(int? position = null)
        {
            var commandResult = CommandResult;

            var allSymbolResultsForCompletion = AllSymbolResultsForCompletion().ToArray();

            var currentSymbol = allSymbolResultsForCompletion.Last();

            return currentSymbol;

            IEnumerable<SymbolResult> AllSymbolResultsForCompletion()
            {
                foreach (var item in commandResult.AllSymbolResults())
                {
                    if (item is CommandResult command)
                    {
                        yield return command;
                    }
                    else if (item is OptionResult option)
                    {
                        if (WillAcceptAnArgument(this, position, option))
                        {
                            yield return option;
                        }
                    }
                }
            }

            static bool WillAcceptAnArgument(
                ParseResult parseResult,
                int? position,
                OptionResult optionResult)
            {
                if (optionResult.IsImplicit)
                {
                    return false;
                }

                if (!optionResult.IsArgumentLimitReached)
                {
                    return true;
                }

                var completionContext = parseResult.GetCompletionContext();

                if (completionContext is TextCompletionContext textCompletionContext)
                {
                    if (position.HasValue)
                    {
                        textCompletionContext = textCompletionContext.AtCursorPosition(position.Value);
                    }

                    if (textCompletionContext.WordToComplete.Length > 0)
                    {
                        var tokenToComplete = parseResult.Tokens.Last(t => t.Value == textCompletionContext.WordToComplete);

                        return optionResult.Tokens.Contains(tokenToComplete);
                    }
                }

                return !optionResult.IsArgumentLimitReached;
            }
        }
    }
}