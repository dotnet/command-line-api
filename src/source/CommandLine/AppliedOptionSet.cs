using System.Collections.Generic;

namespace Microsoft.DotNet.Cli.CommandLine
{
    public class AppliedOptionSet : OptionSet<AppliedOption>
    {
        public AppliedOptionSet()
        {
        }

        public AppliedOptionSet(IReadOnlyCollection<AppliedOption> options) : base(options)
        {
        }

        protected override bool HasAlias(AppliedOption option, string alias) =>
            option.Option.HasAlias(alias);

        protected override bool HasRawAlias(AppliedOption option, string alias) =>
            option.Option.HasRawAlias(alias);

        protected override IReadOnlyCollection<string> RawAliasesFor(AppliedOption option) =>
            option.Option.RawAliases;
    }
}
