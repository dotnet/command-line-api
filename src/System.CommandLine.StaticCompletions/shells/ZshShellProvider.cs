// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.CodeDom.Compiler;
using System.CommandLine.StaticCompletions.Resources;

namespace System.CommandLine.StaticCompletions.Shells;

public class ZshShellProvider : IShellProvider
{
    public string ArgumentName => ShellNames.Zsh;

    public string Extension => "zsh";

    public string HelpDescription => Strings.ZshShellProvider_HelpDescription;

    // override the ToString method to return the argument name so that CLI help is cleaner for 'default' values
    public override string ToString() => ArgumentName;

    private const string StateName = "suggest";

    public string GenerateCompletions(Command command)
    {
        var binaryName = command.Name;
        using var textWriter = new StringWriter { NewLine = "\n" };
        using var writer = new IndentedTextWriter(textWriter);
        string[] pathToCurrentCommand = [command.Name];


        writer.WriteLine($"#compdef {binaryName}");
        writer.WriteLine();
        writer.WriteLine("autoload -U is-at-least");
        writer.WriteLine();
        // TODO: if the CLI grammar doesn't support option bundling, remove -s from these options. -s is the bundling option for _arguments
        writer.WriteLine($$"""
_{{binaryName}}() {
    typeset -A opt_args
    typeset -a _arguments_options
    local ret=1

    if is-at-least 5.2; then
        _arguments_options=(-s -S -C)
    else
        _arguments_options=(-s -C)
    fi

    local context curcontext="$curcontext" state state_descr line
""");

        writer.Indent++;
        writer.WriteLine(ArgumentsHandler());
        writer.Indent++;
        GenerateOptionsAndArgumentsForCommand(binaryName, pathToCurrentCommand, command, writer);
        writer.Indent--;

        // tiny hack here - for dynamic completions we need to know what the entire command line is,
        // so we stash it in a variable and then use it in the dynamic completion handlers
        writer.WriteLine($$"""local original_args="{{binaryName}} ${line[@]}" """);

        writer.Indent--;
        writer.Indent++;
        GenerateSubcommandList(binaryName, pathToCurrentCommand, command, writer);
        writer.Indent--;
        writer.WriteLine("}");
        writer.WriteLine();

        GenerateSubcommandHandlers(pathToCurrentCommand, command, writer);
        writer.Indent--;
        writer.WriteLine($$"""
if [ "$funcstack[1]" = "_{{binaryName}}" ]; then
    _{{binaryName}} "$@"
else
    compdef _{{binaryName}} {{binaryName}}
fi
""");
        writer.Flush();
        return textWriter.ToString();
    }

    private static void GenerateOptionsAndArgumentsForCommand(string binaryName, string[] commandPathForThisCommand, Command command, IndentedTextWriter writer)
    {
        var shouldWriteDynamicCompleter = false;
        foreach (var option in command.HierarchicalOptions())
        {
            var multiplicity = option.Arity.MaximumNumberOfValues > 1 ? "*" : "";
            var helpText = SanitizeHelp(option.Description);
            if (option.IsFlag())
            {
                foreach (var name in option.Names())
                {
                    writer.WriteLine($"'{multiplicity}{name}[{helpText}]' \\");
                }
            }
            else
            {
                if (option.IsDynamic)
                {
                    shouldWriteDynamicCompleter = true;
                }
                var argumentName = option.HelpName ?? " ";
                var argumentValues = ZshValueExpression(option);
                foreach (var name in option.Names())
                {
                    writer.Write($"'{multiplicity}{name}=[{helpText}]:{argumentName}");
                    WriteValueExpression(writer, argumentValues);
                }
            }
        }

        var catch_all_emitted = false;
        foreach (var arg in command.Arguments.Where(c => !c.Hidden))
        {
            var isMultiValued = arg.Arity.MaximumNumberOfValues > 1;
            if (catch_all_emitted && isMultiValued)
            {
                continue;
            }

            string cardinality = "";
            if (isMultiValued && command.Subcommands.Count == 0)
            {
                catch_all_emitted = true;
                cardinality = "*:";
            }
            else if (arg.Arity.MinimumNumberOfValues == 0)
            {
                cardinality = ":";
            }

            if (arg.IsDynamic)
            {
                shouldWriteDynamicCompleter = true;
            }
            var helpText = SanitizeHelp(arg.Description is string d ? " -- " + d : "");
            var completions = ZshValueExpression(arg);
            writer.Write($"'{cardinality}:{arg.Name}{helpText}");
            WriteValueExpression(writer, completions);
        }

        if (command.Subcommands.Any(c => !c.Hidden))
        {
            var parentSubcommandHandlerName = string.Join("__", commandPathForThisCommand);
            writer.WriteLine($"\":: :_{parentSubcommandHandlerName}_commands\" \\");
            writer.WriteLine($"\"*::: :->{command.Name}\" \\");
        }

        writer.WriteLine("&& ret=0");

        if (shouldWriteDynamicCompleter)
        {
            writer.WriteLine("case $state in");
            writer.Indent++;
            GenerateDynamicCompleter(binaryName, writer);
            writer.Indent--;
            writer.WriteLine("esac");
        }

        static void WriteValueExpression(IndentedTextWriter writer, string[]? argumentValues)
        {
            if (argumentValues is null || argumentValues.Length == 0)
            {
                writer.Write($": ");
            }
            else if (argumentValues.Length == 1)
            {
                writer.Write($":{argumentValues[0]}");
            }
            else
            {
                writer.Write(':');
                writer.Write(argumentValues[0]);
                foreach (var line in argumentValues[1..^1])
                {
                    writer.Write(line);
                    writer.Write(" ");
                }
                writer.Write(argumentValues[^1]);
            }
            writer.WriteLine("' \\");
        }
    }

    private static void GenerateSubcommandList(string binaryName, string[] pathToCurrentCommand, Command command, IndentedTextWriter writer)
    {
        if (command.Subcommands.Count == 0)
        {
            return;
        }

        writer.WriteLine("case $state in");
        writer.Indent++;
        writer.WriteLine($"({command.Name})");
        writer.Indent++;
        // skip hidden arguments and any arguments inherited from parent commands when computing the
        // position of this subcommand's word on the line - those don't show up as positional slots
        // that the user actually types, but they would throw off the counting if included.
        var parentArguments = command.Parents.OfType<Command>().SelectMany(parent => parent.Arguments).Select(arg => arg.Name).ToHashSet();
        var pos = command.Arguments.Where(a => !parentArguments.Contains(a.Name) && !a.Hidden).Count() + 1;
        writer.WriteLine($$"""words=($line[{{pos}}] "${words[@]}")""");
        writer.WriteLine("(( CURRENT += 1 ))");
        writer.WriteLine($"curcontext=\"${{curcontext%:*:*}}:{string.Join('-', pathToCurrentCommand)}-command-$line[{pos}]:\"");
        writer.WriteLine($"case $line[{pos}] in");
        writer.Indent++;

        foreach (var subcommand in command.Subcommands.Where(c => !c.Hidden))
        {
            var pathToSubcommand = AppendCommandToPath(pathToCurrentCommand, subcommand);
            // generate help stubs for this subcommand
            writer.WriteLine($"({subcommand.Name})");
            writer.Indent++;
            writer.WriteLine(ArgumentsHandler());
            writer.Indent++;
            GenerateOptionsAndArgumentsForCommand(binaryName, pathToSubcommand, subcommand, writer);
            GenerateSubcommandList(binaryName, pathToSubcommand, subcommand, writer);
            writer.Indent--;
            writer.WriteLine(";;");
            writer.Indent--;
        }
        writer.Indent--;
        writer.WriteLine("esac");
        writer.Indent--;
        writer.WriteLine(";;");
        writer.Indent--;
        writer.WriteLine("esac");
    }

    private static void GenerateDynamicCompleter(string binaryName, IndentedTextWriter writer)
    {
        writer.WriteLine($"({StateName})");
        writer.Indent++;
        writer.WriteLine("local completions=()");
        writer.WriteLine($"local result=$({binaryName} \"[{StateName}:${{#original_args}}]\" \"${{original_args}}\" 2>/dev/null)");
        writer.WriteLine("for line in ${(f)result}; do");
        writer.Indent++;
        writer.WriteLine("completions+=(${(q)line})");
        writer.Indent--;
        writer.WriteLine("done");
        writer.WriteLine("_describe 'completions' $completions && ret=0");
        writer.Indent--;
        writer.WriteLine(";;");
    }

    private static void GenerateSubcommandHandlers(string[] pathToThisCommand, Command command, IndentedTextWriter writer)
    {

        var unique_command_name = string.Join("__", pathToThisCommand);

        writer.WriteLine($"(( $+functions[_{unique_command_name}_commands] )) ||");
        writer.WriteLine($"_{unique_command_name}_commands() {{");
        writer.Indent++;
        writer.Write("local commands; ");
        if (command.Subcommands.Where(s => !s.Hidden).Count() > 0)
        {
            writer.WriteLine("commands=(");
            writer.Indent++;
            foreach (var subcommand in command.Subcommands.Where(s => !s.Hidden))
            {
                writer.WriteLine($"'{subcommand.Name}:{SanitizeHelp(subcommand.Description)}' \\");
            }
            writer.Indent--;
            writer.WriteLine(")");
        }
        else
        {
            writer.WriteLine("commands=()");
        }
        writer.WriteLine($"_describe -t commands '{string.Join(' ', pathToThisCommand)} commands' commands \"$@\"");
        writer.Indent--;
        writer.WriteLine("}");
        writer.WriteLine();


        foreach (var subcommand in command.Subcommands.Where(c => !c.Hidden))
        {
            var pathToSubcommand = AppendCommandToPath(pathToThisCommand, subcommand);
            GenerateSubcommandHandlers(pathToSubcommand, subcommand, writer);
        }
    }

    private static string SanitizeHelp(string? s) =>
        s?
            .Replace("\"", "\\\"")
            .Replace("\'", "'\\''")
            .Replace("[", "\\[")
            .Replace("]", "\\]")
            .Replace(":", "\\:")
            .Replace("$", "\\$")
            .Replace("`", "\\`")
            .Replace("\r\n", " ")
            .Replace('\n', ' ')
            ?? "";

    private static string SanitizeValue(string? s) =>
        s?
            .Replace("\\", "\\\\")
            .Replace("\'", "'\\''")
            .Replace("[", "\\[")
            .Replace("]", "\\]")
            .Replace(":", "\\:")
            .Replace("$", "\\$")
            .Replace("`", "\\`")
            .Replace("(", "\\(")
            .Replace(")", "\\)")
            .Replace(" ", "\\ ")
        ?? "";

    private static string[]? ZshValueExpression(Option option)
    {
        if (option.IsDynamic)
        {
            return [$"->{StateName}"];
        }
        else
        {
            return ZshValueExpression(option as Symbol);
        }
    }

    private static string[]? ZshValueExpression(Argument arg)
    {
        if (arg.IsDynamic)
        {
            return [$"->{StateName}"];
        }
        else
        {
            return ZshValueExpression(arg as Symbol);
        }
    }

    private static string[]? ZshValueExpression(Symbol sym)
    {
        var staticCompletions = sym.GetCompletions(Completions.CompletionContext.Empty).ToArray();
        if (staticCompletions.Length == 0)
        {
            //TODO: attempt to do zsh helpers here
            if (sym is Option<FileInfo> || sym is Argument<FileInfo>)
            {
                return ["_files"];
            }
            else if (sym is Option<Uri> || sym is Argument<Uri>)
            {
                return ["_urls"];
            }

            return null;
        }
        else
        {
            if (staticCompletions.Any(c => c.InsertText is not null || c.Detail is not null || c.Documentation is not null))
            {
                // since any item had 'help', we use the help form of value completions.
                // note the double parens - this ensures that the descriptions are parsed and not treated as part of the value
                var lines = new List<string>(staticCompletions.Length + 2) { "((" };
                foreach (var completion in staticCompletions)
                {
                    var insertText = completion.InsertText ?? completion.Label;
                    var documentation = completion.Documentation ?? completion.Detail ?? completion.Label;
                    // syntax here is value\:"helptext"
                    lines.Add($"{SanitizeValue(insertText)}\\:\"{SanitizeHelp(documentation)}\"");
                }
                lines.Add("))");
                return lines.ToArray();
            }
            else
            {
                // since none have help, we use the raw form
                return [$"({string.Join(" ", staticCompletions.Select(c => SanitizeValue(c.InsertText ?? c.Label)))})"];
            }
        }
    }

    private static string ArgumentsHandler() => "_arguments \"${_arguments_options[@]}\" : \\";

    private static string[] AppendCommandToPath(string[] path, Command command) =>
        path.Length == 0 ? [command.Name] : [.. path, command.Name];
}
