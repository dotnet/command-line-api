using System.Collections.Generic;

namespace System.CommandLine
{
    public class SymbolSet : AliasedSet<Symbol>
    {
        public SymbolSet()
        {
        }

        public SymbolSet(IReadOnlyCollection<Symbol> symbols) : base(symbols)
        {
        }

        protected override bool ContainsItemWithAlias(Symbol item, string alias) =>
            item.SymbolDefinition.HasAlias(alias);

        protected override bool ContainsItemWithRawAlias(Symbol item, string alias) =>
            item.SymbolDefinition.HasRawAlias(alias);

        protected override IReadOnlyCollection<string> GetAliases(Symbol item) =>
            item.SymbolDefinition.RawAliases;
    }
}
