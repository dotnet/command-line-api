// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections;
using System.CommandLine.Binding;
using System.CommandLine.Invocation;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace System.CommandLine.Parsing
{
    /// <summary>
    /// Provides extension methods for parse results.
    /// </summary>
    public static class ParseResultExtensions
    {
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

                var unmatchedTokens = parseResult.UnmatchedTokens;
                if (unmatchedTokens.Count > 0)
                {
                    builder.Append("   ???-->");

                    for (var i = 0; i < unmatchedTokens.Count; i++)
                    {
                        var error = unmatchedTokens[i];
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

            switch (symbolResult)
            {
                case ArgumentResult argumentResult:
                {
                    var includeArgumentName =
                        argumentResult.Argument.FirstParent!.Symbol is Command command &&
                        command.HasArguments && command.Arguments.Count > 1;

                    if (includeArgumentName)
                    {
                        builder.Append("[ ");
                        builder.Append(argumentResult.Argument.Name);
                        builder.Append(" ");
                    }

                    if (argumentResult.Argument.Arity.MaximumNumberOfValues > 0)
                    {
                        ArgumentConversionResult conversionResult = argumentResult.GetArgumentConversionResult();
                        switch (conversionResult.Result)
                        {
                            case ArgumentConversionResultType.NoArgument:
                                break;
                            case ArgumentConversionResultType.Successful:
                                switch (conversionResult.Value)
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
                                        builder.Append(conversionResult.Value);
                                        builder.Append(">");
                                        break;
                                }

                                break;

                            default: // failures
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

                    break;
                }

                default:
                {
                    string tokenValue = symbolResult switch
                    {
                        CommandResult commandResult => commandResult.Token.Value,
                        OptionResult optionResult => optionResult.Token?.Value ?? GetLongestAlias(optionResult.Option),
                        _ => throw new ArgumentOutOfRangeException(nameof(symbolResult))
                    };

                    if (symbolResult is OptionResult { IsImplicit: true })
                    {
                        builder.Append("*");
                    }

                    builder.Append("[ ");
                    builder.Append(tokenValue);

                    foreach (SymbolResult child in symbolResult.SymbolResultTree.GetChildren(symbolResult))
                    {
                        if (child is ArgumentResult arg &&
                            (arg.Argument.ValueType == typeof(bool) ||
                             arg.Argument.Arity.MaximumNumberOfValues == 0))
                        {
                            continue;
                        }

                        builder.Append(" ");

                        builder.Diagram(child, parseResult);
                    }

                    builder.Append(" ]");
                    break;
                }
            }
        }

        private static string GetLongestAlias(Option option)
        {
            string longestAlias = "";

            foreach (string alias in option.Aliases)
            {
                if (alias.Length > longestAlias.Length)
                {
                    longestAlias = alias;
                }
            }

            return longestAlias;
        }
    }
}
