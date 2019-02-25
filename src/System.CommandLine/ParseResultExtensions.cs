// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Generic;
using System.CommandLine.Binding;
using System.Linq;
using System.Text;

namespace System.CommandLine
{
    public static class ParseResultExtensions
    {
        public static string TextToMatch(
            this ParseResult source,
            int? position = null)
        {
            var lastToken = source.Tokens.LastOrDefault();

            if (string.IsNullOrWhiteSpace(source.RawInput))
            {
                return source.UnmatchedTokens.LastOrDefault() ?? "";
            }

            if (position == null)
            {
                // assume the cursor is at the end of the input
                if (!source.RawInput.EndsWith(" "))
                {
                    return lastToken.Value;
                }
                else
                {
                    return "";
                }
            }

            var textBeforeCursor = source.RawInput.Substring(0, position.Value);

            var textAfterCursor = source.RawInput.Substring(position.Value);

            return textBeforeCursor.Split(' ').LastOrDefault() +
                   textAfterCursor.Split(' ').FirstOrDefault();
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

            if (symbolResult is OptionResult option &&
                option.IsImplicit)
            {
                builder.Append("*");
            }

            builder.Append("[ ");

            builder.Append(symbolResult.Token);

            foreach (var child in symbolResult.Children)
            {
                builder.Append(" ");
                builder.Diagram(child, parseResult);
            }

            if (symbolResult.Arguments.Count > 0)
            {
                foreach (var arg in symbolResult.Arguments)
                {
                    builder.Append(" <");
                    builder.Append(arg);
                    builder.Append(">");
                }
            }
            else
            {
                var result = symbolResult.ArgumentResult;
                if (result is SuccessfulArgumentResult _)
                {
                    var value = symbolResult.GetValueOrDefault();

                    switch (value)
                    {
                        case null:
                        case IReadOnlyCollection<string> a when a.Count == 0:
                            break;
                        default:
                            builder.Append(" <");
                            builder.Append(value);
                            builder.Append(">");
                            break;
                    }
                }
            }

            builder.Append(" ]");
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
            var currentSymbolResult = parseResult.SymbolToComplete();
            var currentSymbol = currentSymbolResult.Symbol;

            var currentSymbolSuggestions =
                currentSymbol is ISuggestionSource currentSuggestionSource
                    ? currentSuggestionSource.Suggest(parseResult, position)
                    : Array.Empty<string>();

            if (currentSymbolResult is CommandResult commandResult)
            {
                var optionsWithArgLimitReached =
                    currentSymbolResult
                        .Children
                        .Where(c => c.IsArgumentLimitReached);

                var exclude =
                    optionsWithArgLimitReached
                        .SelectMany(c => c.Symbol.RawAliases)
                        .Concat(commandResult.Symbol.RawAliases)
                        .ToArray();

                currentSymbolSuggestions = currentSymbolSuggestions.Except(exclude);
            }

            return currentSymbolSuggestions;
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

        internal static IEnumerable<IValueDescriptor> ValueDescriptors(this ParseResult parseResult)
        {
            var command = parseResult.CommandResult.Command;

            while (command != null)
            {
                yield return command;

                foreach (var option in command.Children.OfType<Option>())
                {
                    yield return option;
                }

                command = command.Parent;
            }
        }
    }
}
