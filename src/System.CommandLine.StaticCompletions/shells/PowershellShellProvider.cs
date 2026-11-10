// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace System.CommandLine.StaticCompletions.Shells;

using System.CodeDom.Compiler;
using System.CommandLine;
using System.CommandLine.Completions;
using System.CommandLine.StaticCompletions.Resources;

public class PowerShellShellProvider : IShellProvider
{
    public string ArgumentName => ShellNames.PowerShell;

    public string Extension => "ps1";

    public string HelpDescription => Strings.PowershellShellProvider_HelpDescription;

    // override the ToString method to return the argument name so that CLI help is cleaner for 'default' values
    public override string ToString() => ArgumentName;

    public string GenerateCompletions(Command command)
    {
        var binaryName = command.Name;

        using var textWriter = new StringWriter { NewLine = "\n" };
        using var writer = new IndentedTextWriter(textWriter);
        writer.WriteLine(
$$$"""
using namespace System.Management.Automation
using namespace System.Management.Automation.Language

Register-ArgumentCompleter -Native -CommandName '{{{binaryName}}}' -ScriptBlock {
    param($wordToComplete, $commandAst, $cursorPosition)

    $commandElements = $commandAst.CommandElements
    $command = @(
        '{{{binaryName}}}'
        for ($i = 1; $i -lt $commandElements.Count; $i++) {
            $element = $commandElements[$i]
            if ($element -isnot [StringConstantExpressionAst] -or
                $element.StringConstantType -ne [StringConstantType]::BareWord -or
                $element.Value.StartsWith('-') -or
                $element.Value -eq $wordToComplete) {
                break
            }
            $element.Value
        }) -join ';'
""");
        writer.WriteLine();
        writer.Indent++;

        writer.WriteLine("$completions = @()");
        writer.WriteLine("switch ($command) {");
        writer.Indent++;
        GenerateSubcommandCompletions([], writer, command);

        writer.Indent--;
        writer.WriteLine("}");
        writer.WriteLine("$completions | Where-Object -FilterScript { $_.CompletionText -like \"$wordToComplete*\" } | Sort-Object -Property ListItemText");

        writer.Indent--;
        writer.WriteLine("}");
        writer.Flush();
        return textWriter.ToString();
    }

    private static string CompletionResult(string name, string value, string type, string? helpText)
    {
        if (helpText is null)
        {
            return $"[CompletionResult]::new('{value}', '{name}', [CompletionResultType]::{type}, \"{value}\")";
        }
        else
        {
            return $"[CompletionResult]::new('{value}', '{name}', [CompletionResultType]::{type}, \"{helpText}\")";
        }
    }

    private static string ParameterNameResult(string name, string value, string? helpText) => CompletionResult(name, value, "ParameterName", helpText);

    private static string ParameterValueResult(string name, string value, string? helpText) => CompletionResult(name, value, "ParameterValue", helpText);

    private static string? SanitizeHelpDescription(Symbol s) => s.Description?.ReplaceLineEndings(" ").Replace("`", "``").Replace("'", "`'").Replace("\"", "`\"").Replace("$", "`$");

    /// <summary>
    /// Generations completion-list items for the names of the given option. Typically used by commands/subcommands for static lookup lists.
    /// </summary>
    /// <param name="o"></param>
    /// <returns></returns>
    private static IEnumerable<string> GenerateOptionNameCompletions(Option o)
    {
        if (o.Hidden)
        {
            yield break;
        }

        var verboseName = o.Name;
        var names = o.Names();
        var helpText = SanitizeHelpDescription(o);

        // generate a completion recognizer for each alias, but have it's 'commit' value
        // be the longest/most verbose for clarity
        foreach (var name in names)
        {
            // differentiate casing for single character flags because powershell doesn't do case-sensitivity
            var completionValue = name.IsUpperCaseSingleCharacterFlag() ? $"{verboseName} " : verboseName;
            yield return ParameterNameResult(name, completionValue, helpText);
        }
    }

    /// <summary>
    /// Generate completions for the statically-known arguments
    /// </summary>
    /// <param name="argument"></param>
    /// <returns></returns>
    private static IEnumerable<string> GenerateArgumentCompletions(Argument argument)
    {
        if (argument.Hidden)
        {
            yield break;
        }

        if (argument.IsDynamic)
        {
            // if the argument is a not-static-friendly argument, we need to call into the app for completions
            // TODO: not yet supported for powershell
            yield break;
        }
        var argCompletions = argument.GetCompletions(CompletionContext.Empty).ToArray();
        if (argCompletions is not null && argCompletions.Length > 0)
        {
            foreach (var completion in argCompletions)
            {
                yield return ParameterValueResult(completion.Label, completion.InsertText ?? completion.Label, completion.Documentation ?? completion.Detail ?? completion.Label);
            }
        }
    }

    /// <summary>
    /// Generate completions for the subcommands of a given command. Each subcommand generates a list of switches for the staticly-known flags and arguments.
    /// </summary>
    /// <param name="parentCommandNames"></param>
    /// <param name="writer"></param>
    /// <param name="command"></param>
    /// <remarks>Dynamically-generated completions are not yet supported</remarks>
    internal static void GenerateSubcommandCompletions(string[] parentCommandNames, IndentedTextWriter writer, Command command)
    {
        string[] commandNameList = parentCommandNames switch
        {
            null => [command.Name],
            [] => [command.Name],
            var names => [.. names, command.Name]
        };

        GenerateStaticCompletionsForCommand(commandNameList, command, writer);
        GenerateDynamicCompletionsForOptions(commandNameList, command.Options, writer);
        GenerateDynamicCompletionsForArguments(commandNameList, command.Arguments, writer);

        foreach (var subcommand in command.Subcommands)
        {
            if (subcommand.Hidden)
            {
                continue;
            }
            GenerateSubcommandCompletions(commandNameList, writer, subcommand);
        }

    }

    /// <summary>
    /// Generate completions for the statically-known options, arguments, and subcommands of a given command.
    /// </summary>
    private static void GenerateStaticCompletionsForCommand(string[] commandPath, Command command, IndentedTextWriter writer)
    {
        List<string> completions = new();

        foreach (var option in command.HierarchicalOptions())
        {
            completions.AddRange(GenerateOptionNameCompletions(option));
        }

        foreach (var argument in command.Arguments)
        {
            completions.AddRange(GenerateArgumentCompletions(argument));
        }

        foreach (var subcommand in command.Subcommands)
        {
            if (subcommand.Hidden)
            {
                continue;
            }
            var longName = subcommand.Name;
            var description = SanitizeHelpDescription(subcommand);
            foreach (var subcommandName in subcommand.Names())
            {
                completions.Add(ParameterValueResult(subcommandName, longName, description));
            }
        }

        writer.WriteLine($"'{string.Join(";", commandPath)}' {{");
        writer.Indent++;
        writer.WriteLine("$staticCompletions = @(");
        writer.Indent++;
        foreach (var completion in completions)
        {
            writer.WriteLine(completion);
        }
        writer.Indent--;
        writer.WriteLine(")");

        writer.WriteLine("$completions += $staticCompletions");

        if (command.Arguments.Any(argument => argument.IsDynamic))
        {
            GenerateDynamicCompletionsCall(commandPath[0], writer);
        }

        writer.WriteLine("break");
        writer.Indent--;
        writer.WriteLine("}");
    }

    /// <summary>
    /// Generate a call that invokes the application's <c>[suggest]</c> directive to produce dynamic argument completions,
    /// then binds the returned values as CompletionResults.
    /// </summary>
    private static void GenerateDynamicCompletionsCall(string binaryName, IndentedTextWriter writer)
    {
        writer.WriteLine("$text = $commandAst.ToString()");
        writer.WriteLine($"$suggestResults = @(& '{binaryName}' \"[suggest:$cursorPosition]\" $text) | Where-Object {{ $_ -NotMatch \"^-|^/\" }}");
        writer.WriteLine("$dynamicCompletions = $suggestResults | Foreach-Object { [CompletionResult]::new($_, $_, [CompletionResultType]::ParameterValue, $_) }");
        writer.WriteLine("$completions += $dynamicCompletions");
    }


    private static void GenerateDynamicCompletionsForArguments(string[] commandNameList, IList<Argument> arguments, IndentedTextWriter writer)
    {

    }
    private static void GenerateDynamicCompletionsForOptions(string[] commandNameList, IList<Option> options, IndentedTextWriter writer)
    {

    }
}
