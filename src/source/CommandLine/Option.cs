// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Linq;

namespace Microsoft.DotNet.Cli.CommandLine
{
    public class Option
    {
        private readonly HashSet<string> aliases = new HashSet<string>();

        private readonly Suggest suggest;
        private readonly HashSet<string> rawAliases;

        public Option(
            string[] aliases,
            string help,
            ArgumentsRule arguments = null) :
            this(aliases, help, arguments, null)
        {
        }

        protected internal Option(
            string[] aliases,
            string help,
            ArgumentsRule arguments = null,
            Option[] options = null)
        {
            if (aliases == null)
            {
                throw new ArgumentNullException(nameof(aliases));
            }

            if (!aliases.Any())
            {
                throw new ArgumentException("An option must have at least one alias.");
            }

            if (aliases.Any(string.IsNullOrWhiteSpace))
            {
                throw new ArgumentException("An option alias cannot be null, empty, or consist entirely of whitespace.");
            }

            rawAliases = new HashSet<string>(aliases);

            foreach (var alias in aliases)
            {
                this.aliases.Add(alias.RemovePrefix());
            }

            HelpText = help;

            Name = aliases
                .Select(a => a.RemovePrefix())
                .OrderBy(a => a.Length)
                .Last();

            if (options != null && options.Any())
            {
                foreach (var option in options)
                {
                    option.Parent = this;
                    DefinedOptions.Add(option);
                }
            }

            ArgumentsRule = arguments ?? Accept.NoArguments();

            if (options != null)
            {
                ArgumentsRule = ArgumentsRule.And(Accept.ZeroOrMoreOf(options));
            }

            AllowedValues = ArgumentsRule.AllowedValues;

            suggest = ArgumentsRule.Suggest;
        }

        public IReadOnlyCollection<string> Aliases => aliases.ToArray();

        public IReadOnlyCollection<string> RawAliases => rawAliases.ToArray();

        protected internal virtual IReadOnlyCollection<string> AllowedValues { get; }

        public OptionSet DefinedOptions { get; } = new OptionSet();

        public string HelpText { get; }

        public ArgumentsRule ArgumentsRule { get; protected set; }

        public string Name { get; }

        public IEnumerable<string> Suggest(ParseResult parseResult) => suggest(parseResult);

        internal virtual bool IsCommand => false;

        public Option Parent { get; protected internal set; }

        public bool HasAlias(string alias) => aliases.Contains(alias.RemovePrefix());

        public bool HasRawAlias(string alias) => rawAliases.Contains(alias);

        public Option this[string alias] => DefinedOptions[alias];

        public override string ToString() =>
            IsCommand
                ? Name
                : RawAliases.Single(a => a.RemovePrefix() == Name);
    }
}
