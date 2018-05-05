using System.Collections.Generic;

namespace Microsoft.DotNet.Cli.CommandLine
{
    internal class RawParseResult
    {
        public RawParseResult(
            IReadOnlyCollection<string> rawTokens,
            ParsedSymbolSet parsedSymbol,
            ParserConfiguration configuration,
            IReadOnlyCollection<string> unparsedTokens,
            IReadOnlyCollection<string> unmatchedTokens,
            IReadOnlyCollection<ParseError> errors,
            string rawInput = null)
        {
            Errors = errors;
            RawInput = rawInput;
            RawTokens = rawTokens;
            ParsedSymbol = parsedSymbol;
            Configuration = configuration;
            UnparsedTokens = unparsedTokens;
            UnmatchedTokens = unmatchedTokens;
        }

        public IReadOnlyCollection<ParseError> Errors { get; }
        internal string RawInput { get; }
        public IReadOnlyCollection<string> RawTokens { get; }
        public ParsedSymbolSet ParsedSymbol { get; }
        public ParserConfiguration Configuration { get; }
        public IReadOnlyCollection<string> UnparsedTokens { get; }
        public IReadOnlyCollection<string> UnmatchedTokens { get; }
    }
}
