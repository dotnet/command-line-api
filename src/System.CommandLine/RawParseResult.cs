using System.Collections.Generic;

namespace System.CommandLine
{
    internal class RawParseResult
    {
        public RawParseResult(
            IReadOnlyCollection<string> rawTokens,
            SymbolSet symbol,
            ParserConfiguration configuration,
            IReadOnlyCollection<string> unparsedTokens,
            IReadOnlyCollection<string> unmatchedTokens,
            IReadOnlyCollection<ParseError> errors,
            string rawInput = null)
        {
            Errors = errors;
            RawInput = rawInput;
            RawTokens = rawTokens;
            Symbol = symbol;
            Configuration = configuration;
            UnparsedTokens = unparsedTokens;
            UnmatchedTokens = unmatchedTokens;
        }

        public IReadOnlyCollection<ParseError> Errors { get; }
        public string RawInput { get; }
        public IReadOnlyCollection<string> RawTokens { get; }
        public SymbolSet Symbol { get; }
        public ParserConfiguration Configuration { get; }
        public IReadOnlyCollection<string> UnparsedTokens { get; }
        public IReadOnlyCollection<string> UnmatchedTokens { get; }
    }
}
