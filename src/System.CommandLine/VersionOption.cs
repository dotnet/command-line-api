// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.CommandLine.Parsing;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace System.CommandLine
{
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
                    parent.Children.Where(r => !(r is OptionResult optionResult && optionResult.Option is VersionOption))
                          .Any(NotImplicit))
                {
                    result.AddError(LocalizationResources.VersionOptionCannotBeCombinedWithOtherArguments(result.IdentifierToken?.Value ?? result.Option.Name));
                }
            });
        }

        private static bool NotImplicit(SymbolResult symbolResult)
        {
            return symbolResult switch
            {
                ArgumentResult argumentResult => !argumentResult.Implicit,
                OptionResult optionResult => !optionResult.Implicit,
                _ => true
            };
        }

        internal override bool Greedy => false;

        private sealed class VersionOptionAction : CliAction
        {
            public override int Invoke(ParseResult parseResult)
            {
                parseResult.Configuration.Output.WriteLine(CliRootCommand.ExecutableVersion);
                return 0;
            }

            public override Task<int> InvokeAsync(ParseResult parseResult, CancellationToken cancellationToken = default)
                => cancellationToken.IsCancellationRequested
                    ? Task.FromCanceled<int>(cancellationToken)
                    : Task.FromResult(Invoke(parseResult));
        }
    }
}