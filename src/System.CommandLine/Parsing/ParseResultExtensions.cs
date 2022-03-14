// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections;
using System.CommandLine.Binding;
using System.CommandLine.Invocation;
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


            switch (symbolResult)
            {
                case ArgumentResult argumentResult:
                {
                    var includeArgumentName =
                        argumentResult.Argument.FirstParent!.Symbol is Command command &&
                        command.Arguments.Count > 1;

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
                    if (symbolResult is OptionResult { IsImplicit: true })
                    {
                        builder.Append("*");
                    }

                    builder.Append("[ ");
                    builder.Append(symbolResult.Token().Value);

                    for (var i = 0; i < symbolResult.Children.Count; i++)
                    {
                        var child = symbolResult.Children[i];

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

        /// <summary>
        /// Indicates whether a given option is present in the parse result.
        /// </summary>
        /// <remarks>If the option has a default value defined, then <see langword="true"/> will be returned.</remarks>
        /// <param name="parseResult">The parse result to check for the presence of the option.</param>
        /// <param name="option">The option to check for the presence of.</param>
        /// <returns><see langword="true"/> if the option is present; otherwise,  <see langword="false"/>.</returns>
        public static bool HasOption(
            this ParseResult parseResult,
            Option option)
        {
            if (parseResult is null)
            {
                throw new ArgumentNullException(nameof(parseResult));
            }

            return parseResult.FindResultFor(option) is { };
        }
    }
}
