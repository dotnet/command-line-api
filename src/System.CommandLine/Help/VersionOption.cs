// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.CommandLine.Invocation;
using System.CommandLine.IO;
using System.CommandLine.Parsing;
using System.Linq;

namespace System.CommandLine.Help
{
    internal class VersionOption : Option<bool>
    {
        internal VersionOption(CommandLineBuilder builder)
            : base("--version", builder.LocalizationResources.VersionOptionDescription(), new Argument<bool> { Arity = ArgumentArity.Zero })
        {
            AddValidators();
        }

        internal VersionOption(string[] aliases, CommandLineBuilder builder)
            : base(aliases, builder.LocalizationResources.VersionOptionDescription())
        {
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

        internal override bool IsGreedy => false;

        public override bool Equals(object? obj) => obj is VersionOption;

        public override int GetHashCode() => typeof(VersionOption).GetHashCode();

        internal static void Handler(InvocationContext context)
            => context.Console.Out.WriteLine(RootCommand.ExecutableVersion);
    }
}