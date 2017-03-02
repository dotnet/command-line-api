using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CommandLine
{
    public static class ParseResultExtensions
    {
        public static string TextToMatch(this ParseResult source)
        {
            var lastToken = source.Tokens.LastOrDefault();

            if (string.IsNullOrWhiteSpace(lastToken))
            {
                return "";
            }

            if (source.IsProgressive)
            {
                return lastToken;
            }

            return source.UnmatchedTokens.LastOrDefault() ?? "";
        }

        public static Command Command(this ParseResult result) =>
            result.AppliedOptions
                  .FlattenBreadthFirst()
                  .Select(a => a.Option)
                  .OfType<Command>()
                  .LastOrDefault();

        internal static AppliedOption CurrentOption(this ParseResult result) =>
            result.AppliedOptions
                  .LastOrDefault()
                  .AllOptions()
                  .LastOrDefault();

        public static string Diagram(this ParseResult result)
        {
            var builder = new StringBuilder();

            foreach (var o in result.AppliedOptions)
            {
                builder.Diagram(o);
            }

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

        public static string Diagram(this AppliedOption option)
        {
            var stringbuilder = new StringBuilder();

            stringbuilder.Diagram(option);

            return stringbuilder.ToString();
        }

        private static void Diagram(
            this StringBuilder builder,
            AppliedOption option)
        {
            builder.Append("[ ");

            builder.Append(option.Option);

            foreach (var childOption in option.AppliedOptions)
            {
                builder.Append(" ");
                builder.Diagram(childOption);
            }

            if (option.Arguments.Any())
            {
                builder.Append(" <");
                builder.Append(string.Join(" ", option.Arguments));
                builder.Append(">");
            }

            builder.Append(" ]");
        }

        public static CommandExecutionResult Execute(this ParseResult parseResult)
        {
            if (parseResult == null)
            {
                throw new ArgumentNullException(nameof(parseResult));
            }

            var command = parseResult.Command();
            if (command != null)
            {
                var value = parseResult[command.Name].Value();
            }

            return new CommandExecutionResult(parseResult);
        }

        public static IEnumerable<string> Suggestions(this ParseResult parseResult) =>
            parseResult?.CurrentOption()
                       ?.Option
                       ?.Suggest(parseResult) ??
            Array.Empty<string>();
    }
}