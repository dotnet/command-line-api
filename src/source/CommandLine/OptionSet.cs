using System;
using System.Collections.Generic;
using System.Linq;

namespace Microsoft.DotNet.Cli.CommandLine
{
    public class OptionSet : OptionSet<Option>
    {
        protected override bool ContainsItemWithAlias(Option option, string alias) =>
            option.HasAlias(alias);

        protected override bool ContainsItemWithRawAlias(Option option, string alias) =>
            option.HasRawAlias(alias);

        protected override IReadOnlyCollection<string> RawAliasesFor(Option option) =>
            option.RawAliases;
    }
}
