// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace System.CommandLine.Help
{
    internal class HelpOption : Option
    {
        public HelpOption() : base(new[]
        {
            "-h",
            "/h",
            "--help",
            "-?",
            "/?"
        }, "Show help and usage information")
        {
        }

        public override Argument Argument
        {
            get => Argument.None;
            set => throw new NotSupportedException();
        }

        protected bool Equals(HelpOption other)
        {
            return other != null;
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