using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Microsoft.DotNet.Cli.CommandLine
{
    public abstract class Parser
    {
        private readonly ParserConfiguration configuration;

        protected Parser(params Option[] options) : this(new ParserConfiguration(options))
        {
        }

        protected Parser(char[] delimiters, params Option[] options) : this(new ParserConfiguration(options, argumentDelimiters: delimiters))
        {
        }

        protected Parser(ParserConfiguration configuration)
        {
            this.configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
        }

        public OptionSet DefinedOptions => configuration.DefinedOptions;

        public ParseResult Parse(string[] args) => ParseInternal(args, false);

        internal virtual ParseResult ParseInternal(
            IReadOnlyCollection<string> rawArgs,
            bool isProgressive)
        {
            var unparsedTokens = new Queue<Token>(
                NormalizeRootCommand(rawArgs)
                    .Lex(configuration));
            var rootAppliedOptions = new AppliedOptionSet();
            var allAppliedOptions = new List<AppliedOption>();
            var errors = new List<OptionError>();
            var unmatchedTokens = new List<string>();

            while (unparsedTokens.Any())
            {
                var token = unparsedTokens.Dequeue();

                if (token.Type == TokenType.EndOfArguments)
                {
                    // stop parsing further tokens
                    break;
                }

                if (token.Type != TokenType.Argument)
                {
                    var definedOption =
                        DefinedOptions.SingleOrDefault(o => o.HasAlias(token.Value));

                    if (definedOption != null)
                    {
                        var appliedOption = allAppliedOptions
                            .LastOrDefault(o => o.HasAlias(token.Value));

                        if (appliedOption == null)
                        {
                            appliedOption = new AppliedOption(
                                definedOption,
                                token.Value);
                            rootAppliedOptions.Add(appliedOption);
                        }

                        allAppliedOptions.Add(appliedOption);

                        continue;
                    }
                }

                var added = false;

                foreach (var appliedOption in Enumerable.Reverse(allAppliedOptions))
                {
                    var option = appliedOption.TryTakeToken(token);

                    if (option != null)
                    {
                        allAppliedOptions.Add(option);
                        added = true;
                        break;
                    }

                    if (token.Type == TokenType.Argument &&
                        appliedOption.Option.IsCommand)
                    {
                        break;
                    }
                }

                if (!added)
                {
                    unmatchedTokens.Add(token.Value);
                }
            }

            if (rootAppliedOptions.Command()?.TreatUnmatchedTokensAsErrors == true)
            {
                errors.AddRange(
                    unmatchedTokens.Select(UnrecognizedArg));
            }
            
            if (configuration.RootCommandIsImplicit)
            {
                rawArgs = rawArgs.Skip(1).ToArray();
                var appliedOptions = rootAppliedOptions
                                     .SelectMany(o => o.AppliedOptions)
                                     .ToArray();
                rootAppliedOptions = new AppliedOptionSet(appliedOptions);
            }

            return CreateParseResult(
                rawArgs,
                rootAppliedOptions,
                isProgressive,
                configuration,
                unparsedTokens.Select(t => t.Value).ToArray(),
                unmatchedTokens,
                errors);
        }

        protected abstract ParseResult CreateParseResult(
            IReadOnlyCollection<string> rawArgs, 
            AppliedOptionSet rootAppliedOptions, 
            bool isProgressive, 
            ParserConfiguration parserConfiguration, 
            string[] unparsedTokens, 
            List<string> unmatchedTokens, 
            List<OptionError> errors);

        internal IReadOnlyCollection<string> NormalizeRootCommand(IReadOnlyCollection<string> args)
        {
            if (configuration.RootCommandIsImplicit)
            {
                args = new[] { configuration.RootCommand.Name }.Concat(args).ToArray();
            }

            var firstArg = args.FirstOrDefault();

            if (DefinedOptions.Count != 1)
            {
                return args;
            }

            var commandName = DefinedOptions
                              .SingleOrDefault(o => o.IsCommand)
                              ?.Name;

            if (commandName == null ||
                string.Equals(firstArg, commandName, StringComparison.OrdinalIgnoreCase))
            {
                return args;
            }

            if (firstArg != null &&
                firstArg.Contains(Path.DirectorySeparatorChar) &&
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
            new OptionError(ValidationMessages.UnrecognizedCommandOrArgument(arg), arg);
    }
}
