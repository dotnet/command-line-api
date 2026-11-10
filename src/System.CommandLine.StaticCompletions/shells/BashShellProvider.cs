// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.CodeDom.Compiler;
using System.CommandLine.Completions;
using System.CommandLine.StaticCompletions.Resources;

namespace System.CommandLine.StaticCompletions.Shells;

public class BashShellProvider : IShellProvider
{
    public string ArgumentName => ShellNames.Bash;

    public string Extension => "sh";

    public string HelpDescription => Strings.BashShellProvider_HelpDescription;

    // override the ToString method to return the argument name so that CLI help is cleaner for 'default' values
    public override string ToString() => ArgumentName;

    public string GenerateCompletions(Command command)
    {
        var initialFunctionName = command.FunctionName().MakeSafeFunctionName();
        return
            $"""
            #!/usr/bin/env bash
            {GenerateCommandsCompletions([], command, isNestedCommand: false)}

            complete -F {initialFunctionName} {command.Name}
            """;
    }

    private string GenerateCommandsCompletions(string[] parentCommandNames, Command command, bool isNestedCommand)
    {
        var functionName = command.FunctionName(parentCommandNames).MakeSafeFunctionName();

        var dollarOne = !isNestedCommand ? "1" : "$1";
        var subcommandArgument = !isNestedCommand ? "2" : "$(($1+1))";

        // generate the words for options and subcommands
        var visibleSubcommands = command.Subcommands.Where(c => !c.Hidden).ToArray();
        // notably, do not generate completions for all option aliases - since a user is tab-completing we can use the longest forms
        var completionOptions = command.HierarchicalOptions().Where(o => !o.Hidden).Select(o => o.Name).ToArray();
        var completionSubcommands = visibleSubcommands.Select(x => x.Name).ToArray();
        string[] completionWords = [.. completionSubcommands, .. completionOptions];

        // for positional arguments this can be pretty dynamic
        var positionalArgumentCompletions = PositionalArgumentTerms(command.Arguments.Where(a => !a.Hidden).ToArray());

        using var textWriter = new StringWriter { NewLine = "\n" };
        using var writer = new IndentedTextWriter(textWriter);

        // write the overall completion function shell
        writer.WriteLine($"{functionName}() {{");
        writer.WriteLine();
        writer.Indent++;

        // set up state
        writer.WriteLine("""cur="${COMP_WORDS[COMP_CWORD]}" """);
        writer.WriteLine("""prev="${COMP_WORDS[COMP_CWORD-1]}" """);
        writer.WriteLine("COMPREPLY=()");
        writer.WriteLine();

        // fill in a set of completions for all of the subcommands and flag options for the top-level command
        writer.WriteLine($"""opts="{string.Join(' ', completionWords)}" """);
        foreach (var positionalArgumentCompletion in positionalArgumentCompletions)
        {
            writer.WriteLine($"""opts="$opts {positionalArgumentCompletion}" """);
        }
        writer.WriteLine();

        // emit a short-circuit for when the first argument index (COMP_CWORD) is 1 (the top-level command word)
        // in this short-circuit we'll just use the choices we set up above in $opts
        writer.WriteLine($"""if [[ $COMP_CWORD == "{dollarOne}" ]]; then""");
        writer.Indent++;
        writer.WriteLine(GenerateChoicesPrompt("$opts"));
        writer.WriteLine("return");
        writer.Indent--;
        writer.WriteLine("fi");
        writer.WriteLine();

        // generate how to handle completions for options or flags
        var optionHandlers = GenerateOptionHandlers(command);
        if (optionHandlers is not null)
        {
            writer.WriteLine("case $prev in");
            writer.Indent++;
            foreach (var line in optionHandlers.Split('\n'))
            {
                writer.WriteLine(line);
            }
            writer.Indent--;
            writer.WriteLine("esac");
            writer.WriteLine();
        }

        // finally subcommand completions - these are going to emit calls to subcommand completion functions that we'll emit at the end of this method
        if (visibleSubcommands.Length > 0)
        {
            writer.WriteLine($"case ${{COMP_WORDS[{dollarOne}]}} in");
            writer.Indent++;
            foreach (var subcommand in visibleSubcommands)
            {
                writer.WriteLine($"({subcommand.Name})");
                writer.Indent++;
                writer.WriteLine($"{functionName}_{subcommand.Name} {subcommandArgument}");
                writer.WriteLine("return");
                writer.WriteLine(";;");
                writer.WriteLine();
                writer.Indent--;
            }
            writer.Indent--;
            writer.WriteLine("esac");
            writer.WriteLine();
        }

        // write the final trailer for the overall completion script
        writer.WriteLine(GenerateChoicesPrompt("$opts"));
        writer.Indent--;
        writer.WriteLine("}");
        writer.WriteLine();

        // annnnd flush!
        writer.Flush();
        string[] parentCommandNamesForSubcommands = [.. parentCommandNames, command.Name];
        return textWriter.ToString() + string.Join('\n', visibleSubcommands.Select(c => GenerateCommandsCompletions(parentCommandNamesForSubcommands, c, isNestedCommand: true)));
    }

    internal static string[] PositionalArgumentTerms(Argument[] arguments)
    {
        var completions = new List<string>();
        foreach (var argument in arguments)
        {
            if (argument.IsDynamic)
            {
                // if the argument is a not-static-friendly argument, we need to call into the app for completions
                completions.Add($"$({GenerateDynamicCall()})");
                continue;
            }
            var argCompletions = argument.GetCompletions(CompletionContext.Empty).Select(c => c.Label).ToArray();
            if (argCompletions.Length != 0)
            {
                // otherwise emit a direct list of choices
                completions.Add($"""({string.Join(' ', argCompletions)})""");
            }
        }

        return completions.ToArray();
    }

    /// <summary>
    /// Generates the bash expression that invokes the application's <c>[suggest]</c> directive
    /// to resolve dynamic completions.
    /// </summary>
    internal static string GenerateDynamicCall() =>
        """${COMP_WORDS[0]} "[suggest:${COMP_POINT}]" "${COMP_LINE}" 2>/dev/null | tr '\n' ' '""";

    internal static string? GenerateOptionHandlers(Command command)
    {
        var optionHandlers = command.Options.Where(o => !o.Hidden).Select(GenerateOptionHandler).Where(handler => handler is not null).ToArray();
        if (optionHandlers.Length == 0)
        {
            return null;
        }
        return string.Join("\n", optionHandlers);
    }

    /// <summary>
    /// Emit a bash command that calls compgen with a set of choices given the current work/stem, and sets those choices to COMPREPLY.
    /// Think of this like a 'return' from a function.
    /// </summary>
    /// <param name="choicesInvocation">The expression used to generate the set of choices - will be passed to compgen with the -W flag, so should be either
    /// * a concrete set of choices in a bash array already ($opts), or
    /// * a subprocess that will return such an array. </param>
    /// <returns></returns>
    internal static string GenerateChoicesPrompt(string choicesInvocation) => $$"""COMPREPLY=( $(compgen -W "{{choicesInvocation}}" -- "$cur") )""";

    /// <summary>
    /// Generates a concrete set of bash completion selection for a given option.
    /// If the option's completions are dynamic, this will emit a call to the application's configured
    /// dynamic-completion invocation to get completions when the user requests completions for this option.
    /// </summary>
    /// <param name="option"></param>
    /// <returns>a bash switch case expression for providing completions for this option</returns>
    internal static string? GenerateOptionHandler(Option option)
    {
        // unlike the completion-options generation, for actually implementing suggestions we should be able to handle all of the options' aliases.
        // this ensures if the user manually enters an alias we can support that usage.
        var optionNames = string.Join('|', option.Names());
        string completionCommand;
        if (option.IsDynamic)
        {
            // dynamic options require a call into the app for completions
            completionCommand = GenerateChoicesPrompt($"({GenerateDynamicCall()})");
        }
        else if (option.Arity.MaximumNumberOfValues == 0)
        {
            // do not generate completions for flags-style options - the only completion is the option's _name_
            return null;
        }
        else
        {
            var completions = option.GetCompletions(CompletionContext.Empty).Select(c => c.Label);
            if (completions.Count() == 0)
            {
                // if no static completions are available, then don't emit anything
                return null;
            }
            else
            {
                // otherwise emit a direct list of choices
                completionCommand = GenerateChoicesPrompt($"{string.Join(' ', completions)}");
            }
        }

        return $"""
                {optionNames})
                    {completionCommand}
                    return
                ;;
                """;
    }
}
