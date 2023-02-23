// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Generic;
using System.CommandLine.Binding;
using System.CommandLine.Completions;
using System.CommandLine.Invocation;
using System.CommandLine.Parsing;
using System.Linq;
using System.Threading.Tasks;
using System.Threading;

namespace System.CommandLine
{
    /// <summary>
    /// Describes the results of parsing a command line input based on a specific parser configuration.
    /// </summary>
    public class ParseResult
    {
        private readonly IReadOnlyList<ParseError> _errors;
        private readonly CommandResult _rootCommandResult;
        private readonly IReadOnlyList<Token> _unmatchedTokens;
        private CompletionContext? _completionContext;
        private ICommandHandler? _handler;
        private bool _useChainedHandler;

        internal ParseResult(
            CommandLineConfiguration configuration,
            CommandResult rootCommandResult,
            CommandResult commandResult,
            List<Token> tokens,
            IReadOnlyList<Token>? unmatchedTokens,
            List<ParseError>? errors,
            string? commandLineText,
            ICommandHandler? handler,
            bool useChainedHandler)
        {
            Configuration = configuration;
            _rootCommandResult = rootCommandResult;
            CommandResult = commandResult;
            _handler = handler;
            _useChainedHandler = useChainedHandler;

            // skip the root command when populating Tokens property
            if (tokens.Count > 1)
            {
                // Since TokenizeResult.Tokens is not public and not used anywhere after the parsing,
                // we take advantage of its mutability and remove the root command token
                // instead of creating a copy of the whole list.
                tokens.RemoveAt(0);
                Tokens = tokens;
            }
            else
            {
                Tokens = Array.Empty<Token>();
            }

            CommandLineText = commandLineText;
            _unmatchedTokens = unmatchedTokens is null ? Array.Empty<Token>() : unmatchedTokens;
            _errors = errors is not null ? errors : Array.Empty<ParseError>();
        }

        internal static ParseResult Empty() => new RootCommand().Parse(Array.Empty<string>());

        /// <summary>
        /// A result indicating the command specified in the command line input.
        /// </summary>
        public CommandResult CommandResult { get; }

        /// <summary>
        /// The configuration used to produce the parse result.
        /// </summary>
        public CommandLineConfiguration Configuration { get; }

        /// <summary>
        /// Gets the root command result.
        /// </summary>
        public CommandResult RootCommandResult => _rootCommandResult;

        /// <summary>
        /// Gets the parse errors found while parsing command line input.
        /// </summary>
        public IReadOnlyList<ParseError> Errors => _errors;

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
        public IReadOnlyList<string> UnmatchedTokens
            => _unmatchedTokens.Count == 0 ? Array.Empty<string>() : _unmatchedTokens.Select(t => t.Value).ToArray();

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
                Argument<T> argument => GetValue(argument),
                Option<T> option => GetValue(option),
                _ => throw new ArgumentOutOfRangeException()
            };

        /// <summary>
        /// Gets the parsed or default value for the specified argument.
        /// </summary>
        /// <param name="argument">The argument for which to get a value.</param>
        /// <returns>The parsed value or a configured default.</returns>
        public T? GetValue<T>(Argument<T> argument)
            => RootCommandResult.GetValue(argument);

        /// <summary>
        /// Gets the parsed or default value for the specified option.
        /// </summary>
        /// <param name="option">The option for which to get a value.</param>
        /// <returns>The parsed value or a configured default.</returns>
        public T? GetValue<T>(Option<T> option)
            => RootCommandResult.GetValue(option);

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
        /// Gets the result, if any, for the specified directive.
        /// </summary>
        /// <param name="directive">The directive for which to find a result.</param>
        /// <returns>A result for the specified directive, or <see langword="null"/> if it was not provided.</returns>
        public DirectiveResult? FindResultFor(Directive directive) => _rootCommandResult.FindResultFor(directive);

        /// <summary>
        /// Gets the result, if any, for the specified symbol.
        /// </summary>
        /// <param name="symbol">The symbol for which to find a result.</param>
        /// <returns>A result for the specified symbol, or <see langword="null"/> if it was not provided and no default was configured.</returns>
        public SymbolResult? FindResultFor(Symbol symbol)
            => _rootCommandResult.SymbolResultTree.TryGetValue(symbol, out SymbolResult? result) ? result : null;

        /// <summary>
        /// Gets completions based on a given parse result.
        /// </summary>
        /// <param name="position">The position at which completions are requested.</param>
        /// <returns>A set of completions for completion.</returns>
        public IEnumerable<CompletionItem> GetCompletions(
            int? position = null)
        {
            SymbolResult currentSymbolResult = SymbolToComplete(position);

            Symbol currentSymbol = currentSymbolResult switch
            {
                ArgumentResult argumentResult => argumentResult.Argument,
                OptionResult optionResult => optionResult.Option,
                DirectiveResult directiveResult => directiveResult.Directive,
                _ => ((CommandResult)currentSymbolResult).Command
            };

            var context = GetCompletionContext();

            if (position is not null &&
                context is TextCompletionContext tcc)
            {
                context = tcc.AtCursorPosition(position.Value);
            }

            var completions =
                currentSymbol is not null
                    ? currentSymbol.GetCompletions(context)
                    : Array.Empty<CompletionItem>();

            string[] optionsWithArgumentLimitReached = currentSymbolResult is CommandResult commandResult
                ? OptionsWithArgumentLimitReached(commandResult)
                : Array.Empty<string>();

            completions =
                completions.Where(item => optionsWithArgumentLimitReached.All(s => s != item.Label));

            return completions;

            static string[] OptionsWithArgumentLimitReached(CommandResult commandResult) =>
                commandResult
                    .Children
                    .OfType<OptionResult>()
                    .Where(c => c.IsArgumentLimitReached)
                    .Select(o => o.Option)
                    .SelectMany(c => c.Aliases)
                    .ToArray();
        }

        /// <summary>
        /// Invokes the appropriate command handler for a parsed command line input.
        /// </summary>
        /// <param name="console">A console to which output can be written. By default, <see cref="System.Console"/> is used.</param>
        /// <param name="cancellationToken">A token that can be used to cancel an invocation.</param>
        /// <returns>A task whose result can be used as a process exit code.</returns>
        public Task<int> InvokeAsync(IConsole? console = null, CancellationToken cancellationToken = default)
            => InvocationPipeline.InvokeAsync(this, console, cancellationToken);

        /// <summary>
        /// Invokes the appropriate command handler for a parsed command line input.
        /// </summary>
        /// <param name="console">A console to which output can be written. By default, <see cref="System.Console"/> is used.</param>
        /// <returns>A value that can be used as a process exit code.</returns>
        public int Invoke(IConsole? console = null) => InvocationPipeline.Invoke(this, console);

        internal ICommandHandler? Handler
        {
            get
            {
                _handler ??= CommandResult.Command.Handler;

                if (_useChainedHandler && _handler is not ChainedCommandHandler)
                {
                    // we can't create it when creating a ParseResult, because some Hosting extensions
                    // set Command.Handler via middleware, which is executed after parsing
                    _handler = new ChainedCommandHandler(_rootCommandResult.SymbolResultTree, _handler);
                }

                return _handler;
            }
        }
        private SymbolResult SymbolToComplete(int? position = null)
        {
            var commandResult = CommandResult;

            var allSymbolResultsForCompletion = AllSymbolResultsForCompletion();

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