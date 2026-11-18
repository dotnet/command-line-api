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
        var commands = new List<Command>();
        CollectCommands(command, commands);

        using var textWriter = new StringWriter { NewLine = "\n" };
        using var writer = new IndentedTextWriter(textWriter);
        writer.WriteLine(
$$$"""
using namespace System.Management.Automation
using namespace System.Management.Automation.Language

Register-ArgumentCompleter -Native -CommandName {{{Quote(binaryName)}}} -ScriptBlock {
    param($wordToComplete, $commandAst, $cursorPosition)

""");
        writer.Indent++;
        WriteContextTracking(writer, commands);

        writer.WriteLine("$completions = @()");
        writer.WriteLine("switch ($state) {");
        writer.Indent++;
        for (int i = 0; i < commands.Count; i++)
        {
            GenerateStaticCompletionsForCommand(i, binaryName, commands[i], writer);
        }

        writer.Indent--;
        writer.WriteLine("}");
        writer.WriteLine("$completions | Where-Object -FilterScript { $_.CompletionText -like \"$wordToComplete*\" } | Sort-Object -Property ListItemText");

        writer.Indent--;
        writer.WriteLine("}");
        writer.Flush();
        return textWriter.ToString();
    }

    private static string Quote(string value) => $"'{value.Replace("'", "''")}'";

    private static string Boolean(bool value) => value ? "$true" : "$false";

    private static void CollectCommands(Command command, List<Command> commands)
    {
        if (commands.Contains(command))
        {
            return;
        }

        commands.Add(command);
        foreach (var child in command.Subcommands)
        {
            CollectCommands(child, commands);
        }
    }

    private static void WriteContextTracking(IndentedTextWriter writer, List<Command> commands)
    {
        writer.WriteLine("$states = @(");
        writer.Indent++;
        foreach (var command in commands)
        {
            writer.WriteLine("@{");
            writer.Indent++;
            writer.WriteLine("Commands = [System.Collections.Generic.Dictionary[string, int]]::new([StringComparer]::Ordinal)");
            writer.WriteLine("Options = [System.Collections.Generic.Dictionary[string, object]]::new([StringComparer]::Ordinal)");
            writer.WriteLine("Arguments = @(");
            writer.Indent++;
            foreach (var argument in command.Arguments)
            {
                writer.WriteLine($"@{{ Maximum = {argument.Arity.MaximumNumberOfValues}; Capture = {Boolean(argument.CaptureRemainingTokens)} }}");
            }
            writer.Indent--;
            writer.WriteLine(")");
            writer.Indent--;
            writer.WriteLine("}");
        }
        writer.Indent--;
        writer.WriteLine(")");

        for (int i = 0; i < commands.Count; i++)
        {
            var command = commands[i];
            foreach (var child in new[] { command }.Concat(command.Subcommands))
            {
                foreach (var name in child.Names())
                {
                    writer.WriteLine($"$states[{i}].Commands.Add({Quote(name)}, {commands.IndexOf(child)})");
                }
            }

            // Recognition includes hidden symbols and every alias, unlike the candidate lists.
            var names = new HashSet<string>(command.Names().Concat(command.Subcommands.SelectMany(c => c.Names())), StringComparer.Ordinal);
            for (Command? current = command; current is not null; current = current.Parents.OfType<Command>().FirstOrDefault())
            {
                foreach (var option in current.Options.Where(o => current == command || o.Recursive))
                {
                    bool boolean = option.ValueType == typeof(bool) || option.ValueType == typeof(bool?);
                    int maximum = option.AllowMultipleArgumentsPerToken ? option.Arity.MaximumNumberOfValues : Math.Min(1, option.Arity.MaximumNumberOfValues);
                    writer.WriteLine($"$option = @{{ Name = {Quote(option.Name)}; Maximum = {maximum}; Greedy = {Boolean(option.Arity.MinimumNumberOfValues > 0 && option.ValueType != typeof(bool))}; Boolean = {Boolean(boolean)} }}");
                    foreach (var name in new[] { option.Name }.Concat(option.Aliases).Where(names.Add))
                    {
                        writer.WriteLine($"$states[{i}].Options.Add({Quote(name)}, $option)");
                    }
                }
            }
        }

        // Tokenize before assigning positional arguments, as the parser does for CaptureRemainingTokens.
        foreach (var line in
"""
$tokens = [System.Collections.Generic.List[object]]::new()
$tokenState = 0
$previousOption = $null
$endOfOptions = $false
$inDirectives = $true
foreach ($element in $commandAst.CommandElements | Select-Object -Skip 1) {
    if ($element.Extent.EndOffset -ge $cursorPosition) {
        if ($element.Extent.StartOffset -lt $cursorPosition) {
            $length = [Math]::Min($wordToComplete.Length, $cursorPosition - $element.Extent.StartOffset)
            $wordToComplete = $wordToComplete.Substring(0, $length)
        }
        break
    }
    $word = if ($element -is [StringConstantExpressionAst]) { $element.Value } else { $element.Extent.Text }
    $context = $states[$tokenState]
    if ($endOfOptions) {
        $tokens.Add(@{ Kind = 'argument'; Value = $word })
        continue
    }
    if ($word -ceq '--') {
        $endOfOptions = $true
        $previousOption = $null
        $tokens.Add(@{ Kind = 'end' })
        continue
    }
    if ($inDirectives -and $word -match '^\[[^:\]][^\]]*\]$') { continue }
    $inDirectives = $false

    $known = $context.Commands.ContainsKey($word) -or $context.Options.ContainsKey($word)
    if ($known -and $previousOption -and $previousOption.Greedy) {
        $tokens.Add(@{ Kind = 'argument'; Value = $word })
        $previousOption = $null
    } elseif ($context.Commands.ContainsKey($word)) {
        $nextState = $context.Commands[$word]
        if ($nextState -eq $tokenState) {
            $tokens.Add(@{ Kind = 'argument'; Value = $word })
        } else {
            $tokenState = $nextState
            $tokens.Add(@{ Kind = 'command'; State = $tokenState })
        }
        $previousOption = $null
    } elseif ($context.Options.ContainsKey($word)) {
        $previousOption = $context.Options[$word]
        $tokens.Add(@{ Kind = 'option'; Option = $previousOption })
    } else {
        $separator = $word.IndexOfAny([char[]]'=:')
        $name = if ($separator -ge 0) { $word.Substring(0, $separator) } else { $word }
        if ($separator -ge 0 -and $context.Options.ContainsKey($name)) {
            $previousOption = $context.Options[$name]
            $tokens.Add(@{ Kind = 'option'; Option = $previousOption })
            if ($separator -lt $word.Length - 1) {
                $tokens.Add(@{ Kind = 'argument'; Value = $word.Substring($separator + 1) })
                $previousOption = $null
            }
        } elseif ($word.Length -gt 2 -and $word[0] -eq '-' -and $word[1] -ne '-' -and
                  $word[2] -notin ':', '=' -and -not ($previousOption -and $previousOption.Greedy)) {
            $bundle = [System.Collections.Generic.List[object]]::new()
            for ($j = 1; $j -lt $word.Length; $j++) {
                $name = '-' + $word[$j]
                if (-not $context.Options.ContainsKey($name)) {
                    if ($bundle.Count -gt 0) {
                        $bundle.Add(@{ Kind = 'argument'; Value = $word.Substring($j) })
                    }
                    break
                }
                $option = $context.Options[$name]
                $bundle.Add(@{ Kind = 'option'; Option = $option })
                if ($j -lt $word.Length - 1 -and ($option.Greedy -or $word[$j + 1] -in ':', '=')) {
                    $start = $j + 1
                    if ($word[$start] -in ':', '=') { $start++ }
                    $bundle.Add(@{ Kind = 'argument'; Value = $word.Substring($start) })
                    break
                }
            }
            if ($bundle.Count -gt 0) {
                $tokens.AddRange($bundle)
                $last = $bundle[$bundle.Count - 1]
                $previousOption = if ($last.Kind -eq 'option') { $last.Option } else { $null }
            } else {
                $tokens.Add(@{ Kind = 'argument'; Value = $word })
                $previousOption = $null
            }
        } else {
            $tokens.Add(@{ Kind = 'argument'; Value = $word })
            $previousOption = $null
        }
    }
}

$state = 0
$pendingOption = $null
$optionValueCount = 0
$argumentIndex = 0
$argumentValueCount = 0
$endOfOptions = $false
$capturing = $false
foreach ($token in $tokens) {
    $arguments = $states[$state].Arguments
    while ($argumentIndex -lt $arguments.Count -and $argumentValueCount -ge $arguments[$argumentIndex].Maximum) {
        $argumentIndex++
        $argumentValueCount = 0
    }
    $capture = $argumentIndex -lt $arguments.Count -and $arguments[$argumentIndex].Capture -and
        ($capturing -or ($token.Kind -ne 'end' -and ($token.Kind -eq 'argument' -or $argumentIndex -gt 0 -or $argumentValueCount -gt 0)))
    if ($pendingOption -and $token.Kind -eq 'argument') {
        if (-not $pendingOption.Boolean -or $token.Value.Trim() -imatch '^(true|false)$') {
            $optionValueCount++
            if ($optionValueCount -ge $pendingOption.Maximum) { $pendingOption = $null }
            continue
        }
    }
    $pendingOption = $null
    if ($capture) {
        $capturing = $true
        $argumentValueCount++
    } elseif ($token.Kind -eq 'end') {
        $endOfOptions = $true
    } elseif ($token.Kind -eq 'command') {
        $state = $token.State
        $argumentIndex = 0
        $argumentValueCount = 0
        $capturing = $false
    } elseif ($token.Kind -eq 'option') {
        $optionValueCount = 0
        if ($token.Option.Maximum -gt 0) { $pendingOption = $token.Option }
    } elseif ($argumentIndex -lt $arguments.Count) {
        $argumentValueCount++
    }
}
$arguments = $states[$state].Arguments
while ($argumentIndex -lt $arguments.Count -and $argumentValueCount -ge $arguments[$argumentIndex].Maximum) {
    $argumentIndex++
    $argumentValueCount = 0
}
$completionOption = $pendingOption
$valuePrefix = ''
$valueToComplete = $wordToComplete
$separator = $wordToComplete.IndexOfAny([char[]]'=:')
if (-not $endOfOptions -and -not $capturing -and $separator -ge 0) {
    $name = $wordToComplete.Substring(0, $separator)
    if ($states[$state].Options.ContainsKey($name) -and $states[$state].Options[$name].Maximum -gt 0) {
        $completionOption = $states[$state].Options[$name]
        $valuePrefix = $wordToComplete.Substring(0, $separator + 1)
        $valueToComplete = $wordToComplete.Substring($separator + 1)
    }
}
""".ReplaceLineEndings("\n").Split('\n'))
        {
            if (line.Length == 0)
            {
                writer.InnerWriter.WriteLine();
            }
            else
            {
                writer.WriteLine(line);
            }
        }
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
    /// Generate completions for the statically-known options, arguments, and subcommands of a given command.
    /// </summary>
    private static void GenerateStaticCompletionsForCommand(int state, string binaryName, Command command, IndentedTextWriter writer)
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

        writer.WriteLine($"{state} {{");
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
            GenerateDynamicCompletionsCall(binaryName, writer);
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
        writer.WriteLine("$suggestPosition = $cursorPosition - $commandAst.Extent.StartOffset");
        writer.WriteLine($"$suggestResults = @(& {Quote(binaryName)} \"[suggest:$suggestPosition]\" $text) | Where-Object {{ $_ -NotMatch \"^-|^/\" }}");
        writer.WriteLine("$dynamicCompletions = $suggestResults | Foreach-Object { [CompletionResult]::new($_, $_, [CompletionResultType]::ParameterValue, $_) }");
        writer.WriteLine("$completions += $dynamicCompletions");
    }
}
