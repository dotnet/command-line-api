// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.CommandLine.Parsing;
using System.Linq;

namespace System.CommandLine.Help
{
    internal class VersionOption : Option
    {
        public VersionOption() : base("--version")
        {
            DisallowBinding = true;

            AddValidators();
        }

        public VersionOption(string[] aliases) : base(aliases)
        {
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
                    return result.LocalizationResources.VersionOptionCannotBeCombinedWithOtherArguments(result.Token?.Value ?? result.Symbol.Name);
                }

                return null;
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

        internal override Argument Argument => Argument.None();

        public override bool Equals(object obj)
        {
            return obj is VersionOption;
        }

        public override int GetHashCode()
        {
            return typeof(VersionOption).GetHashCode();
        }
    }
}