using System.Collections.Generic;

namespace Microsoft.DotNet.Cli.CommandLine
{
    public class AppliedOptionSet : OptionSet<AppliedOption>
    {
        public override bool HasAlias(AppliedOption option, string alias) => option.HasAlias(alias);

        protected override IReadOnlyCollection<string> AliasesFor(AppliedOption option) => option.Aliases;
    }
}