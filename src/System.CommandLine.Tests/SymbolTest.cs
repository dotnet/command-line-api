using System.Collections.Generic;

namespace System.CommandLine.Tests
{
    internal class SymbolTest : Symbol
    {
        internal SymbolTest(IReadOnlyCollection<string> aliases, string description, Argument argDef = null)
            : base(aliases, description, argDef)
        {
        }
    }
}