using System.CommandLine.Invocation;
using System.Linq;
using System.CommandLine.IO;
using System.CommandLine.Parsing;

namespace System.CommandLine
{
    public sealed class SuggestDirective : Directive
    {
        public SuggestDirective() : base("suggest", syncHandler: SyncHandler)
        {
        }

        private static void SyncHandler(InvocationContext context)
        {
            SuggestDirective symbol = (SuggestDirective)context.ParseResult.Symbol;
            string? parsedValues = context.ParseResult.FindResultFor(symbol)!.Value;
            string? rawInput = context.ParseResult.CommandLineText;

            int position = parsedValues is not null ? int.Parse(parsedValues) : rawInput?.Length ?? 0;

            var commandLineToComplete = context.ParseResult.Tokens.LastOrDefault(t => t.Type != TokenType.Directive)?.Value ?? "";

            var completionParseResult = context.Parser.Parse(commandLineToComplete);

            var completions = completionParseResult.GetCompletions(position);

            context.Console.Out.WriteLine(
                string.Join(
                    Environment.NewLine,
                    completions));
        }
    }
}
