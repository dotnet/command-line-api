using System.Collections.Generic;

namespace Microsoft.DotNet.Cli.CommandLine
{
    public class CommandParser : Parser
    {
        public CommandParser(params Command[] commands) : base(commands)
        {
        }

        protected override ParseResult CreateParseResult(
            IReadOnlyCollection<string> rawArgs,
            ParsedOptionSet rootParsedOptions,
            bool isProgressive,
            ParserConfiguration parserConfiguration,
            string[] unparsedTokens,
            List<string> unmatchedTokens,
            List<OptionError> errors)
        {
            return new CommandParseResult(
                rawArgs,
                rootParsedOptions,
                isProgressive,
                parserConfiguration,
                unparsedTokens,
                unmatchedTokens,
                errors);
        }
    }
}