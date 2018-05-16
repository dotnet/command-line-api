using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace System.CommandLine
{
    public abstract class SymbolParser
    {
        private readonly ParserConfiguration configuration;

        protected SymbolParser(IReadOnlyCollection<SymbolDefinition> optionDefinitions) : this(new ParserConfiguration(optionDefinitions))
        {
        }

        protected SymbolParser(ParserConfiguration configuration)
        {
            this.configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
        }

        public SymbolDefinitionSet SymbolDefinitions => configuration.SymbolDefinitions;

        internal virtual RawParseResult ParseRaw(IReadOnlyCollection<string> rawTokens, string rawInput = null)
        {
            var unparsedTokens = new Queue<Token>(
                NormalizeRootCommand(rawTokens)
                    .Lex(configuration));
            var rootParsedOptions = new SymbolSet();
            var allParsedOptions = new List<Symbol>();
            var errors = new List<ParseError>();
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
                        SymbolDefinitions.SingleOrDefault(o => o.HasAlias(token.Value));

                    if (definedOption != null)
                    {
                        var parsedOption = allParsedOptions
                            .LastOrDefault(o => o.HasAlias(token.Value));

                        if (parsedOption == null)
                        {
                            parsedOption = Symbol.Create(definedOption, token.Value);

                            rootParsedOptions.Add(parsedOption);
                        }

                        allParsedOptions.Add(parsedOption);

                        continue;
                    }
                }

                var added = false;

                foreach (var parsedOption in Enumerable.Reverse(allParsedOptions))
                {
                    var option = parsedOption.TryTakeToken(token);

                    if (option != null)
                    {
                        allParsedOptions.Add(option);
                        added = true;
                        break;
                    }

                    if (token.Type == TokenType.Argument &&
                        parsedOption.SymbolDefinition is CommandDefinition)
                    {
                        break;
                    }
                }

                if (!added)
                {
                    unmatchedTokens.Add(token.Value);
                }
            }

            if (rootParsedOptions.Command()?.TreatUnmatchedTokensAsErrors == true)
            {
                errors.AddRange(
                    unmatchedTokens.Select(token => UnrecognizedArg(token)));
            }

            if (configuration.RootCommandIsImplicit)
            {
                rawTokens = rawTokens.Skip(1).ToArray();
                var parsedOptions = rootParsedOptions
                                     .SelectMany(o => o.Children)
                                     .ToArray();
                rootParsedOptions = new SymbolSet(parsedOptions);
            }

            return new RawParseResult(
                rawTokens,
                rootParsedOptions,
                configuration,
                unparsedTokens.Select(t => t.Value).ToArray(),
                unmatchedTokens,
                errors,
                rawInput);
        }

        internal IReadOnlyCollection<string> NormalizeRootCommand(IReadOnlyCollection<string> args)
        {
            if (configuration.RootCommandIsImplicit)
            {
                args = new[] { configuration.RootCommandDefinition.Name }.Concat(args).ToArray();
            }

            var firstArg = args.FirstOrDefault();

            if (SymbolDefinitions.Count != 1)
            {
                return args;
            }

            var commandName = SymbolDefinitions
                              .OfType<CommandDefinition>()
                              .SingleOrDefault()
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

        private static ParseError UnrecognizedArg(string arg) =>
            new ParseError(ValidationMessages.UnrecognizedCommandOrArgument(arg));
    }
}
