using System.CommandLine.Invocation;
using System.Linq;
using System.CommandLine.IO;
using System.CommandLine.Parsing;

namespace System.CommandLine
{
    /// <summary>
    /// Enables the use of the <c>[suggest]</c> directive which when specified in command line input short circuits normal command handling and writes a newline-delimited list of suggestions suitable for use by most shells to provide command line completions.
    /// </summary>
    /// <remarks>The <c>dotnet-suggest</c> tool requires the suggest directive to be enabled for an application to provide completions.</remarks>
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
