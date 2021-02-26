// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace System.CommandLine.Help
{
    internal class HelpOption : Option
    {
        public HelpOption() : base(new[]
        {
            "-?",
            "/?",
            "-h",
            "/h",
            "--help"
        }, Resources.Instance.HelpOptionDescription())
        {
            DisallowBinding = true;
        }

        internal override Argument Argument
        {
            get => Argument.None;
        }

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