using System.Collections.Generic;

namespace Microsoft.DotNet.Cli.CommandLine
{
    internal class RawParseResult
    {
        public RawParseResult(
            IReadOnlyCollection<string> rawTokens,
            ParsedSet parsed,
            ParserConfiguration configuration,
            IReadOnlyCollection<string> unparsedTokens,
            IReadOnlyCollection<string> unmatchedTokens,
            IReadOnlyCollection<OptionError> errors,
            string rawInput = null)
        {
            Errors = errors;
            RawInput = rawInput;
            RawTokens = rawTokens;
            Parsed = parsed;
            Configuration = configuration;
            UnparsedTokens = unparsedTokens;
            UnmatchedTokens = unmatchedTokens;
        }

        public IReadOnlyCollection<OptionError> Errors { get; }
        internal string RawInput { get; }
        public IReadOnlyCollection<string> RawTokens { get; }
        public ParsedSet Parsed { get; }
        public ParserConfiguration Configuration { get; }
        public IReadOnlyCollection<string> UnparsedTokens { get; }
        public IReadOnlyCollection<string> UnmatchedTokens { get; }
    }
}
