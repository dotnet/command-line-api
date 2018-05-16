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

        protected override bool ContainsSymbolWithAlias(ParsedSymbol option, string alias) =>
            option.Symbol.HasAlias(alias);

        protected override bool ContainsSymbolWithRawAlias(ParsedSymbol option, string alias) =>
            option.Symbol.HasRawAlias(alias);

        protected override IReadOnlyCollection<string> RawAliasesFor(ParsedSymbol option) =>
            option.Symbol.RawAliases;
    }
}
