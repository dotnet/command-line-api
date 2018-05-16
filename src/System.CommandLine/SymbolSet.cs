using System.Collections.Generic;

namespace System.CommandLine
{
    public class SymbolSet : SymbolSet<Symbol>
    {
        public SymbolSet()
        {
        }

        public SymbolSet(IReadOnlyCollection<Symbol> symbols) : base(symbols)
        {
        }

        protected override bool ContainsSymbolWithAlias(Symbol symbol, string alias) =>
            symbol.SymbolDefinition.HasAlias(alias);

        protected override bool ContainsSymbolWithRawAlias(Symbol symbol, string alias) =>
            symbol.SymbolDefinition.HasRawAlias(alias);

        protected override IReadOnlyCollection<string> RawAliasesFor(Symbol symbol) =>
            symbol.SymbolDefinition.RawAliases;
    }
}
