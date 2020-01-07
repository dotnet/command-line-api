// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Generic;
using System.CommandLine.Binding;
using System.CommandLine.Suggestions;
using System.Linq;
using System.Text;

namespace System.CommandLine.Parsing
{
    public static class ParseResultExtensions
    {
        public static string TextToMatch(
            this ParseResult source,
            int? position = null)
        {
            var lastToken = source.Tokens.LastOrDefault(t => t.Type != TokenType.Directive);

            string textToMatch = null;
            var rawInput = source.RawInput;

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
                if (source.UnmatchedTokens.Any() ||
                    lastToken?.Type == TokenType.Argument)
                {
                    return textToMatch;
                }
            }
            else 
            {
                var textBeforeCursor = rawInput.Substring(0, position.Value);

                var textAfterCursor = rawInput.Substring(position.Value);

                return textBeforeCursor.Split(' ').LastOrDefault() +
                       textAfterCursor.Split(' ').FirstOrDefault();
            }

            return "";
        }

        public static string Diagram(this ParseResult result)
        {
            var builder = new StringBuilder();

            builder.Diagram(result.RootCommandResult, result);

            if (result.UnmatchedTokens.Any())
            {
                builder.Append("   ???-->");

                foreach (var error in result.UnmatchedTokens)
                {
                    builder.Append(" ");
                    builder.Append(error);
                }
            }

            return builder.ToString();
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
                    argument.Parents.First() is ICommand command &&
                    command.Name != argument.Name;

                if (includeArgumentName)
                {
                    builder.Append("[ ");
                    builder.Append(argumentResult.Argument.Name);
                    builder.Append(" ");
                }

                switch (symbolResult.ArgumentConversionResult)
                {
                    case SuccessfulArgumentConversionResult successful:

                        switch (successful.Value)
                        {
                            case null:
                            case IReadOnlyCollection<string> a when a.Count == 0:
                                break;
                            case IEnumerable<string> args:
                                builder.Append("<");
                                builder.Append(string.Join("> <", args));
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

                if (includeArgumentName)
                {
                    builder.Append(" ]");
                }
            }
            else
            {
                builder.Append("[ ");
                builder.Append(symbolResult.Token.Value);

                foreach (var child in symbolResult.Children)
                {
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
            if (parseResult == null)
            {
                throw new ArgumentNullException(nameof(parseResult));
            }

            return parseResult.CommandResult.Children.Any(s => s.Symbol == option);
        }

        public static bool HasOption(
            this ParseResult parseResult,
            string alias)
        {
            if (parseResult == null)
            {
                throw new ArgumentNullException(nameof(parseResult));
            }

            return parseResult.CommandResult.Children.Contains(alias);
        }

        public static IEnumerable<string> Suggestions(
            this ParseResult parseResult,
            int? position = null)
        {
            var textToMatch = parseResult.TextToMatch(position);
            var currentSymbolResult = parseResult.SymbolToComplete(position);
            var currentSymbol = currentSymbolResult.Symbol;

            var currentSymbolSuggestions =
                currentSymbol is ISuggestionSource currentSuggestionSource
                    ? currentSuggestionSource.GetSuggestions(textToMatch)
                    : Array.Empty<string>();

            IEnumerable<string> siblingSuggestions;
            var parentSymbol = currentSymbolResult.Parent?.Symbol;

            if (parentSymbol == null ||
                !currentSymbolResult.IsArgumentLimitReached)
            {
                siblingSuggestions = Array.Empty<string>();
            }
            else
            {
                siblingSuggestions = parentSymbol
                                     .GetSuggestions(textToMatch)
                                     .Except(parentSymbol
                                             .Children
                                             .OfType<ICommand>()
                                             .Select(c => c.Name));
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
                              .SelectMany(c => c.Symbol.RawAliases)
                              .Concat(commandResult.Symbol.RawAliases)
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
