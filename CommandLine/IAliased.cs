using System.Collections.Generic;

namespace CommandLine
{
    public interface IAliased
    {
        IReadOnlyCollection<string> Aliases { get; }

        bool HasAlias(string alias);
    }
}