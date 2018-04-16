// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using static Microsoft.DotNet.Cli.CommandLine.Accept;

namespace Microsoft.DotNet.Cli.CommandLine
{
    public class Command : Option
    {
        private static readonly Lazy<string> executableName =
            new Lazy<string>(() => Path.GetFileNameWithoutExtension(Assembly.GetEntryAssembly().Location));

        public Command(
            IReadOnlyCollection<Option> options) :
            this(executableName.Value, "", options, NoArguments())
        {
        }

        public Command(
            string name,
            string description,
            IReadOnlyCollection<Command> subcommands) :
            this(name, description, options: subcommands)
        {
            var commandNames = subcommands.SelectMany(o => o.Aliases).ToArray();

            ArgumentsRule =
                ExactlyOneCommandRequired()
                    .WithSuggestionsFrom(commandNames)
                    .And(ArgumentsRule);
        }

        public Command(
            string name,
            string description,
            IReadOnlyCollection<Option> options = null,
            ArgumentsRule arguments = null,
            bool treatUnmatchedTokensAsErrors = true) :
            base(new[] { name }, description, arguments, options)
        {
            TreatUnmatchedTokensAsErrors = treatUnmatchedTokensAsErrors;

            if (options != null && options.Any())
            {
                foreach (var option in options)
                {
                    option.Parent = this;
                    DefinedOptions.Add(option);
                }

                ArgumentsRule = ArgumentsRule.And(ZeroOrMoreOf(options.ToArray()));
            }
        }

        public override bool IsCommand => true;

        public bool TreatUnmatchedTokensAsErrors { get; } = true;
    }
}
