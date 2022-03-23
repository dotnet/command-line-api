// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace System.CommandLine.Help
{
    internal class HelpOption : Option<bool>
    {
        private readonly Func<LocalizationResources> _localizationResources;
        private string? _description;

        public HelpOption(string[] aliases, Func<LocalizationResources> getLocalizationResources)
            : base(aliases)
        {
            _localizationResources = getLocalizationResources;
            DisallowBinding = true;
        }

        public HelpOption(Func<LocalizationResources> getLocalizationResources) : this(new[]
        {
            "-h",
            "/h",
            "--help",
            "-?",
            "/?"
        }, getLocalizationResources)
        {
        }

        public override string? Description
        {
            get => _description ??= _localizationResources().HelpOptionDescription();
            set => _description = value;
        }

        internal override Argument Argument => Argument.None();

        internal override bool IsGreedy => false;

        public override bool Equals(object? obj)
        {
            return obj is HelpOption;
        }

        public override int GetHashCode()
        {
            return typeof(HelpOption).GetHashCode();
        }
    }
}