// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Generic;

namespace System.CommandLine.Parsing
{
    internal sealed class SymbolResultTree : Dictionary<Symbol, SymbolResult>
    {
        private readonly Command _rootCommand;
        internal List<ParseError>? Errors;
        internal List<Token>? UnmatchedTokens;
        private Dictionary<string, SymbolNode>? _symbolsByName;

        internal SymbolResultTree(
            Command rootCommand, 
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

        internal ArgumentResult? GetResult(Argument argument)
            => TryGetValue(argument, out SymbolResult? result) ? (ArgumentResult)result : default;

        internal CommandResult? GetResult(Command command)
            => TryGetValue(command, out var result) ? (CommandResult)result : default;

        internal OptionResult? GetResult(Option option)
            => TryGetValue(option, out SymbolResult? result) ? (OptionResult)result : default;

        internal DirectiveResult? GetResult(Directive directive)
            => TryGetValue(directive, out SymbolResult? result) ? (DirectiveResult)result : default;

        internal IEnumerable<SymbolResult> GetChildren(SymbolResult parent)
        {
            if (parent is not ArgumentResult)
            {
                foreach (KeyValuePair<Symbol, SymbolResult> pair in this)
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

        internal void AddUnmatchedToken(Token token, CommandResult commandResult, CommandResult rootCommandResult)
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

        private void PopulateSymbolsByName(Command command)
        {
            if (command.HasArguments)
            {
                for (var i = 0; i < command.Arguments.Count; i++)
                {
                    AddToSymbolsByName(command.Arguments[i], command);
                }
            }

            if (command.HasOptions)
            {
                for (var i = 0; i < command.Options.Count; i++)
                {
                    AddToSymbolsByName(command.Options[i], command);
                }
            }

            if (command.HasSubcommands)
            {
                for (var i = 0; i < command.Subcommands.Count; i++)
                {
                    var childCommand = command.Subcommands[i];
                    AddToSymbolsByName(childCommand, command);
                    PopulateSymbolsByName(childCommand);
                }
            }

            void AddToSymbolsByName(Symbol symbol, Command parent)
            {
                if (_symbolsByName!.TryGetValue(symbol.Name, out var node))
                {
                    var current = node;
                    do
                    {
                        // The same symbol can be added to multiple commands and have multiple parents.
                        // We can't allow for name duplicates within the same command.
                        if (ReferenceEquals(current.Parent, parent))
                        {
                            throw new InvalidOperationException($"Command {parent.Name} has more than one child named \"{symbol.Name}\".");
                        }
                        current = current.Next;
                    } while (current is not null);

                    _symbolsByName[symbol.Name] = new(symbol, parent)
                    {
                        Next = node
                    };
                }
                else
                {
                    _symbolsByName[symbol.Name] = new(symbol, parent);
                }
            }
        }
    }
}