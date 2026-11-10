// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace System.CommandLine.StaticCompletions;

using System.CommandLine;

public static class HelpExtensions
{
    /// <summary>
    /// Create a unique shell function name for a command - these names should be
    /// * distinct from the 'root' command's name (i.e. we should not generate the function name 'dotnet' for the binary 'dotnet')
    /// * distinct based on 'path' to get to this function (hence the parentCommandNames)
    /// </summary>
    /// <param name="command"></param>
    /// <param name="parentCommandNames">The chain of commands to get to this command</param>
    /// <returns></returns>
    public static string FunctionName(this Command command, string[]? parentCommandNames = null) => parentCommandNames switch
    {
        null => "_" + command.Name,
        [] => "_" + command.Name,
        var names => "_" + string.Join('_', names) + "_" + command.Name
    };

    /// <summary>
    /// Sanitizes a function name to be safe for bash
    /// </summary>
    /// <param name="functionName"></param>
    /// <returns></returns>
    public static string MakeSafeFunctionName(this string functionName) => functionName.Replace('-', '_');

    /// <summary>
    /// Get all names for an option, including the primary name and all aliases
    /// </summary>
    /// <param name="option"></param>
    /// <returns></returns>
    public static string[] Names(this Option option)
    {
        var (primary, aliases) = PrimaryNameAndAliases(option);
        return aliases is null ? [primary] : [primary, .. aliases];
    }

    public static (string primary, string[]? aliases) PrimaryNameAndAliases(this Option option)
    {
        if (option.Aliases.Count == 0)
        {
            return (option.Name, null);
        }
        else if (option is System.CommandLine.Help.HelpOption) // some of the help aliases are truly horrible
        {
            return ("--help", ["-h"]);
        }
        else
        {
            return (option.Name, [.. option.Aliases]);
        }
    }

    /// <summary>
    /// Get all names for a command, including the primary name and all aliases
    /// </summary>
    /// <param name="command"></param>
    /// <returns></returns>
    public static string[] Names(this Command command)
    {
        if (command.Aliases.Count == 0)
        {
            return [command.Name];
        }
        else
        {
            return [command.Name, .. command.Aliases];
        }
    }

    public static IEnumerable<Option> HierarchicalOptions(this Command c)
    {
        // don't include hidden options, because hidden shouldn't be shown in completions at all.
        var myOptions = c.Options.Where(o => !o.Hidden);
        if (c.Parents.Count() == 0)
        {
            return myOptions;
        }
        else
        {
            // the parents could return the same logical option, so we need to dedupe them in order to not crowd the completion lists.
            return myOptions.Concat(c.Parents.OfType<Command>().SelectMany(OptionsForParent)).DistinctBy(o => o.Name);
        }
    }

    private static IEnumerable<Option> OptionsForParent(Command c)
    {
        foreach (var o in c.Options)
        {
            if (o.Recursive && !o.Hidden)
            {
                yield return o;
            }
        }
        foreach (var p in c.Parents.OfType<Command>())
        {
            foreach (var o in OptionsForParent(p))
            {
                yield return o;
            }
        }
    }

    public static bool IsUpperCaseSingleCharacterFlag(this string name) => name.Length == 2 && char.IsUpper(name[1]);

    public static bool IsLongAlias(this string name) => name.Length > 2 && name[0] == '-' && name[1] == '-';
    public static bool IsShortAlias(this string name) => name.Length == 2 && name[0] == '-' && char.IsAsciiLetter(name[1]);

    public static bool IsFlag(this Option option) => option.Arity.Equals(ArgumentArity.Zero);
}
