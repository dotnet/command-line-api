// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace System.CommandLine
{
    public static class ParseResultExtensions
    {
        public static object GetDefaultValue(this Type type)
        {
            return type.IsValueType ? Activator.CreateInstance(type) : null;
        }

        public static object GetValue(this ParseResult parseResult, Option option)
        {
            var result = parseResult.CommandResult.Children
                                   .Where(c => c.Symbol == option)
                                   .FirstOrDefault();
            switch (result)
            {
                case OptionResult optionResult:
                    return optionResult.GetValueOrDefault();
                case null:
                    var optionType = option.Argument.ArgumentType == null
                                    ? typeof(bool)
                                    : option.Argument.ArgumentType;
                    return optionType.GetDefaultValue();
                default:
                    throw new InvalidOperationException("Internal: Unknown result type");
            }
        }

        public static object GetValue(this ParseResult parseResult, Argument argument)
        {
            // TODO: Change when we support multiple arguments
            return parseResult.CommandResult.GetValueOrDefault();
        }

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
                    return lastToken;
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
                var result = symbolResult.Result;
                if (result is SuccessfulArgumentParseResult _)
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
            var currentSymbolResult = parseResult.CurrentSymbol();
            var currentSymbol = currentSymbolResult.Symbol;
            var includeParentSuggestions = true;

            if (currentSymbolResult is OptionResult optionResult)
            {
                var maxNumberOfArguments =
                    optionResult.Option
                                .Argument
                                .Arity
                                .MaximumNumberOfArguments;

                if (currentSymbolResult.Arguments.Count < maxNumberOfArguments)
                {
                    includeParentSuggestions = false;
                }
            }

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

            var parentSymbolSuggestions =
                includeParentSuggestions &&
                currentSymbol?.Parent is ISuggestionSource parentSuggestionSource
                    ? parentSuggestionSource.Suggest(parseResult, position).Except(currentSymbol.RawAliases)
                    : Array.Empty<string>();

            return parentSymbolSuggestions
                   .Concat(currentSymbolSuggestions)
                   .ToArray();
        }
    }
}
