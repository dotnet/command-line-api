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
        // A command's index is its state ID in the generated PowerShell script.
        // Use the same ordered list for both context tracking and completion candidates.
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

        // The tracker selects a command; the existing candidate generation and filtering still
        // apply to that command. The recorded option and argument context is not used here yet.
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

    // PowerShell single-quoted strings escape a single quote by doubling it.
    private static string Quote(string value) => $"'{value.Replace("'", "''")}'";

    // Emit PowerShell literals rather than the C# Boolean names.
    private static string Boolean(bool value) => value ? "$true" : "$false";

    // Include hidden commands for recognition even though they are not completion candidates.
    // The guard also gives a shared command one state ID if it has multiple parents.
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

    /// <summary>
    /// Emit the command metadata and two-pass tracker used by the native PowerShell completer.
    /// </summary>
    /// <remarks>
    /// The script recognizes the command line up to the cursor without invoking the application.
    /// It approximates the default System.CommandLine tokenizer and parser, not PowerShell evaluation
    /// or response-file expansion. Keep its recognition rules aligned with StringExtensions.Tokenize
    /// and ParseOperation when those rules change.
    ///
    /// PowerShell passes an AST (abstract syntax tree) and a cursor offset to the completer.
    /// The script uses hashtables (<c>@{ ... }</c>) for metadata, arrays (<c>@( ... )</c>) for
    /// ordered values, and <c>$null</c> for absent values. Its first pass maps completed AST
    /// elements to tokens. Its second pass replays those tokens to track the active command,
    /// pending option, and positional argument. The cursor's current word is not replayed.
    /// </remarks>
    private static void WriteContextTracking(IndentedTextWriter writer, List<Command> commands)
    {
        // Each state contains case-sensitive name lookups and ordered positional argument metadata.
        // PowerShell comparisons are case-insensitive by default, so the dictionaries use Ordinal.
        // Commands maps names to state IDs. Options maps names to a shared option record.
        // Arguments stores each positional argument's maximum arity and capture flag.
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
            // The current command's own names are recognized without moving to another state.
            // Every alias of a child points to the same state as its canonical name.
            foreach (var child in new[] { command }.Concat(command.Subcommands))
            {
                foreach (var name in child.Names())
                {
                    writer.WriteLine($"$states[{i}].Commands.Add({Quote(name)}, {commands.IndexOf(child)})");
                }
            }

            // Recognition includes hidden symbols and every alias, unlike the candidate lists.
            // Reserve command names first, then local options before recursive ancestor options.
            var names = new HashSet<string>(command.Names().Concat(command.Subcommands.SelectMany(c => c.Names())), StringComparer.Ordinal);
            for (Command? current = command; current is not null; current = current.Parents.OfType<Command>().FirstOrDefault())
            {
                foreach (var option in current.Options.Where(o => current == command || o.Recursive))
                {
                    // Name identifies the option across aliases. Maximum is per occurrence and
                    // is limited to one unless AllowMultipleArgumentsPerToken is enabled.
                    // Greedy is true for a required value except on non-nullable Boolean options.
                    // Boolean controls whether an argument token can be consumed as a value.
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

        // These comments are emitted with the script so generated scripts can also be maintained.
        // Keep tokenization separate from argument assignment, as the parser does for capture.
        foreach (var line in
"""
# Register-ArgumentCompleter passes the current word, the parsed command AST, and an absolute
# cursor offset. CommandElements includes the executable, so tracking starts after that element.
# First classify completed elements into parser-like tokens. The second pass assigns their values.
$tokens = [System.Collections.Generic.List[object]]::new()
$tokenState = 0
$previousOption = $null
$endOfOptions = $false
$inDirectives = $true
foreach ($element in $commandAst.CommandElements | Select-Object -Skip 1) {
    # Do not consume the element being completed or any element after the cursor.
    # If the cursor is inside an element, keep only the prefix for candidate filtering.
    if ($element.Extent.EndOffset -ge $cursorPosition) {
        if ($element.Extent.StartOffset -lt $cursorPosition) {
            $length = [Math]::Min($wordToComplete.Length, $cursorPosition - $element.Extent.StartOffset)
            $wordToComplete = $wordToComplete.Substring(0, $length)
        }
        break
    }
    # Use the unquoted value of a PowerShell string literal. For other AST nodes, use their text
    # without evaluating expressions, variables, or substitutions.
    $word = if ($element -is [StringConstantExpressionAst]) { $element.Value } else { $element.Extent.Text }
    $context = $states[$tokenState]
    if ($endOfOptions) {
        $tokens.Add(@{ Kind = 'argument'; Value = $word })
        continue
    }
    # -ceq is case-sensitive. The end marker makes all later elements positional tokens.
    if ($word -ceq '--') {
        $endOfOptions = $true
        $previousOption = $null
        $tokens.Add(@{ Kind = 'end' })
        continue
    }
    # Directives are recognized only before the first non-directive element.
    if ($inDirectives -and $word -match '^\[[^:\]][^\]]*\]$') { continue }
    $inDirectives = $false

    # A greedy option classifies the next known name as an argument, even if it names a subcommand.
    # Otherwise, command aliases select a new tokenization state and option aliases select metadata.
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
        # Split an inline value, such as --name=value or -n:value, into option and argument tokens.
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
            # Expand a POSIX-style bundle such as -ab. A greedy option or an unknown character
            # after a recognized option makes the remainder an argument value.
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

# Replay the tokens to find the parser context at the cursor. $tokenState above is only for
# token recognition; $state can differ when a positional argument captures subsequent tokens.
# Option counts are per occurrence. Argument counts and indices belong to the current command.
$state = 0
$pendingOption = $null
$optionValueCount = 0
$argumentIndex = 0
$argumentValueCount = 0
$endOfOptions = $false
$capturing = $false
foreach ($token in $tokens) {
    $arguments = $states[$state].Arguments
    # Skip positional arguments whose maximum arity is already filled.
    while ($argumentIndex -lt $arguments.Count -and $argumentValueCount -ge $arguments[$argumentIndex].Maximum) {
        $argumentIndex++
        $argumentValueCount = 0
    }
    # CaptureRemainingTokens can consume command and option tokens after capture starts.
    # An end marker before capture starts keeps its normal meaning.
    $capture = $argumentIndex -lt $arguments.Count -and $arguments[$argumentIndex].Capture -and
        ($capturing -or ($token.Kind -ne 'end' -and ($token.Kind -eq 'argument' -or $argumentIndex -gt 0 -or $argumentValueCount -gt 0)))
    # A pending option consumes contiguous argument tokens up to its per-occurrence maximum.
    # Boolean options consume only explicit true or false values, ignoring case and whitespace.
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
# Advance once more so the index identifies the positional argument at the cursor.
$arguments = $states[$state].Arguments
while ($argumentIndex -lt $arguments.Count -and $argumentValueCount -ge $arguments[$argumentIndex].Maximum) {
    $argumentIndex++
    $argumentValueCount = 0
}
# Keep the option and positional context for later completion features. For an inline value,
# separate the option prefix from the value prefix without treating the current word as consumed.
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
        // PowerShell's cursor offset is relative to the full input, but [suggest] receives
        // only the command AST text and expects an offset relative to that text.
        writer.WriteLine("$suggestPosition = $cursorPosition - $commandAst.Extent.StartOffset");
        writer.WriteLine($"$suggestResults = @(& {Quote(binaryName)} \"[suggest:$suggestPosition]\" $text) | Where-Object {{ $_ -NotMatch \"^-|^/\" }}");
        writer.WriteLine("$dynamicCompletions = $suggestResults | Foreach-Object { [CompletionResult]::new($_, $_, [CompletionResultType]::ParameterValue, $_) }");
        writer.WriteLine("$completions += $dynamicCompletions");
    }
}
