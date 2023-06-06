// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.CommandLine.Invocation;
using System.CommandLine.Parsing;
using System.Linq;

namespace System.CommandLine
{
    /// <summary>
    /// A standard option that indicates that version information should be displayed for the app.
    /// </summary>
    public sealed class VersionOption : CliOption<bool>
    {
        private CliAction? _action;

        /// <summary>
        /// When added to a <see cref="CliCommand"/>, it enables the use of a <c>--version</c> option, which when specified in command line input will short circuit normal command handling and instead write out version information before exiting.
        /// </summary>
        public VersionOption() : this("--version", Array.Empty<string>())
        {
        }

        /// <summary>
        /// When added to a <see cref="CliCommand"/>, it enables the use of a provided option name and aliases, which when specified in command line input will short circuit normal command handling and instead write out version information before exiting.
        /// </summary>
        public VersionOption(string name, params string[] aliases)
            : base(name, aliases, new CliArgument<bool>("--version") { Arity = ArgumentArity.Zero })
        {
            Description = LocalizationResources.VersionOptionDescription();
            AddValidators();
        }

        /// <inheritdoc />
        public override CliAction? Action
        {
            get => _action ??= new VersionOptionAction();
            set => _action = value ?? throw new ArgumentNullException(nameof(value));
        }

        private void AddValidators()
        {
            Validators.Add(static result =>
            {
                if (result.Parent is CommandResult parent &&
                    parent.Children.Any(r => r is not OptionResult { Option: VersionOption }))
                {
                    result.AddError(LocalizationResources.VersionOptionCannotBeCombinedWithOtherArguments(result.IdentifierToken?.Value ?? result.Option.Name));
                }
            });
        }

        internal override bool Greedy => false;

        private sealed class VersionOptionAction : SynchronousCliAction
        {
            public override int Invoke(ParseResult parseResult)
            {
                parseResult.Configuration.Output.WriteLine(CliRootCommand.ExecutableVersion);
                return 0;
            }
        }
    }
}