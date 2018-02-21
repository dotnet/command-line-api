// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
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
            params Option[] options) :
            base(new[] { executableName.Value }, "", NoArguments(), options)
        {
        }

        public Command(
            string name,
            string description,
            Option[] options = null,
            ArgumentsRule arguments = null,
            bool treatUnmatchedTokensAsErrors = true) :
            base(new[] { name }, description, arguments, options)
        {
            TreatUnmatchedTokensAsErrors = treatUnmatchedTokensAsErrors;
        }

        public Command(
            string name,
            string description,
            Command[] subcommands) :
            base(new[] { name }, description, options: subcommands)
        {
            var commandNames = subcommands.SelectMany(o => o.Aliases).ToArray();

            ArgumentsRule =
                ExactlyOneCommandRequired()
                    .WithSuggestionsFrom(commandNames)
                    .And(ArgumentsRule);
        }

        internal override bool IsCommand => true;

        public bool TreatUnmatchedTokensAsErrors { get; } = true;
    }
}
