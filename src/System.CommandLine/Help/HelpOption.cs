// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace System.CommandLine.Help
{
    internal class HelpOption : Option
    {
        public HelpOption(string[] aliases)
            : base(aliases)
        {
            DisallowBinding = true;
        }

        public HelpOption() : this(new[]
        {
            "-h",
            "/h",
            "--help",
            "-?",
            "/?"
        })
        { }

        internal override Argument Argument => Argument.None();

        public override bool Equals(object obj)
        {
            return obj is HelpOption;
        }

        public override int GetHashCode()
        {
            return typeof(HelpOption).GetHashCode();
        }
    }
}