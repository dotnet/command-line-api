// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Generic;

namespace System.CommandLine.Parsing
{
    internal sealed class SymbolResultTree : Dictionary<Symbol, SymbolResult>
    {
        private readonly Command? _rootCommand;
        internal CommandResult? InnermostCommandResult;
        internal List<ParseError>? Errors;
        internal List<Token>? UnmatchedTokens;
        private Dictionary<string, SymbolNode>? _parsedSymbolsByName;
        private CommandResult? _parsedSymbolsCommandResult;
        private Dictionary<string, SymbolNode>? _symbolsByName;

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
            if (_symbolsByName is null && InnermostCommandResult is not null)
            {
                if (_parsedSymbolsByName is null || !ReferenceEquals(_parsedSymbolsCommandResult, InnermostCommandResult))
                {
                    _parsedSymbolsByName = new(StringComparer.Ordinal);
                    _parsedSymbolsCommandResult = InnermostCommandResult;
                    PopulateParsedCommandChain(InnermostCommandResult, _parsedSymbolsByName);
                }

                if (_parsedSymbolsByName.TryGetValue(name, out SymbolNode? parsedNode))
                {
                    return FindMatchingResult(parsedNode);
                }
            }

            if (_symbolsByName is null && _rootCommand is not null)
            {
                _symbolsByName = new(StringComparer.Ordinal);
                PopulateSymbolsByName(_rootCommand, _symbolsByName);
            }

            if (_symbolsByName is null || !_symbolsByName.TryGetValue(name, out SymbolNode? node))
            {
                throw new ArgumentException($"No symbol result found with name \"{name}\".");
            }

            return FindMatchingResult(node);
        }

        private SymbolResult? FindMatchingResult(SymbolNode? node)
        {
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

        private static void PopulateParsedCommandChain(CommandResult innermost, Dictionary<string, SymbolNode> target)
        {
            List<Command> chain = new(4);
            for (SymbolResult? current = innermost; current is CommandResult cmdResult; current = cmdResult.Parent)
            {
                chain.Add(cmdResult.Command);
            }

            for (int c = chain.Count - 1; c >= 0; c--)
            {
                Command command = chain[c];
                HashSet<string>? childNames = null;

                if (command.HasArguments)
                {
                    for (var i = 0; i < command.Arguments.Count; i++)
                    {
                        Argument arg = command.Arguments[i];
                        TrackChildName(ref childNames, arg.Name, command);
                        AddSymbolNode(target, arg, command);
                    }
                }

                if (command.HasOptions)
                {
                    for (var i = 0; i < command.Options.Count; i++)
                    {
                        Option opt = command.Options[i];
                        TrackChildName(ref childNames, opt.Name, command);
                        AddSymbolNode(target, opt, command);
                    }
                }

                if (command._subcommands is { Count: > 0 } subs)
                {
                    for (var i = 0; i < subs.Count; i++)
                    {
                        string subName = subs.GetEntryName(i);
                        TrackChildName(ref childNames, subName, command);
                        if (subs.TryGetMaterialized(i) is { } materializedSub)
                        {
                            AddSymbolNode(target, materializedSub, command);
                        }
                    }
                }
            }

            static void TrackChildName(ref HashSet<string>? names, string childName, Command parent)
            {
                names ??= new HashSet<string>(StringComparer.Ordinal);
                if (!names.Add(childName))
                {
                    throw new InvalidOperationException($"Command {parent.Name} has more than one child named \"{childName}\".");
                }
            }
        }

        private static void PopulateSymbolsByName(Command command, Dictionary<string, SymbolNode> target)
        {
            if (command.HasArguments)
            {
                for (var i = 0; i < command.Arguments.Count; i++)
                {
                    AddSymbolNode(target, command.Arguments[i], command);
                }
            }

            if (command.HasOptions)
            {
                for (var i = 0; i < command.Options.Count; i++)
                {
                    AddSymbolNode(target, command.Options[i], command);
                }
            }

            if (command.HasSubcommands)
            {
                for (var i = 0; i < command.Subcommands.Count; i++)
                {
                    var childCommand = command.Subcommands[i];
                    AddSymbolNode(target, childCommand, command);
                    PopulateSymbolsByName(childCommand, target);
                }
            }
        }

        private static void AddSymbolNode(Dictionary<string, SymbolNode> target, Symbol symbol, Command parent)
        {
            if (target.TryGetValue(symbol.Name, out var node))
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

                target[symbol.Name] = new(symbol, parent)
                {
                    Next = node
                };
            }
            else
            {
                target[symbol.Name] = new(symbol, parent);
            }
        }
    }
}