using System.Collections.Generic;

namespace System.CommandLine.Tests
{
    internal class TestSymbol : Symbol
    {
        internal TestSymbol(IReadOnlyCollection<string> aliases, string description, Argument argDef = null)
            : base(aliases, description, argDef)
        {
        }
    }
}
