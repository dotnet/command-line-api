// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections;
using System.Collections.Generic;
using System.CommandLine.Binding;
using System.CommandLine.Invocation;
using System.CommandLine.Completions;
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
        /// <summary>
        /// Invokes the appropriate command handler for a parsed command line input.
        /// </summary>
        /// <param name="parseResult">A parse result on which the invocation is based.</param>
        /// <param name="console">A console to which output can be written. By default, <see cref="System.Console"/> is used.</param>
        /// <returns>A task whose result can be used as a process exit code.</returns>
        public static async Task<int> InvokeAsync(
            this ParseResult parseResult,
            IConsole? console = null) =>
            await new InvocationPipeline(parseResult).InvokeAsync(console);

        /// <summary>
        /// Invokes the appropriate command handler for a parsed command line input.
        /// </summary>
        /// <param name="parseResult">A parse result on which the invocation is based.</param>
        /// <param name="console">A console to which output can be written. By default, <see cref="System.Console"/> is used.</param>
        /// <returns>A value that can be used as a process exit code.</returns>
        public static int Invoke(
            this ParseResult parseResult,
            IConsole? console = null) =>
            new InvocationPipeline(parseResult).Invoke(console);

        /// <summary>
        /// Formats a string explaining a parse result.
        /// </summary>
        /// <param name="parseResult">The parse result to be diagrammed.</param>
        /// <returns>A string containing a diagram of the parse result.</returns>
        public static string Diagram(this ParseResult parseResult)
        {
            var builder = StringBuilderPool.Default.Rent();

            try
            {
                builder.Diagram(parseResult.RootCommandResult, parseResult);

                if (parseResult.UnmatchedTokens.Count > 0)
                {
                    builder.Append("   ???-->");

                    for (var i = 0; i < parseResult.UnmatchedTokens.Count; i++)
                    {
                        var error = parseResult.UnmatchedTokens[i];
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

        /// <summary>
        /// Indicates whether a given option is present in the parse result.
        /// </summary>
        /// <remarks>If the option has a default value defined, then <see langword="true"/> will be returned.</remarks>
        /// <param name="parseResult">The parse result to check for the presence of the option.</param>
        /// <param name="option">The option to check for the presence of.</param>
        /// <returns><see langword="true"/> if the option is present; otherwise,  <see langword="false"/>.</returns>
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

        /// <inheritdoc cref="HasOption(System.CommandLine.Parsing.ParseResult,System.CommandLine.IOption)"/>
        [Obsolete("This method is obsolete and will be removed in a future version. Please use ParseResultExtensions.HasOption(ParseResult, IOption) instead. For details see https://github.com/dotnet/command-line-api/issues/1127")]
        public static bool HasOption(
            this ParseResult parseResult,
            string alias)
        {
            if (parseResult is null)
            {
                throw new ArgumentNullException(nameof(parseResult));
            }

            return parseResult.CommandResult.Children.ContainsAlias(alias);
        }

        /// <summary>
        /// Gets completions based on a given parse result.
        /// </summary>
        /// <param name="parseResult">The parse result that provides context for the completions.</param>
        /// <param name="position">The position at which completions are requested.</param>
        /// <returns>A set of completions for completion.</returns>
        public static IEnumerable<CompletionItem> GetCompletions(
            this ParseResult parseResult,
            int? position = null)
        {
            var currentSymbolResult = parseResult.SymbolToComplete(position);

            var currentSymbol = currentSymbolResult.Symbol;

            var context = parseResult.GetCompletionContext();

            if (position is not null && 
                context is TextCompletionContext tcc)
            {
                context = tcc.AtPosition(position.Value);
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
                    .OfType<IIdentifierSymbol>()
                    .SelectMany(c => c.Aliases);
        }

        internal static SymbolResult SymbolToComplete(
            this ParseResult parseResult,
            int? position = null)
        {
            var commandResult = parseResult.CommandResult;

            var currentSymbol = AllSymbolResultsForCompletion().Last();

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
                            WillAcceptAnArgument(parseResult, position, option);

                        if (willAcceptAnArgument)
                        {
                            yield return option;
                        }
                    }
                }
            }

            static bool WillAcceptAnArgument(
                ParseResult parseResult,
                int? position,
                OptionResult option)
            {
                if (option.IsImplicit)
                {
                    return false;
                }

                if (!option.IsArgumentLimitReached)
                {
                    return true;
                }

                return parseResult.GetCompletionContext() switch
                {
                    TextCompletionContext c when position.HasValue =>
                        c.AtPosition(position.Value).TextToMatch.Length > 0,
                    { } c =>
                        c.TextToMatch.Length > 0,
                    _ =>
                        throw new ArgumentOutOfRangeException()
                };
            }
        }
    }
}
