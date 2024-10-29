// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Generic;
using System.CommandLine.Parsing;

namespace System.CommandLine
{
    /// <summary>
    /// Describes the results of parsing a command line input based on a specific parser configuration.
    /// </summary>
    public sealed class ParseResult
    {
        private readonly IReadOnlyDictionary<CliSymbol, CliValueResult> valueResultDictionary = new Dictionary<CliSymbol, CliValueResult>();
        private SymbolLookupByName? symbolLookupByName = null;

        // TODO: Remove usage and remove
        private readonly CliCommandResultInternal _rootCommandResult;
        // TODO: unmatched tokens, invocation, completion
        /*
        private readonly IReadOnlyList<CliToken> _unmatchedTokens;
        private CompletionContext? _completionContext;
        private readonly CliAction? _action;
        private readonly List<CliAction>? _preActions;
        */

        internal ParseResult(
            CliConfiguration configuration,
            // TODO: Remove RootCommandResult - it is the root of the CommandValueResult ancestors (fix that)
            CliCommandResultInternal rootCommandResult,
            // TODO: Replace with CommandValueResult and remove CommandResult
            CliCommandResultInternal commandResultInternal,
            IReadOnlyDictionary<CliSymbol, CliValueResult> valueResultDictionary,
            /*
            List<CliToken> tokens,
            */
            // TODO: unmatched tokens
            // List<CliToken>? unmatchedTokens,
            List<CliDiagnostic>? errors,
            // TODO: commandLineText should be string array
            string? commandLineText = null //,
            // TODO: invocation
            /*
            CliAction? action = null,
            List<CliAction>? preActions = null)
            */
            )
        {
            Configuration = configuration;
            _rootCommandResult = rootCommandResult;
            // TODO: Why do we need this?
            CommandResultInternal = commandResultInternal;
            CommandResult = commandResultInternal.CommandResult;
            this.valueResultDictionary = valueResultDictionary;
            // TODO: invocation
            /*
            _action = action;
            _preActions = preActions;
            */
            /*
            // skip the root command when populating Tokens property
            /*
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
                Tokens = Array.Empty<CliToken>();
            }
            */

            CommandLineText = commandLineText;

            // TODO: unmatched tokens
            // _unmatchedTokens = unmatchedTokens is null ? Array.Empty<CliToken>() : unmatchedTokens;

            Errors = errors is not null ? errors : Array.Empty<CliDiagnostic>();
        }

        public CliSymbol? GetSymbolByName(string name, bool valuesOnly = false)
        {
            symbolLookupByName ??= new SymbolLookupByName(this);
            return symbolLookupByName.TryGetSymbol(name, out var symbol, valuesOnly: valuesOnly)
                        ? symbol
                        : throw new ArgumentException($"No symbol result found with name \"{name}\".", nameof(name));
        }

        // TODO: check that constructing empty ParseResult directly is correct
        /*
        internal static ParseResult Empty() => new CliRootCommand().Parse(Array.Empty<string>());
        */

        /// <summary>
        /// A result indicating the command specified in the command line input.
        /// </summary>
        // TODO: Update SymbolLookupByName to use CommandValueResult, then remove
        internal CliCommandResultInternal CommandResultInternal { get; }

        public CliCommandResult CommandResult { get; }

        /// <summary>
        /// The configuration used to produce the parse result.
        /// </summary>
        public CliConfiguration Configuration { get; }

        /// <summary>
        /// Gets the root command result.
        /// </summary>
        // TODO: Update usage and then remove
        internal CliCommandResultInternal RootCommandResult => _rootCommandResult;

        /// <summary>
        /// Gets the parse errors found while parsing command line input.
        /// </summary>
        public IReadOnlyList<CliDiagnostic> Errors { get; }

        /*
        // TODO: don't expose tokens
        // TODO: This appears to be set, but only read during testing. Consider removing.
        /// <summary>
        /// Gets the tokens identified while parsing command line input.
        /// </summary>
        internal IReadOnlyList<CliToken> Tokens { get; }
        */

        // TODO: This appears to be set, but never used. Consider removing.
        /// <summary>
        /// Holds the value of a complete command line input prior to splitting and tokenization, when provided.
        /// </summary>
        /// <remarks>This will not be set when the parser is called from <c>Program.Main</c>. It is primarily used when calculating suggestions via the <c>dotnet-suggest</c> tool.</remarks>
        internal string? CommandLineText { get; }

        // TODO: CommandLineText, completion
        /*
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
                    ? new CompletionContext(this)
                    : new TextCompletionContext(this, CommandLineText);
        */
        /// <summary>
        /// Gets the parsed or default value for the specified argument.
        /// </summary>
        /// <param name="argument">The argument for which to get a value.</param>
        /// <returns>The parsed value or a configured default.</returns>
        public T? GetValue<T>(CliArgument<T> argument)
            => GetValueInternal<T>(argument);

        /// <summary>
        /// Gets the parsed or default value for the specified option.
        /// </summary>
        /// <param name="option">The option for which to get a value.</param>
        /// <returns>The parsed value or a configured default.</returns>
        public T? GetValue<T>(CliOption<T> option)
            => GetValueInternal<T>(option);

        private T? GetValueInternal<T>(CliValueSymbol valueSymbol)
            => valueResultDictionary.TryGetValue(valueSymbol, out var result)
                ? (T?)result.Value
                : default;

        /// <summary>
        /// Gets the parsed or default value for the specified symbol name, in the context of parsed command (not entire symbol tree).
        /// </summary>
        /// <param name="name">The name of the Symbol for which to get a value.</param>
        /// <returns>The parsed value or a configured default.</returns>
        /// <exception cref="InvalidOperationException">Thrown when parsing resulted in parse error(s).</exception>
        /// <exception cref="ArgumentException">Thrown when there was no symbol defined for given name for the parsed command.</exception>
        /// <exception cref="InvalidCastException">Thrown when parsed result can not be cast to <typeparamref name="T"/>.</exception>
        public T? GetValue<T>(string name)
        {
            var symbol = GetSymbolByName(name, valuesOnly: true);
            return symbol switch
            {
                CliArgument<T> argument => GetValue(argument),
                CliOption<T> option => GetValue(option),
                _ => throw new InvalidOperationException("Unexpected symbol type")
            };
        }

        // TODO: diagramming
        /*
        /// <inheritdoc />
        public override string ToString() => ParseDiagramAction.Diagram(this).ToString();
        */

        /// <summary>
        /// Gets the <see cref="CliValueResult"/> if any, for the specified option or argument
        /// </summary>
        /// <param name="valueSymbol">The option or argument for which to find a result.</param>
        /// <returns>A result for the specified option, or <see langword="null"/> if it was not entered by the user.</returns>
        public CliValueResult? GetValueResult(CliValueSymbol valueSymbol)
            => GetValueResultInternal(valueSymbol);

        private CliValueResult? GetValueResultInternal(CliValueSymbol valueSymbol)
            => valueResultDictionary.TryGetValue(valueSymbol, out var result)
                ? result
                : null;

        // TODO: Update tests and remove all use of things deriving from SymbolResult from this class
        /// <summary>
        /// Gets the result, if any, for the specified argument.
        /// </summary>
        /// <param name="argument">The argument for which to find a result.</param>
        /// <returns>A result for the specified argument, or <see langword="null"/> if it was not provided and no default was configured.</returns>
        internal CliArgumentResultInternal? GetResult(CliArgument argument) =>
            _rootCommandResult.GetResult(argument);

        /* Not used
        /// <summary>
        /// Gets the result, if any, for the specified command.
        /// </summary>
        /// <param name="command">The command for which to find a result.</param>
        /// <returns>A result for the specified command, or <see langword="null"/> if it was not provided.</returns>
        internal CliCommandResultInternal? GetResult(CliCommand command) =>
            _rootCommandResult.GetResult(command);
        */

        /// <summary>
        /// Gets the result, if any, for the specified option.
        /// </summary>
        /// <param name="option">The option for which to find a result.</param>
        /// <returns>A result for the specified option, or <see langword="null"/> if it was not provided and no default was configured.</returns>
        internal CliOptionResultInternal? GetResult(CliOption option) =>
            _rootCommandResult.GetResult(option);

        // TODO: Directives
        /*
        /// <summary>
        /// Gets the result, if any, for the specified directive.
        /// </summary>
        /// <param name="directive">The directive for which to find a result.</param>
        /// <returns>A result for the specified directive, or <see langword="null"/> if it was not provided.</returns>
        public DirectiveResult? GetResult(CliDirective directive) => _rootCommandResult.GetResult(directive);
        */
        /* Replaced with GetValueResult 
        /// <summary>
        /// Gets the result, if any, for the specified symbol.
        /// </summary>
        /// <param name="symbol">The symbol for which to find a result.</param>
        /// <returns>A result for the specified symbol, or <see langword="null"/> if it was not provided and no default was configured.</returns>
        public CliSymbolResultInternal? GetResult(CliSymbol symbol)
            => _rootCommandResult.SymbolResultTree.TryGetValue(symbol, out CliSymbolResultInternal? result) ? result : null;
        */
        // TODO: completion, invocation
        /*
        /// <summary>
        /// Gets completions based on a given parse result.
        /// </summary>
        /// <param name="position">The position at which completions are requested.</param>
        /// <returns>A set of completions for completion.</returns>
        public IEnumerable<CompletionItem> GetCompletions(
            int? position = null)
        {
            CliSymbolResultInternal currentSymbolResult = SymbolToComplete(position);

            CliSymbol currentSymbol = currentSymbolResult switch
            {
                ArgumentResult argumentResult => argumentResult.Argument,
                OptionResult optionResult => optionResult.Option,
                DirectiveResult directiveResult => directiveResult.Directive,
                _ => ((CliCommandResultInternal)currentSymbolResult).Command
            };

            var context = GetCompletionContext();

            if (position is not null &&
                context is TextCompletionContext tcc)
            {
                context = tcc.AtCursorPosition(position.Value);
            }

            var completions = currentSymbol.GetCompletions(context);

            string[] optionsWithArgumentLimitReached = currentSymbolResult is CliCommandResultInternal commandResult
                                                            ? OptionsWithArgumentLimitReached(commandResult)
                                                            : Array.Empty<string>();

            completions =
                completions.Where(item => optionsWithArgumentLimitReached.All(s => s != item.Label));

            return completions;

            static string[] OptionsWithArgumentLimitReached(CliCommandResultInternal commandResult) =>
                commandResult
                    .Children
                    .OfType<OptionResult>()
                    .Where(c => c.IsArgumentLimitReached)
                    .Select(o => o.Option)
                    .SelectMany(c => new[] { c.Name }.Concat(c.Aliases))
                    .ToArray();
        }

        /// <summary>
        /// Invokes the appropriate command handler for a parsed command line input.
        /// </summary>
        /// <param name="cancellationToken">A token that can be used to cancel an invocation.</param>
        /// <returns>A task whose result can be used as a process exit code.</returns>
        public Task<int> InvokeAsync(CancellationToken cancellationToken = default)
            => InvocationPipeline.InvokeAsync(this, cancellationToken);

        /// <summary>
        /// Invokes the appropriate command handler for a parsed command line input.
        /// </summary>
        /// <returns>A value that can be used as a process exit code.</returns>
        public int Invoke()
        {
            var useAsync = false;

            if (Action is AsynchronousCliAction)
            {
                useAsync = true;
            }
            else if (PreActions is not null)
            {
                for (var i = 0; i < PreActions.Count; i++)
                {
                    var action = PreActions[i];
                    if (action is AsynchronousCliAction)
                    {
                        useAsync = true;
                        break;
                    }
                }
            }

            if (useAsync)
            {
                return InvocationPipeline.InvokeAsync(this, CancellationToken.None).ConfigureAwait(false).GetAwaiter().GetResult();
            }
            else
            {
                return InvocationPipeline.Invoke(this);
            }
        }

        /// <summary>
        /// Gets the <see cref="CliAction"/> for parsed result. The handler represents the action
        /// that will be performed when the parse result is invoked.
        /// </summary>
        public CliAction? Action => _action ?? CliCommandResultInternal.Command.Action;

        internal IReadOnlyList<CliAction>? PreActions => _preActions;

        private CliSymbolResultInternal SymbolToComplete(int? position = null)
        {
            var commandResult = CliCommandResultInternal;

            var allSymbolResultsForCompletion = AllSymbolResultsForCompletion();

            var currentSymbol = allSymbolResultsForCompletion.Last();

            return currentSymbol;

            IEnumerable<CliSymbolResultInternal> AllSymbolResultsForCompletion()
            {
                foreach (var item in commandResult.AllSymbolResults())
                {
                    if (item is CliCommandResultInternal command)
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
                if (optionResult.Implicit)
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
        */
    }
}