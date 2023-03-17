// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace System.CommandLine.Help
{
    public sealed class HelpOption : Option<bool>
    {
        public HelpOption(string name, string[] aliases)
            : base(name, aliases, new Argument<bool>(name) { Arity = ArgumentArity.Zero })
        {
            AppliesToSelfAndChildren = true;
            Description = LocalizationResources.HelpOptionDescription();
            Action = new HelpAction();
        }

        public HelpOption() : this("--help", new[] { "-h", "/h", "-?", "/?" })
        {
        }

        internal override bool IsGreedy => false;

        public override bool Equals(object? obj) => obj is HelpOption;

        public override int GetHashCode() => typeof(HelpOption).GetHashCode();
    }
}