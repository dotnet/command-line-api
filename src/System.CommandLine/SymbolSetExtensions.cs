using System.Linq;

namespace System.CommandLine
{
    static internal class SymbolSetExtensions
    {
        internal static CommandDefinition CommandDefinition(this SymbolSet symbols) =>
            symbols.FlattenBreadthFirst()
                   .Select(a => a.SymbolDefinition)
                   .OfType<CommandDefinition>()
                   .LastOrDefault();
    }
}