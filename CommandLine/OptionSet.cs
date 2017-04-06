using System;
using System.Collections.Generic;
using System.Linq;

namespace Microsoft.DotNet.Cli.CommandLine
{
    public class OptionSet : OptionSet<Option>
    {
        public override bool HasAlias(Option option, string alias) =>
            option.RawAliases.Contains(alias) ||
            option.Aliases.Contains(alias);

        protected override IReadOnlyCollection<string> AliasesFor(Option option) => option.RawAliases;
    }
}