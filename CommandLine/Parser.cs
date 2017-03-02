// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Microsoft.DotNet.Cli.CommandLine
{
    public class Parser
    {
        public Parser(params Option[] options)
        {
            if (options == null)
            {
                throw new ArgumentNullException(nameof(options));
            }

            DefinedOptions.AddRange(options);
        }

        public OptionSet<Option> DefinedOptions { get; } = new OptionSet<Option>();

        public ParseResult Parse(string[] args) => Parse(args, false);

        internal ParseResult Parse(
            IReadOnlyCollection<string> rawArgs,
            bool isProgressive)
        {
            var unparsedTokens = new Queue<string>(Normalize(rawArgs).Lex());
            var appliedOptions = new OptionSet<AppliedOption>();
            var errors = new List<OptionError>();
            var unmatchedTokens = new List<string>();

            AppliedOption currentOption = null;

            string arg;

            while (unparsedTokens.Any())
            {
                arg = unparsedTokens.Dequeue();

                if (arg == "--")
                {
                    // stop parsing further args
                    break;
                }

                if (DefinedOptions.Any(o => o.HasAlias(arg)))
                {
                    var option = appliedOptions.SingleOrDefault(o => o.HasAlias(arg));

                    var alreadySeen = option != null;

                    if (!alreadySeen)
                    {
                        option = new AppliedOption(
                            DefinedOptions.Single(s => s.HasAlias(arg)),
                            arg);

                        appliedOptions.Add(option);
                    }

                    if (!option.Option.IsCommand || !alreadySeen)
                    {
                        currentOption = option;
                        continue;
                    }
                }

                unmatchedTokens.Add(arg);

                if (currentOption != null)
                {
                    unmatchedTokens = currentOption
                        .TryTakeTokens(unmatchedTokens.ToArray())
                        .ToList();
                }
            }

            errors.AddRange(
                unmatchedTokens.Select(UnrecognizedArg));

            return new ParseResult(
                this,
                rawArgs,
                appliedOptions,
                isProgressive,
                unparsedTokens,
                unmatchedTokens,
                errors);
        }

        public IReadOnlyCollection<string> Normalize(IReadOnlyCollection<string> args)
        {
            var firstArg = args.First();

            if (DefinedOptions.Count != 1)
            {
                return args;
            }

            var commandName = DefinedOptions.Single().Name;

            if (firstArg.Equals(commandName, StringComparison.OrdinalIgnoreCase))
            {
                return args;
            }

            if (firstArg.Contains(Path.DirectorySeparatorChar) &&
                (firstArg.EndsWith(commandName, StringComparison.OrdinalIgnoreCase) ||
                 firstArg.EndsWith($"{commandName}.exe", StringComparison.OrdinalIgnoreCase)))
            {
                args = new[] { commandName }.Concat(args.Skip(1)).ToArray();
            }
            else
            {
                args = new[] { commandName }.Concat(args).ToArray();
            }

            return args;
        }

        private static OptionError UnrecognizedArg(string arg) =>
            new OptionError(
                $"Option '{arg}' is not recognized.", arg);
    }
}