using System.Collections.Generic;

namespace Microsoft.DotNet.Cli.CommandLine
{
    public interface IAliased
    {
        IReadOnlyCollection<string> Aliases { get; }

        bool HasAlias(string alias);
    }
}