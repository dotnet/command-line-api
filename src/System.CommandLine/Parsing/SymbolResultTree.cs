// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Generic;

namespace System.CommandLine.Parsing
{
    internal sealed class SymbolResultTree : Dictionary<CliSymbol, CliSymbolResultInternal>
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
                Errors = new List<CliDiagnostic>(tokenizeErrors.Count);

                for (var i = 0; i < tokenizeErrors.Count; i++)
                {
                    Errors.Add(new CliDiagnostic(new("", "", tokenizeErrors[i], CliDiagnosticSeverity.Warning, null), []));
                }
            }
        }

        internal int ErrorCount => Errors?.Count ?? 0;

        internal CliArgumentResultInternal? GetResultInternal(CliArgument argument)
            => TryGetValue(argument, out CliSymbolResultInternal? result) ? (CliArgumentResultInternal)result : default;

        internal CliCommandResultInternal? GetResultInternal(CliCommand command)
            => TryGetValue(command, out var result) ? (CliCommandResultInternal)result : default;

        internal CliOptionResultInternal? GetResultInternal(CliOption option)
            => TryGetValue(option, out CliSymbolResultInternal? result) ? (CliOptionResultInternal)result : default;

        // TODO: Determine how this is used. It appears to be O^n in the size of the tree and so if it is called multiple times, we should reconsider to avoid O^(N*M)
        internal IEnumerable<CliSymbolResultInternal> GetChildren(CliSymbolResultInternal parent)
        {
            // Argument can't have children
            if (parent is not CliArgumentResultInternal)
            {
                foreach (KeyValuePair<CliSymbol, CliSymbolResultInternal> pair in this)
                {
                    if (ReferenceEquals(parent, pair.Value.Parent))
                    {
                        yield return pair.Value;
                    }
                }
            }
        }

        internal IReadOnlyDictionary<CliSymbol, CliValueResult> BuildValueResultDictionary()
        {
            var dict = new Dictionary<CliSymbol, CliValueResult>();
            foreach (KeyValuePair<CliSymbol, CliSymbolResultInternal> pair in this)
            {
                var result = pair.Value;
                if (result is CliOptionResultInternal optionResult)
                {
                    dict.Add(pair.Key, optionResult.ValueResult);
                    continue;
                }
                if (result is CliArgumentResultInternal argumentResult)
                {
                    dict.Add(pair.Key, argumentResult.ValueResult);
                    continue;
                }
            }
            return dict;
        }

        internal void AddError(CliDiagnostic parseError) => (Errors ??= new()).Add(parseError);
        internal void InsertFirstError(CliDiagnostic parseError) => (Errors ??= new()).Insert(0, parseError);

        internal void AddUnmatchedToken(CliToken token, CliCommandResultInternal commandResult, CliCommandResultInternal rootCommandResult)
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
            AddError(new CliDiagnostic(new("", "", LocalizationResources.UnrecognizedCommandOrArgument(token.Value), CliDiagnosticSeverity.Warning, null), [], symbolResult: commandResult));
            /*
        }

        public SymbolResult? GetResult(string name)
        {
            if (_symbolsByName is null)
            {
                _symbolsByName = new();
                PopulateSymbolsByName(_rootCommand);
            }
            */
        }

// TODO: symbolsbyname - this is inefficient
// results for some values may not be queried at all, dependent on other options
// so we could avoid using their value factories and adding them to the dictionary
// could we sort by name allowing us to do a binary search instead of allocating a dictionary?
// could we add codepaths that query for specific kinds of symbols so they don't have to search all symbols?
        private void PopulateSymbolsByName(CliCommand command)
        {
            if (command.HasArguments)
            {
                for (var i = 0; i < command.Arguments.Count; i++)
                {
                    AddToSymbolsByName(command.Arguments[i]);
                }
            }

            if (command.HasOptions)
            {
                for (var i = 0; i < command.Options.Count; i++)
                {
                    AddToSymbolsByName(command.Options[i]);
                }
            }

            if (command.HasSubcommands)
            {
                for (var i = 0; i < command.Subcommands.Count; i++)
                {
                    var childCommand = command.Subcommands[i];
                    AddToSymbolsByName(childCommand);
                    PopulateSymbolsByName(childCommand);
                }
            }

            // TODO: Explore removing closure here
            void AddToSymbolsByName(CliSymbol symbol)
            {
                if (_symbolsByName!.TryGetValue(symbol.Name, out var node))
                {
                    if (symbol.Name == node.Symbol.Name &&
                        symbol.FirstParent?.Symbol is { } parent &&
                        parent == node.Symbol.FirstParent?.Symbol)
                    {
                        throw new InvalidOperationException($"Command {parent.Name} has more than one child named \"{symbol.Name}\".");
                    }

                    _symbolsByName[symbol.Name] = new(symbol)
                    {
                        Next = node
                    };
                }
                else
                {
                    _symbolsByName[symbol.Name] = new(symbol);
                }
            }
        }
    }
}
