using System.Collections.Generic;

namespace System.CommandLine
{
    public class ParsedSymbolSet : SymbolSet<ParsedSymbol>
    {
        public ParsedSymbolSet()
        {
        }

        public ParsedSymbolSet(IReadOnlyCollection<ParsedSymbol> options) : base(options)
        {
        }

        protected override bool ContainsSymbolWithAlias(ParsedSymbol symbol, string alias) =>
            symbol.SymbolDefinition.HasAlias(alias);

        protected override bool ContainsSymbolWithRawAlias(ParsedSymbol symbol, string alias) =>
            symbol.SymbolDefinition.HasRawAlias(alias);

        protected override IReadOnlyCollection<string> RawAliasesFor(ParsedSymbol symbol) =>
            symbol.SymbolDefinition.RawAliases;
    }
}
