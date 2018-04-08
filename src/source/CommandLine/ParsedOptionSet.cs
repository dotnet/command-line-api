using System.Collections.Generic;

namespace Microsoft.DotNet.Cli.CommandLine
{
    public class ParsedOptionSet : OptionSet<ParsedOption>
    {
        public ParsedOptionSet()
        {
        }

        public ParsedOptionSet(IReadOnlyCollection<ParsedOption> options) : base(options)
        {
        }

        protected override bool HasAlias(ParsedOption option, string alias) =>
            option.Option.HasAlias(alias);

        protected override bool HasRawAlias(ParsedOption option, string alias) =>
            option.Option.HasRawAlias(alias);

        protected override IReadOnlyCollection<string> RawAliasesFor(ParsedOption option) =>
            option.Option.RawAliases;
    }
}
