using System.Collections.Generic;

namespace Microsoft.DotNet.Cli.CommandLine
{
    public class ParsedSymbolSet : SymbolSet<ParsedSymbol>
    {
        public ParsedSymbolSet()
        {
        }

        public ParsedSymbolSet(IReadOnlyCollection<ParsedSymbol> options) : base(options)
        {
        }

        protected override bool ContainsItemWithAlias(ParsedSymbol option, string alias) =>
            option.Symbol.HasAlias(alias);

        protected override bool ContainsItemWithRawAlias(ParsedSymbol option, string alias) =>
            option.Symbol.HasRawAlias(alias);

        protected override IReadOnlyCollection<string> RawAliasesFor(ParsedSymbol option) =>
            option.Symbol.RawAliases;
    }
}