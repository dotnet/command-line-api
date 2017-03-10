// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Authentication.ExtendedProtection;

namespace Microsoft.DotNet.Cli.CommandLine
{
    public class Parser
    {
        private static readonly char[] defaultTokenSplitDelimiters = { '=', ':' };

        private readonly char[] tokenSplitDelimiters = null;

        public Parser(params Option[] options)
        {
            if (options == null)
            {
                throw new ArgumentNullException(nameof(options));
            }

            if (!options.Any())
            {
                throw new ArgumentException("You must specify at least one option.");
            }

            DefinedOptions.AddRange(options);
            tokenSplitDelimiters = defaultTokenSplitDelimiters;
        }

        public Parser(char[] delimiters, params Option[] options) : this(options)
        {
            tokenSplitDelimiters = delimiters ?? defaultTokenSplitDelimiters;
        }

        public OptionSet<Option> DefinedOptions { get; } = new OptionSet<Option>();

        public ParseResult Parse(string[] args) => Parse(args, false);

        internal ParseResult Parse(
            IReadOnlyCollection<string> rawArgs,
            bool isProgressive)
        {
            var knownTokens = new HashSet<string>(
                DefinedOptions
                    .FlattenBreadthFirst()
                    .SelectMany(o => o.RawAliases));

            var unparsedTokens = new Queue<Token>(
                NormalizeRootCommand(rawArgs)
                    .Lex(knownTokens, tokenSplitDelimiters));
            var appliedOptions = new OptionSet<AppliedOption>();
            var errors = new List<OptionError>();
            var unmatchedTokens = new List<string>();

            AppliedOption currentOption = null;

            Token arg;

            while (unparsedTokens.Any())
            {
                arg = unparsedTokens.Dequeue();

                if (arg.Value == "--")
                {
                    // stop parsing further args
                    break;
                }

                if (DefinedOptions.Any(o => o.HasAlias(arg.Value)))
                {
                    var option = appliedOptions.SingleOrDefault(o => o.HasAlias(arg.Value));

                    var alreadySeen = option != null;

                    if (!alreadySeen)
                    {
                        option = new AppliedOption(
                            DefinedOptions.Single(s => s.HasAlias(arg.Value)),
                            arg.Value);

                        appliedOptions.Add(option);
                    }

                    if (!option.Option.IsCommand || !alreadySeen)
                    {
                        currentOption = option;
                        continue;
                    }
                }

                unmatchedTokens.Add(arg.Value);

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
                rawArgs,
                appliedOptions,
                isProgressive,
                unparsedTokens.Select(t => t.Value).ToArray(),
                unmatchedTokens,
                errors);
        }

        public IReadOnlyCollection<string> NormalizeRootCommand(IReadOnlyCollection<string> args)
        {
            var firstArg = args.First();

            if (DefinedOptions.Count != 1)
            {
                return args;
            }

            var commandName = DefinedOptions
                .SingleOrDefault(o => o.IsCommand)
                ?.Name;

            if (commandName == null ||
                firstArg.Equals(commandName, StringComparison.OrdinalIgnoreCase))
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

    internal enum TokenType
    {
        Argument,
        Command,
        Option
    }

    internal class Token
    {
        public Token(string value, TokenType type)
        {
            Value = value;
            Type = type;
        }

        public string Value { get; }
        public TokenType Type { get; }

        public override string ToString()
        {
            return $"{Type}: {Value}";
        }
    }
}