// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.CommandLine.Parsing;
using System.Linq;

namespace System.CommandLine.Help
{
    internal class VersionOption : Option<bool>
    {
        private readonly CommandLineBuilder _builder;
        private string? _description;

        public VersionOption(CommandLineBuilder builder) : base("--version", null, new Argument<bool> { Arity = ArgumentArity.Zero })
        {
            _builder = builder;
            
            AddValidators();
        }

        public VersionOption(string[] aliases, CommandLineBuilder builder) : base(aliases)
        {
            _builder = builder;

            AddValidators();
        }

        private void AddValidators()
        {
            Validators.Add(static result =>
            {
                if (result.Parent is CommandResult parent &&
                    parent.Children.Where(r => !(r is OptionResult optionResult && optionResult.Option is VersionOption))
                          .Any(IsNotImplicit))
                {
                    result.AddError(result.LocalizationResources.VersionOptionCannotBeCombinedWithOtherArguments(result.Token?.Value ?? result.Option.Name));
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