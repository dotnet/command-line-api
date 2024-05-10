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
// Most importantly, the previous approach fails for options, like the previous global options, that appear on multiple
// commands, since we are now explicitly putting them on all commands.

/// <summary>
/// Provides a mechanism to lookup symbols by their name. This searches the symbols corresponding to the current command and its ancestors.
/// </summary>
public class SymbolLookupByName
{
    private readonly struct CommandCache(CliCommand command)
    {
        public CliCommand Command { get; } = command;
        public Dictionary<string, CliSymbol> SymbolsByName { get; } = new();
    }

    private List<CommandCache> cache;

    /// <summary>
    /// Creates a new symbol lookup tied to a specific parseResult. 
    /// </summary>
    /// <param name="parseResult"></param>
    // TODO: If needed, consider a static list/dictionary of ParseResult to make general use easier.
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

        static void AddSymbolsToCache(CommandCache commandCache, IEnumerable<CliSymbol> symbols, CliCommand command)
        {
            foreach (var symbol in symbols)
            {
                if (commandCache.SymbolsByName.ContainsKey(symbol.Name))
                {
                    throw new InvalidOperationException($"Command {command.Name} has more than one child named \"{symbol.Name}\".");
                }
                commandCache.SymbolsByName.Add(symbol.Name, symbol);
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

    /// <summary>
    /// Gets the symbol with the requested name that appears nearest to the starting command, which defaults to the current or leaf command.
    /// </summary>
    /// <param name="name">The name to search for</param>
    /// <param name="symbol">An out parameter to receive the symbol, if found.</param>
    /// <param name="parent">An out parameter to receive the parent, if found.</param>
    /// <param name="startCommand">The command to start searching up from, which defaults to the current command.</param>
    /// <param name="skipAncestors">If true, only the starting command and no ancestors are searched.</param>
    /// <param name="valuesOnly">If true, commands are ignored and only options and arguments are found.</param>
    /// <returns>A tuple of the found symbol and its parent command. Throws if the name is not found.</returns>
    /// <exception cref="InvalidOperationException">Thrown if the name is not found.</exception>
    // TODO: Add tests
    public bool TryGetSymbolAndParent(string name,
                                      [NotNullWhen(true)] out CliSymbol? symbol,
                                      [NotNullWhen(true)] out CliCommand? parent,
                                      CliCommand? startCommand = null,
                                      bool skipAncestors = false,
                                      bool valuesOnly = false)
    {
        if (TryGetSymbolAndParentInternal(name, out var storedSymbol, out var storedParent, out var errorMessage, startCommand, skipAncestors, valuesOnly))
        {
            symbol = storedSymbol;
            parent = storedParent;
            return true;
        }
        if (errorMessage is not null)
        {
            throw new InvalidOperationException(errorMessage);
        }
        symbol = null;
        parent = null;
        return false;
    }


    /// <summary>
    /// Returns true if the symbol is found, and provides the symbols as the `out` symbol parameter.
    /// </summary>
    /// <param name="name">The name to search for</param>
    /// <param name="symbol">An out parameter to receive the symbol if it is found.</param>
    /// <param name="startCommand">The command to start searching up from, which defaults to the current command.</param>
    /// <param name="skipAncestors">If true, only the starting command and no ancestors are searched.</param>
    /// <param name="valuesOnly">If true, commands are ignored and only options and arguments are found.</param>
    /// <returns>True if a symbol with the requested name is found</returns>
    public bool TryGetSymbol(string name, [NotNullWhen(true)] out CliSymbol? symbol, CliCommand? startCommand = null, bool skipAncestors = false, bool valuesOnly = false)
        => TryGetSymbolAndParentInternal(name, out symbol, out var _, out var _, startCommand, skipAncestors, valuesOnly);


    private IEnumerable<CommandCache>? GetCommandCachesToUse(CliCommand currentCommand)
    {
        int index = FindIndex(cache, currentCommand);
        return index == -1 
            ? null 
            : cache.Skip(index);

        static int FindIndex(List<CommandCache> cache, CliCommand? currentCommand)
            => cache.FindIndex(c => c.Command == currentCommand);
    }
}
