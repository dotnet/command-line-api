// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

namespace System.CommandLine.Parsing;

// Performance note: Special cases might result in the previous single dictionary being faster, but it did not 
// give correct results for many CLIs, and also, it built a dictionary for the full CLI tree, rather than just the
// current commands and its ancestors, so in many cases, this will be faster. 
//
// Most importantly, that approach fails for options, like the previous global options, that appear on multiple
// commands, since we are now explicitly putting them on all commands.

public class SymbolLookupByName
{
    private class CommandCache(CliCommand command)
    {
        public CliCommand Command { get; } = command;
        public Dictionary<string, CliSymbol> SymbolsByName { get; } = new();
    }

    private List<CommandCache> cache;

    public SymbolLookupByName(ParseResult parseResult)
        => cache = BuildCache(parseResult);

    private List<CommandCache> BuildCache(ParseResult parseResult)
    {
        if (cache is not null)
        {
            return cache;
        }
        cache = [];
        var commandResult = parseResult.CommandResult;
        while (commandResult is not null)
        {
            var command = commandResult.Command;
            if (TryGetCommandCache(command, out var _))
            {
                throw new InvalidOperationException("Command hierarchy appears to be recursive.");
            }
            var commandCache = new CommandCache(command);
            cache.Add(commandCache);

            AddSymbolsToCache(commandCache, command.Options, command);
            AddSymbolsToCache(commandCache, command.Arguments, command);
            AddSymbolsToCache(commandCache, command.Subcommands, command);
            commandResult = (CommandResult?)commandResult.Parent;
        }

        return cache;

        static void AddSymbolsToCache(CommandCache CommandCache, IEnumerable<CliSymbol> symbols, CliCommand command)
        {
            foreach (var symbol in symbols)
            {
                if (CommandCache.SymbolsByName.ContainsKey(symbol.Name))
                {
                    throw new InvalidOperationException($"Command {command.Name} has more than one child named \"{symbol.Name}\".");
                }
                CommandCache.SymbolsByName.Add(symbol.Name, symbol);
            }
        }
    }

    private bool TryGetCommandCache(CliCommand command, [NotNullWhen(true)] out CommandCache? commandCache)
    {
        var candidates = cache.Where(x => x.Command == command);
        if (candidates.Any())
        {
            commandCache = candidates.Single(); // multiples are a failure in construction
            return true;
        }
        commandCache = null;
        return false;
    }

    private bool TryGetSymbolAndParentInternal(string name,
                                               [NotNullWhen(true)] out CliSymbol? symbol,
                                               [NotNullWhen(true)] out CliCommand? parent,
                                               [NotNullWhen(false)] out string? errorMessage,
                                               CliCommand? startCommand,
                                               bool skipAncestors,
                                               bool valuesOnly)
    {
        startCommand ??= cache.First().Command;  // The construction of the dictionary makes this the parseResult.CommandResult - current command
        var commandCaches = GetCommandCachesToUse(startCommand);
        if (commandCaches is null || !commandCaches.Any())
        {
            errorMessage = $"Requested command {startCommand.Name} is not in the results.";
            symbol = null;
            parent = null;
            return false;
        }

        foreach (var commandCache in commandCaches)
        {
            if (commandCache.SymbolsByName.TryGetValue(name, out symbol))
            {
                if (symbol is not null && (!valuesOnly || (symbol is CliArgument or CliOption)))
                {
                    parent = commandCache.Command;
                    errorMessage = null;
                    return true;
                }
            }

            if (skipAncestors)
            {
                break;
            }
        }

        errorMessage = $"Requested symbol {name} was not found.";
        symbol = null;
        parent = null;
        return false;
    }

    public (CliSymbol symbol, CliCommand parent) GetSymbolAndParent(string name, CliCommand? startCommand = null, bool skipAncestors = false, bool valuesOnly = false)
        => TryGetSymbolAndParentInternal(name, out var symbol, out var parent, out var errorMessage, startCommand, skipAncestors, valuesOnly)
            ? (symbol, parent)
            : throw new InvalidOperationException(errorMessage);

    public bool TryGetSymbol(string name, out CliSymbol symbol, CliCommand? startCommand = null, bool skipAncestors = false, bool valuesOnly = false)
        => TryGetSymbolAndParentInternal(name, out symbol, out var _, out var _, startCommand, skipAncestors, valuesOnly);


    private IEnumerable<CommandCache>? GetCommandCachesToUse(CliCommand currentCommand)
    {
        if (cache[0].Command == currentCommand)
        {
            return cache;
        }
        for (int i = 1; i < cache.Count; i++) // we tested for 0 earlier
        {
            if (cache[i].Command == currentCommand)
            {
                return cache.Skip(i);
            }
        }
        return null;
    }
}
