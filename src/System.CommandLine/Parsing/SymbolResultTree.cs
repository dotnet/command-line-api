// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Generic;

namespace System.CommandLine.Parsing
{
    internal sealed class SymbolResultTree : Dictionary<CliSymbol, SymbolResult>
    {
        private readonly CliCommand _rootCommand;
        internal List<ParseError>? Errors;
        internal List<CliToken>? UnmatchedTokens;
        private Dictionary<string, SymbolNode>? _symbolsByName;

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

        internal DirectiveResult? GetResult(CliDirective directive)
            => TryGetValue(directive, out SymbolResult? result) ? (DirectiveResult)result : default;

        internal IEnumerable<SymbolResult> GetChildren(SymbolResult parent)
        {
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

        internal void AddError(ParseError parseError) => (Errors ??= new()).Add(parseError);

        internal void InsertFirstError(ParseError parseError) => (Errors ??= new()).Insert(0, parseError);

        internal void AddUnmatchedToken(CliToken token, CommandResult commandResult, CommandResult rootCommandResult)
        {
            (UnmatchedTokens ??= new()).Add(token);

            if (commandResult.Command.TreatUnmatchedTokensAsErrors)
            {
                if (commandResult != rootCommandResult && !rootCommandResult.Command.TreatUnmatchedTokensAsErrors)
                {
                    return;
                }

                AddError(new ParseError(LocalizationResources.UnrecognizedCommandOrArgument(token.Value), commandResult));
            }
        }

        public SymbolResult? GetResult(string name)
        {
            if (_symbolsByName is null)
            {
                _symbolsByName = new();  
                PopulateSymbolsByName(_rootCommand);
            }
          
            if (!_symbolsByName.TryGetValue(name, out SymbolNode? node))
            {
                throw new ArgumentException($"No symbol result found with name \"{name}\".");
            }

            while (node is not null)
            {
                if (TryGetValue(node.Symbol, out var result))
                {
                    return result;
                }

                node = node.Next;
            }

            return null;
        }

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