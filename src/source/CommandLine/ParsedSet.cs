using System.Collections.Generic;

namespace Microsoft.DotNet.Cli.CommandLine
{
    public class ParsedSet : OptionSet<Parsed>
    {
        // FIX: (ParsedSet) collapse the different OptionSet classes
        public ParsedSet()
        {
        }

        public ParsedSet(IReadOnlyCollection<Parsed> options) : base(options)
        {
        }

        protected override bool ContainsItemWithAlias(Parsed option, string alias) =>
            option.Option.HasAlias(alias);

        protected override bool ContainsItemWithRawAlias(Parsed option, string alias) =>
            option.Option.HasRawAlias(alias);

        protected override IReadOnlyCollection<string> RawAliasesFor(Parsed option) =>
            option.Option.RawAliases;
    }
}