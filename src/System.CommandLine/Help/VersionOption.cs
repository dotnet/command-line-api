// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.CommandLine.Builder;
using System.CommandLine.Parsing;
using System.Linq;

namespace System.CommandLine.Help
{
    internal class VersionOption : Option<bool>
    {
        private readonly CommandLineBuilder _builder;
        private string? _description;

        public VersionOption(CommandLineBuilder builder) : base("--version")
        {
            _builder = builder;
            
            DisallowBinding = true;

            AddValidators();
        }

        public VersionOption(string[] aliases, CommandLineBuilder builder) : base(aliases)
        {
            _builder = builder;

            DisallowBinding = true;

            AddValidators();
        }

        private void AddValidators()
        {
            AddValidator(result =>
            {
                if (result.Parent is { } parent &&
                    parent.Children.Where(r => r.Symbol is not VersionOption)
                          .Any(IsNotImplicit))
                {
                    result.ErrorMessage =  result.LocalizationResources.VersionOptionCannotBeCombinedWithOtherArguments(result.Token?.Value ?? result.Symbol.Name);
                }
            });
        }

        private static bool IsNotImplicit(SymbolResult symbolResult)
        {
            return symbolResult switch
            {
                ArgumentResult argumentResult => !argumentResult.IsImplicit,
                OptionResult optionResult => !optionResult.IsImplicit,
                _ => true
            };
        }

        public override string? Description
        {
            get => _description ??= _builder.LocalizationResources.VersionOptionDescription();
            set => _description = value;
        }

        internal override Argument Argument => Argument.None();

        internal override bool IsGreedy => false;

        public override bool Equals(object? obj)
        {
            return obj is VersionOption;
        }

        public override int GetHashCode()
        {
            return typeof(VersionOption).GetHashCode();
        }
    }
}