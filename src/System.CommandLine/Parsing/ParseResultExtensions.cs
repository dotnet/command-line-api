// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections;
using System.Collections.Generic;
using System.CommandLine.Binding;
using System.CommandLine.Invocation;
using System.CommandLine.Suggestions;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace System.CommandLine.Parsing
{
    /// <summary>
    /// Provides extension methods for parse results.
    /// </summary>
    public static class ParseResultExtensions
    {
        public static async Task<int> InvokeAsync(
            this ParseResult parseResult,
            IConsole? console = null) =>
            await new InvocationPipeline(parseResult).InvokeAsync(console);

        public static int Invoke(
            this ParseResult parseResult,
            IConsole? console = null) =>
            new InvocationPipeline(parseResult).Invoke(console);

        public static string TextToMatch(
            this ParseResult source,
            int? position = null)
        {
            Token? lastToken = source.Tokens.LastOrDefault(t => t.Type != TokenType.Directive);

            string? textToMatch = null;
            string? rawInput = source.RawInput;

            if (rawInput != null)
            {
                if (position != null)
                {
                    if (position > rawInput.Length)
                    {
                        rawInput += ' ';
                        position = Math.Min(rawInput.Length, position.Value);
                    }
                }
                else
                {
                    position = rawInput.Length;
                }
            }
            else if (lastToken?.Value != null)
            {
                position = null;
                textToMatch = lastToken.Value;
            }

            if (string.IsNullOrWhiteSpace(rawInput))
            {
                if (source.UnmatchedTokens.Count > 0 ||
                    lastToken?.Type == TokenType.Argument)
                {
                    return textToMatch ?? "";
                }
            }
            else
            {
                var textBeforeCursor = rawInput!.Substring(0, position!.Value);

                var textAfterCursor = rawInput.Substring(position.Value);

                return textBeforeCursor.Split(' ').LastOrDefault() +
                       textAfterCursor.Split(' ').FirstOrDefault();
            }

            return "";
        }

        public static string Diagram(this ParseResult result)
        {
            var builder = StringBuilderPool.Default.Rent();

            try
            {
                builder.Diagram(result.RootCommandResult, result);

                if (result.UnmatchedTokens.Count > 0)
                {
                    builder.Append("   ???-->");

                    for (var i = 0; i < result.UnmatchedTokens.Count; i++)
                    {
                        var error = result.UnmatchedTokens[i];
                        builder.Append(" ");
                        builder.Append(error);
                    }
                }

                return builder.ToString();
            }
            finally
            {
                StringBuilderPool.Default.ReturnToPool(builder);
            }
        }

        private static void Diagram(
            this StringBuilder builder,
            SymbolResult symbolResult,
            ParseResult parseResult)
        {
            if (parseResult.Errors.Any(e => e.SymbolResult == symbolResult))
            {
                builder.Append("!");
            }

            if (symbolResult is OptionResult optionResult &&
                optionResult.IsImplicit)
            {
                builder.Append("*");
            }

            if (symbolResult is ArgumentResult argumentResult)
            {

                var includeArgumentName =
                    argumentResult.Argument is Argument argument &&
                    argument.Parents[0] is ICommand command &&
                    command.Arguments.Count > 1;

                if (includeArgumentName)
                {
                    builder.Append("[ ");
                    builder.Append(argumentResult.Argument.Name);
                    builder.Append(" ");
                }

                if (argumentResult.Argument.Arity.MaximumNumberOfValues > 0)
                {
                    switch (argumentResult.GetArgumentConversionResult())
                    {
                        case SuccessfulArgumentConversionResult successful:

                            switch (successful.Value)
                            {
                                case string s:
                                    builder.Append($"<{s}>");
                                    break;

                                case IEnumerable items:
                                    builder.Append("<");
                                    builder.Append(
                                        string.Join("> <",
                                                    items.Cast<object>().ToArray()));
                                    builder.Append(">");
                                    break;

                                default:
                                    builder.Append("<");
                                    builder.Append(successful.Value);
                                    builder.Append(">");
                                    break;
                            }

                            break;

                        case FailedArgumentConversionResult _:

                            builder.Append("<");
                            builder.Append(string.Join("> <", symbolResult.Tokens.Select(t => t.Value)));
                            builder.Append(">");

                            break;
                    }
                }

                if (includeArgumentName)
                {
                    builder.Append(" ]");
                }
            }
            else
            {
                builder.Append("[ ");
                builder.Append(symbolResult.Token().Value);

                for (var i = 0; i < symbolResult.Children.Count; i++)
                {
                    var child = symbolResult.Children[i];

                    if (child is ArgumentResult arg && 
                        arg.Argument.Arity.MaximumNumberOfValues == 0)
                    {
                        continue;
                    }

                    builder.Append(" ");
                    builder.Diagram(child, parseResult);
                }

                builder.Append(" ]");
            }
        }

        public static bool HasOption(
            this ParseResult parseResult,
            IOption option)
        {
            if (parseResult is null)
            {
                throw new ArgumentNullException(nameof(parseResult));
            }

            return parseResult.FindResultFor(option) is { };
        }

        [Obsolete("This method is obsolete and will be removed in a future version. Please use ParseResultExtensions.HasOption(ParseResult, IOption) instead. For details see https://github.com/dotnet/command-line-api/issues/1127")]
        public static bool HasOption(
            this ParseResult parseResult,
            string alias)
        {
            if (parseResult is null)
            {
                throw new ArgumentNullException(nameof(parseResult));
            }

            return parseResult.CommandResult.Children.Contains(alias);
        }

        public static IEnumerable<string?> GetSuggestions(
            this ParseResult parseResult,
            int? position = null)
        {
            var textToMatch = parseResult.TextToMatch(position);
            var currentSymbolResult = parseResult.SymbolToComplete(position);
            var currentSymbol = currentSymbolResult.Symbol;

            var currentSymbolSuggestions =
                currentSymbol is ISuggestionSource currentSuggestionSource
                    ? currentSuggestionSource.GetSuggestions(parseResult, textToMatch)
                    : Array.Empty<string>();

            IEnumerable<string?> siblingSuggestions;
            var parentSymbol = currentSymbolResult.Parent?.Symbol;

            if (parentSymbol is null ||
                !currentSymbolResult.IsArgumentLimitReached)
            {
                siblingSuggestions = Array.Empty<string?>();
            }
            else
            {
                siblingSuggestions = parentSymbol
                                     .GetSuggestions(parseResult, textToMatch)
                                     .Except(parentSymbol
                                             .Children
                                             .OfType<ICommand>()
                                             .SelectMany(c => c.Aliases));
            }

            if (currentSymbolResult is CommandResult commandResult)
            {
                currentSymbolSuggestions = currentSymbolSuggestions
                    .Except(OptionsWithArgumentLimitReached(currentSymbolResult));

                if (currentSymbolResult.Parent is CommandResult parent)
                {
                    siblingSuggestions = siblingSuggestions.Except(OptionsWithArgumentLimitReached(parent));
                }
            }

            return currentSymbolSuggestions.Concat(siblingSuggestions);

            string[] OptionsWithArgumentLimitReached(SymbolResult symbolResult)
            {
                var optionsWithArgLimitReached =
                    symbolResult
                        .Children
                        .Where(c => c.IsArgumentLimitReached);

                var exclude = optionsWithArgLimitReached
                              .OfType<OptionResult>()
                              .Select(o => o.Symbol)
                              .OfType<IIdentifierSymbol>()
                              .SelectMany(c => c.Aliases)
                              .Concat(commandResult.Command.Aliases)
                              .ToArray();

                return exclude;
            }
        }

        internal static SymbolResult SymbolToComplete(
            this ParseResult parseResult,
            int? position = null)
        {
            var commandResult = parseResult.CommandResult;

            var currentSymbol = AllSymbolResultsForCompletion()
                .LastOrDefault();

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
                        var willAcceptAnArgument =
                            !option.IsImplicit &&
                            (!option.IsArgumentLimitReached ||
                             parseResult.TextToMatch(position).Length > 0);

                        if (willAcceptAnArgument)
                        {
                            yield return option;
                        }
                    }
                }
            }
        }
    }
}
