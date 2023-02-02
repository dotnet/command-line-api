// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.CommandLine.Invocation;
using System.CommandLine.IO;
using System.CommandLine.Parsing;

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
                CommandResult parent = (CommandResult)result.Parent!;

                bool setHandler = true;
                foreach (SymbolResult sibling in parent.Children)
                {
                    setHandler = sibling switch
                    {
                        OptionResult optionResult => optionResult.IsImplicit || optionResult.Option is VersionOption,
                        ArgumentResult argumentResult => argumentResult.IsImplicit,
                        _ => false
                    };

                    if (!setHandler)
                    {
                        result.AddError(result.LocalizationResources.VersionOptionCannotBeCombinedWithOtherArguments(result.Token?.Value ?? result.Option.Name));
                        break;
                    }
                }

                if (setHandler)
                {
                    parent.Command.Handler = new AnonymousCommandHandler(static context =>
                    {
                        context.Console.Out.WriteLine(RootCommand.ExecutableVersion);
                    });
                }
            });
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