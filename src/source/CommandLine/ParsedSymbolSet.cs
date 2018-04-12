using System.Collections.Generic;

namespace Microsoft.DotNet.Cli.CommandLine
{
    public class ParsedSymbolSet : OptionSet<ParsedSymbol>
    {
        // FIX: (ParsedSet) collapse the different OptionSet classes
        public ParsedSymbolSet()
        {
        }

        public ParsedSymbolSet(IReadOnlyCollection<ParsedSymbol> options) : base(options)
        {
        }

        protected override bool ContainsItemWithAlias(ParsedSymbol option, string alias) =>
            option.Option.HasAlias(alias);

        protected override bool ContainsItemWithRawAlias(ParsedSymbol option, string alias) =>
            option.Option.HasRawAlias(alias);

        protected override IReadOnlyCollection<string> RawAliasesFor(ParsedSymbol option) =>
            option.Option.RawAliases;
    }
}