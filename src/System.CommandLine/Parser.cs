// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace System.CommandLine
{
    public class Parser
    {
        private readonly ParserConfiguration _configuration;

        public Parser(params SymbolDefinition[] symbolDefinitions) : this(new ParserConfiguration(symbolDefinitions))
        {
        }

        public Parser(ParserConfiguration configuration)
        {
            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
        }

        public SymbolDefinitionSet SymbolDefinitions => _configuration.SymbolDefinitions;

        public virtual ParseResult Parse(IReadOnlyCollection<string> rawTokens, string rawInput = null)
        {
            var lexResult = NormalizeRootCommand(rawTokens).Lex(_configuration);
            var unparsedTokens = new Queue<Token>(lexResult.Tokens);
            var rootSymbols = new SymbolSet();
            var allSymbols = new List<Symbol>();
            var errors = new List<ParseError>(lexResult.Errors);
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
                        var parsedOption = allSymbols
                            .LastOrDefault(o => o.HasAlias(token.Value));

                        if (parsedOption == null)
                        {
                            parsedOption = Symbol.Create(definedOption, token.Value, validationMessages: _configuration.ValidationMessages);

                            rootSymbols.Add(parsedOption);
                        }

                        allSymbols.Add(parsedOption);

                        continue;
                    }
                }

                var added = false;

                foreach (var parsedOption in Enumerable.Reverse(allSymbols))
                {
                    var option = parsedOption.TryTakeToken(token);

                    if (option != null)
                    {
                        allSymbols.Add(option);
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

            if (rootSymbols.CommandDefinition()?.TreatUnmatchedTokensAsErrors == true)
            {
                errors.AddRange(
                    unmatchedTokens.Select(token => new ParseError(_configuration.ValidationMessages.UnrecognizedCommandOrArgument(token))));
            }

            return new ParseResult(
                rawTokens,
                rootSymbols,
                _configuration,
                unparsedTokens.Select(t => t.Value).ToArray(),
                unmatchedTokens,
                errors,
                rawInput);
        }

        internal IReadOnlyCollection<string> NormalizeRootCommand(IReadOnlyCollection<string> args)
        {
            if (_configuration.RootCommandIsImplicit)
            {
                args = new[] { _configuration.RootCommandDefinition.Name }.Concat(args).ToArray();
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
    }
}
