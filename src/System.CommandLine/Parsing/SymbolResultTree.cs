// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Generic;
using System.Linq;

namespace System.CommandLine.Parsing
{
    internal sealed class SymbolResultTree : Dictionary<CliSymbol, SymbolResult>
    {
        private readonly CliCommand _rootCommand;
        internal List<ParseError>? Errors;
        // TODO: unmatched tokens
        /*
                internal List<CliToken>? UnmatchedTokens;
        */

        // TODO: Looks like this is a SymboNode/linked list because a symbol may appear multiple
        // places in the tree and multiple symbols will have the same short name. The question is 
        // whether creating the multiple node instances is faster than just using lists. Could well be.
        internal SymbolResultTree(
            CliCommand rootCommand,
            List<string>? tokenizeErrors)
        {
            _rootCommand = rootCommand;

            if (tokenizeErrors is not null)
            {
                Errors = new List<ParseError>(tokenizeErrors.Count);

                for (var i = 0; i < tokenizeErrors.Count; i++)
                {
                    Errors.Add(new ParseError(tokenizeErrors[i]));
                }
            }
        }

        internal int ErrorCount => Errors?.Count ?? 0;

        internal ArgumentResult? GetResult(CliArgument argument)
            => TryGetValue(argument, out SymbolResult? result) ? (ArgumentResult)result : default;

        internal CommandResult? GetResult(CliCommand command)
            => TryGetValue(command, out var result) ? (CommandResult)result : default;

        internal OptionResult? GetResult(CliOption option)
            => TryGetValue(option, out SymbolResult? result) ? (OptionResult)result : default;

        // TODO: Determine how this is used. It appears to be O^n in the size of the tree and so if it is called multiple times, we should reconsider to avoid O^(N*M)
        internal IEnumerable<SymbolResult> GetChildren(SymbolResult parent)
        {
            // Argument can't have children
            if (parent is not ArgumentResult)
            {
                foreach (KeyValuePair<CliSymbol, SymbolResult> pair in this)
                {
                    if (ReferenceEquals(parent, pair.Value.Parent))
                    {
                        yield return pair.Value;
                    }
                }
            }
        }

        internal IReadOnlyDictionary<CliSymbol, ValueResult> BuildValueResultDictionary()
        {
            var dict = new Dictionary<CliSymbol, ValueResult>();
            foreach (KeyValuePair<CliSymbol, SymbolResult> pair in this)
            {
                var result = pair.Value;
                if (result is OptionResult optionResult)
                {
                    dict.Add(pair.Key, optionResult.ValueResult);
                    continue;
                }
                if (result is ArgumentResult argumentResult)
                {
                    dict.Add(pair.Key, argumentResult.ValueResult);
                    continue;
                }
            }
            return dict;
        }

        internal void AddError(ParseError parseError) => (Errors ??= new()).Add(parseError);
        internal void InsertFirstError(ParseError parseError) => (Errors ??= new()).Insert(0, parseError);

        internal void AddUnmatchedToken(CliToken token, CommandResult commandResult, CommandResult rootCommandResult)
        {
            /*
            // TODO: unmatched tokens
                        (UnmatchedTokens ??= new()).Add(token);

                        if (commandResult.Command.TreatUnmatchedTokensAsErrors)
                        {
                            if (commandResult != rootCommandResult && !rootCommandResult.Command.TreatUnmatchedTokensAsErrors)
                            {
                                return;
                            }

            */
            AddError(new ParseError(LocalizationResources.UnrecognizedCommandOrArgument(token.Value), commandResult));
            //            }
        }

    }
}