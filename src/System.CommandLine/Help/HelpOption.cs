// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.CommandLine.Builder;

namespace System.CommandLine.Help
{
    internal class HelpOption : Option
    {
        private readonly CommandLineBuilder _builder;
        private string? _description;

        public HelpOption(string[] aliases, CommandLineBuilder builder)
            : base(aliases)
        {
            _builder = builder;
            DisallowBinding = true;
        }

        public HelpOption(CommandLineBuilder builder) : this(new[]
        {
            "-h",
            "/h",
            "--help",
            "-?",
            "/?"
        }, builder)
        {
        }

        public override string? Description
        {
            get => _description ??= _builder.Resources.HelpOptionDescription();
            set => _description = value;
        }

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