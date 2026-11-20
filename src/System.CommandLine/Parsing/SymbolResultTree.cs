// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Generic;

namespace System.CommandLine.Parsing
{
    internal sealed class SymbolResultTree : Dictionary<Symbol, SymbolResult>
    {
        private sealed class IndexedSymbol
        {
            internal IndexedSymbol(Symbol symbol, Command parent)
            {
                Symbol = symbol;
                Parent = parent;
            }

            internal Symbol Symbol { get; }

            internal Command Parent { get; }

            internal IndexedSymbol? Next { get; set; }
        }

        private readonly Command? _rootCommand;
        internal CommandResult? InnermostCommandResult;
        internal List<ParseError>? Errors;
        internal List<Token>? UnmatchedTokens;
        private Dictionary<string, IndexedSymbol>? _parsedSymbolsByName;
        private CommandResult? _parsedSymbolsCommandResult;
        private Dictionary<string, IndexedSymbol>? _symbolsByName;

        internal SymbolResultTree(
            Command? rootCommand = null, 
            List<string>? tokenizeErrors = null)
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
            // Always consult the parsed command chain first so:
            // 1. Lookups for options/arguments/subcommands (including uncreated lazy sibling subcommands)
            //    on the parsed command chain never materialize unrelated lazy subtrees.
            // 2. Innermost-parsed-command precedence remains authoritative even after a prior
            //    fallback lookup has populated _symbolsByName.
            if (InnermostCommandResult is not null)
            {
                if (_parsedSymbolsByName is null || !ReferenceEquals(_parsedSymbolsCommandResult, InnermostCommandResult))
                {
                    _parsedSymbolsByName = new(StringComparer.Ordinal);
                    _parsedSymbolsCommandResult = InnermostCommandResult;
                    PopulateParsedCommandChain(InnermostCommandResult, _parsedSymbolsByName);
                }

                if (_parsedSymbolsByName.TryGetValue(name, out IndexedSymbol? parsedNode))
                {
                    ThrowIfUnmaterializedSubcommandDuplicatesName(InnermostCommandResult, name);
                    return FindMatchingResult(parsedNode);
                }

                if (HasUnmaterializedSubcommandOnParsedChain(InnermostCommandResult, name))
                {
                    return null;
                }
            }

            if (_symbolsByName is null && _rootCommand is not null)
            {
                _symbolsByName = new(StringComparer.Ordinal);
                PopulateSymbolsByName(_rootCommand, _symbolsByName);
            }

            if (_symbolsByName is null || !_symbolsByName.TryGetValue(name, out IndexedSymbol? node))
            {
                throw new ArgumentException($"No symbol result found with name \"{name}\".");
            }

            return FindMatchingResult(node);
        }

        private SymbolResult? FindMatchingResult(IndexedSymbol? node)
        {
            while (node is not null)
            {
                if (TryGetValue(node.Symbol, out SymbolResult? result))
                {
                    return result;
                }

                node = node.Next;
            }

            return null;
        }

        private static void PopulateParsedCommandChain(CommandResult current, Dictionary<string, IndexedSymbol> target)
        {
            if (current.Parent is CommandResult parent)
            {
                PopulateParsedCommandChain(parent, target);
            }

            Command command = current.Command;

            if (command.HasArguments)
            {
                for (int i = 0; i < command.Arguments.Count; i++)
                {
                    Argument arg = command.Arguments[i];
                    AddIndexedSymbol(target, arg.Name, arg, command);
                }
            }

            if (command.HasOptions)
            {
                for (int i = 0; i < command.Options.Count; i++)
                {
                    Option opt = command.Options[i];
                    AddIndexedSymbol(target, opt.Name, opt, command);
                }
            }

            if (command._subcommands is { Count: > 0 } subs)
            {
                for (int i = 0; i < subs.Count; i++)
                {
                    if (subs.TryGetMaterialized(i) is { } materializedSub)
                    {
                        AddIndexedSymbol(target, materializedSub.Name, materializedSub, command);
                    }
                }
            }
        }

        private static void ThrowIfUnmaterializedSubcommandDuplicatesName(CommandResult innermost, string name)
        {
            for (SymbolResult? cur = innermost; cur is CommandResult cr; cur = cr.Parent)
            {
                Command cmd = cr.Command;
                if (cmd._subcommands is { Count: > 0 } subs && subs.CountUnmaterializedByName(name) > 0)
                {
                    if ((cmd.HasArguments && HasNamedSymbol(cmd.Arguments, name)) ||
                        (cmd.HasOptions && HasNamedSymbol(cmd.Options, name)) ||
                        subs.CountEntriesByName(name) > 1)
                    {
                        throw new InvalidOperationException($"Command {cmd.Name} has more than one child named \"{name}\".");
                    }
                }
            }
        }

        private static bool HasUnmaterializedSubcommandOnParsedChain(CommandResult innermost, string name)
        {
            bool found = false;
            for (SymbolResult? cur = innermost; cur is CommandResult cr; cur = cr.Parent)
            {
                Command cmd = cr.Command;
                if (cmd._subcommands is { Count: > 0 } subs)
                {
                    int matches = subs.CountUnmaterializedByName(name);
                    if (matches > 1)
                    {
                        throw new InvalidOperationException($"Command {cmd.Name} has more than one child named \"{name}\".");
                    }
                    if (matches == 1)
                    {
                        found = true;
                    }
                }
            }

            return found;
        }

        private static bool HasNamedSymbol<T>(IList<T> symbols, string name) where T : Symbol
        {
            for (int i = 0; i < symbols.Count; i++)
            {
                if (string.Equals(symbols[i].Name, name, StringComparison.Ordinal))
                {
                    return true;
                }
            }

            return false;
        }

        private static void PopulateSymbolsByName(Command command, Dictionary<string, IndexedSymbol> target)
        {
            if (command.HasArguments)
            {
                for (int i = 0; i < command.Arguments.Count; i++)
                {
                    Argument arg = command.Arguments[i];
                    AddIndexedSymbol(target, arg.Name, arg, command);
                }
            }

            if (command.HasOptions)
            {
                for (int i = 0; i < command.Options.Count; i++)
                {
                    Option opt = command.Options[i];
                    AddIndexedSymbol(target, opt.Name, opt, command);
                }
            }

            if (command.HasSubcommands)
            {
                for (int i = 0; i < command.Subcommands.Count; i++)
                {
                    Command childCommand = command.Subcommands[i];
                    AddIndexedSymbol(target, childCommand.Name, childCommand, command);
                    PopulateSymbolsByName(childCommand, target);
                }
            }
        }

        private static void AddIndexedSymbol(Dictionary<string, IndexedSymbol> target, string name, Symbol symbol, Command parent)
        {
            if (target.TryGetValue(name, out IndexedSymbol? node))
            {
                IndexedSymbol? current = node;
                do
                {
                    // The same symbol can be added to multiple commands and have multiple parents.
                    // We can't allow for name duplicates within the same command.
                    if (ReferenceEquals(current.Parent, parent))
                    {
                        throw new InvalidOperationException($"Command {parent.Name} has more than one child named \"{name}\".");
                    }
                    current = current.Next;
                } while (current is not null);

                target[name] = new IndexedSymbol(symbol, parent)
                {
                    Next = node
                };
            }
            else
            {
                target[name] = new IndexedSymbol(symbol, parent);
            }
        }
    }
}