// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.CodeDom.Compiler;
using System.CommandLine.Completions;
using System.CommandLine.StaticCompletions.Resources;

namespace System.CommandLine.StaticCompletions.Shells;

public class FishShellProvider : IShellProvider
{
    public string ArgumentName => ShellNames.Fish;

    public string Extension => "fish";

    public string HelpDescription => Strings.FishShellProvider_HelpDescription;

    // override the ToString method to return the argument name so that CLI help is cleaner for 'default' values
    public override string ToString() => ArgumentName;

    // Windows-style ('/'-prefixed) aliases are not offered as candidates; the state walker still recognizes them.
    private static IEnumerable<string> SanitizeOptionNames(IEnumerable<string> names) =>
        names.Where(n => n.StartsWith('-'));

    public string GenerateCompletions(Command command)
    {
        var safeName = command.Name.MakeSafeFunctionName();

        using var textWriter = new StringWriter { NewLine = "\n" };
        using var writer = new IndentedTextWriter(textWriter);

        writer.WriteLine($"# fish completions for {command.Name}");
        writer.WriteLineNoTabs("");

        // Collect all commands in a flat list and assign numeric state IDs
        var states = new List<(int id, Command cmd)>();
        CollectStates(command, states);
        var stateIdByCommand = states.ToDictionary(s => s.cmd, s => s.id);

        // Write the main completion function
        writer.WriteLine($"function _{safeName}");
        writer.Indent++;

        var options = states.SelectMany(s => ParsingOptions(s.cmd)).Distinct().ToArray();
        WriteStateWalker(writer, states, stateIdByCommand, options);
        writer.WriteLineNoTabs("");
        WriteOptionValueCompletions(writer, options);
        WriteStateCompletions(writer, states);

        writer.Indent--;
        writer.WriteLine("end");
        writer.WriteLineNoTabs("");

        writer.WriteLine($"complete -c {command.Name} -f -a '(_{safeName})'");

        writer.Flush();
        return textWriter.ToString();
    }

    /// <summary>
    /// Recursively collect commands into a flat list with numeric state IDs.
    /// State 0 is the root command.
    /// </summary>
    private static void CollectStates(Command cmd, List<(int id, Command cmd)> states)
    {
        states.Add((states.Count, cmd));
        foreach (var sub in cmd.Subcommands)
        {
            CollectStates(sub, states);
        }
    }

    private static IEnumerable<Option> ParsingOptions(Command command) =>
        command.Options.Concat(command.Parents.OfType<Command>()
            .SelectMany(ParsingOptions).Where(o => o.Recursive)).DistinctBy(o => o.Name);

    private static string Cases(IEnumerable<string> names) =>
        string.Join(" ", names.Select(n => FishEscape(n.Replace("\\", "\\\\").Replace("*", "\\*").Replace("?", "\\?").Replace("[", "\\["))));

    private static void WriteStateWalker(
        IndentedTextWriter writer,
        List<(int id, Command cmd)> states,
        Dictionary<Command, int> stateIdByCommand,
        Option[] options)
    {
        // Keep the cursor token in the walk so the tracker can report its completion context without counting it as consumed.
        writer.WriteLine("set -l tokens (commandline -opc)");
        writer.WriteLine("set -l current_token (commandline -ct)");
        writer.WriteLine("set -a tokens \"$current_token\"");
        writer.WriteLine("set -l state 0");
        // Argument ownership follows positional values, not token indexes; option names and their values do not fill argument slots.
        writer.WriteLine("set -l position 0");
        writer.WriteLine("set -l end_options 0");
        writer.WriteLine("set -l capture_started 0");
        writer.WriteLine("set -l capture 0");
        writer.WriteLine("set -l pending 0");
        writer.WriteLine("set -l remaining 0");
        writer.WriteLine("set -l first_value 0");
        writer.WriteLine("set -l greedy 0");
        writer.WriteLine("set -l boolean 0");
        writer.WriteLine("set -l value_prefix ''");
        writer.WriteLine("set -l i 2");
        writer.WriteLine("while test $i -le (count $tokens)");
        writer.Indent++;
        writer.WriteLine("set -l word \"$tokens[$i]\"");
        writer.WriteLine("set -l current 0");
        writer.WriteLine("if test $i -eq (count $tokens)");
        writer.Indent++;
        writer.WriteLine("set current 1");
        writer.Indent--;
        writer.WriteLine("end");
        writer.WriteLine("set -l kind argument");
        writer.WriteLine("set -l next_state $state");
        writer.WriteLine("set -l option_id 0");
        writer.WriteLine("set -l maximum 0");
        writer.WriteLine("set -l option_greedy 0");
        writer.WriteLine("set -l option_boolean 0");
        writer.WriteLine("set -l inline 0");
        writer.WriteLine("set -l option_names");
        writer.WriteLine("set -l option_ids");
        writer.WriteLine("set -l option_maxima");
        writer.WriteLine("set -l option_greedies");
        writer.WriteLine("set -l option_booleans");
        writer.WriteLine("set -l option_name (string replace -r '[:=].*$' '' -- \"$word\")");
        writer.WriteLine("if test $end_options -eq 0");
        writer.Indent++;
        writer.WriteLine("switch $state");
        writer.Indent++;

        foreach (var (stateId, cmd) in states)
        {
            writer.WriteLine($"case {stateId}");
            writer.Indent++;
            var namedOptions = ParsingOptions(cmd)
                .SelectMany(option => new[] { option.Name }.Concat(option.Aliases).Select(name => (name, option)))
                .ToArray();
            // Without AllowMultipleArgumentsPerToken, one shell token cannot supply multiple option values.
            var maximumValues = namedOptions.Select(o => o.option.AllowMultipleArgumentsPerToken
                ? o.option.Arity.MaximumNumberOfValues
                : Math.Min(1, o.option.Arity.MaximumNumberOfValues));
            var greedyValues = namedOptions.Select(o =>
                o.option.Arity.MinimumNumberOfValues > 0 && o.option.ValueType != typeof(bool) ? 1 : 0);
            var booleanValues = namedOptions.Select(o =>
                o.option.ValueType == typeof(bool) || o.option.ValueType == typeof(bool?) ? 1 : 0);
            writer.WriteLine($"set option_names {string.Join(" ", namedOptions.Select(o => FishEscape(o.name)))}".TrimEnd());
            writer.WriteLine($"set option_ids {string.Join(" ", namedOptions.Select(o => Array.IndexOf(options, o.option) + 1))}".TrimEnd());
            writer.WriteLine($"set option_maxima {string.Join(" ", maximumValues)}".TrimEnd());
            writer.WriteLine($"set option_greedies {string.Join(" ", greedyValues)}".TrimEnd());
            writer.WriteLine($"set option_booleans {string.Join(" ", booleanValues)}".TrimEnd());
            writer.WriteLine("switch $word");
            writer.Indent++;
            foreach (var sub in cmd.Subcommands)
            {
                writer.WriteLine($"case {Cases(sub.Names())}");
                writer.Indent++;
                writer.WriteLine("set kind command");
                writer.WriteLine($"set next_state {stateIdByCommand[sub]}");
                writer.Indent--;
            }
            writer.Indent--;
            writer.WriteLine("end");
            writer.Indent--;
        }
        writer.Indent--;
        writer.WriteLine("end");
        // Split short-option bundles into ordinary tokens so the same arity and value rules apply to bundled options.
        foreach (var line in """
            if test "$kind" != command
                set -l index (contains -i -- "$word" $option_names)
                if test (count $index) -eq 0
                    set index (contains -i -- "$option_name" $option_names)
                    if test (count $index) -gt 0
                        set inline 1
                    end
                end
                if test (count $index) -gt 0
                    set kind option
                    set option_id $option_ids[$index]
                    set maximum $option_maxima[$index]
                    set option_greedy $option_greedies[$index]
                    set option_boolean $option_booleans[$index]
                else if test $end_options -eq 0; and string match -qr '^-[^-:=].+' -- "$word"
                    if not test $pending -gt 0 -a $first_value -eq 1 -a $greedy -eq 1
                        set -l bundle
                        set -l j 2
                        while test $j -le (string length -- "$word")
                            set -l short -(string sub -s $j -l 1 -- "$word")
                            set index (contains -i -- "$short" $option_names)
                            if test (count $index) -eq 0
                                if test (count $bundle) -gt 0
                                    set -l rest (string sub -s $j -- "$word" | string replace -r '^[:=]' '')
                                    set bundle[-1] "$bundle[-1]=$rest"
                                    if test $current -eq 1
                                        set value_prefix (string sub -l (math $j - 1) -- "$word")
                                    end
                                end
                                break
                            end
                            set -a bundle "$short"
                            set -l rest (string sub -s (math $j + 1) -- "$word")
                            if test -n "$rest"; and begin; test $option_greedies[$index] -eq 1; or string match -qr '^[:=]' -- "$rest"; end
                                set -l prefix_length $j
                                if string match -qr '^[:=]' -- "$rest"
                                    set prefix_length (math $j + 1)
                                end
                                set rest (string replace -r '^[:=]' '' -- "$rest")
                                set bundle[-1] "$short=$rest"
                                if test $current -eq 1
                                    set value_prefix (string sub -l $prefix_length -- "$word")
                                end
                                break
                            end
                            set j (math $j + 1)
                        end
                        if test (count $bundle) -gt 0
                            set -l before (math $i - 1)
                            set -l after (math $i + 1)
                            set tokens $tokens[1..$before] $bundle $tokens[$after..-1]
                            continue
                        end
                    end
                end
            end
            """.Split('\n'))
        {
            writer.WriteLine(line);
        }
        writer.Indent--;
        writer.WriteLine("end");
        // CaptureRemainingTokens can turn option-looking words into values, so detect its active slot before consuming the token.
        writer.WriteLine("set capture 0");
        writer.WriteLine("switch $state");
        writer.Indent++;
        foreach (var (stateId, cmd) in states.Where(s => s.cmd.Arguments.Any(a => a.CaptureRemainingTokens)))
        {
            writer.WriteLine($"case {stateId}");
            writer.Indent++;
            long start = 0;
            foreach (var argument in cmd.Arguments)
            {
                long end = start + argument.Arity.MaximumNumberOfValues;
                if (argument.CaptureRemainingTokens)
                {
                    writer.WriteLine($"if test $position -ge {start} -a $position -lt {end}");
                    writer.Indent++;
                    writer.WriteLine("if test $position -gt 0 -o $capture_started -eq 1; or test \"$kind\" = argument -a $current -eq 0");
                    writer.Indent++;
                    writer.WriteLine("set capture 1");
                    writer.Indent--;
                    writer.WriteLine("end");
                    writer.Indent--;
                    writer.WriteLine("end");
                }
                start = end;
            }
            writer.Indent--;
        }
        writer.Indent--;
        writer.WriteLine("end");
        writer.WriteLine("if string match -q -- -- \"$word\"; and test $end_options -eq 0");
        writer.Indent++;
        writer.WriteLine("if test $current -eq 1");
        writer.Indent++;
        writer.WriteLine("set pending 0");
        writer.WriteLine("break");
        writer.Indent--;
        writer.WriteLine("end");
        writer.WriteLine("set end_options 1");
        writer.WriteLine("if test $capture_started -eq 0 -o $capture -eq 0");
        writer.Indent++;
        writer.WriteLine("set pending 0");
        writer.WriteLine("set i (math $i + 1)");
        writer.WriteLine("continue");
        writer.Indent--;
        writer.WriteLine("end");
        writer.Indent--;
        writer.WriteLine("end");
        writer.WriteLine("if test $pending -gt 0");
        writer.Indent++;
        writer.WriteLine("set -l consume 0");
        // Required option values can look like commands or options; optional values must not swallow those symbols.
        writer.WriteLine("if test \"$kind\" = argument; or test $first_value -eq 1 -a $greedy -eq 1 -a $inline -eq 0");
        writer.Indent++;
        writer.WriteLine("set consume 1");
        writer.Indent--;
        writer.WriteLine("end");
        writer.WriteLine("if test $boolean -eq 1");
        writer.Indent++;
        writer.WriteLine("if test $current -eq 0; and not string match -qi -r '^(true|false)$' -- \"$word\"");
        writer.Indent++;
        writer.WriteLine("set consume 0");
        writer.Indent--;
        writer.WriteLine("else if test $current -eq 1; and not string match -qi -- \"$word*\" true false");
        writer.Indent++;
        writer.WriteLine("set consume 0");
        writer.Indent--;
        writer.WriteLine("end");
        writer.Indent--;
        writer.WriteLine("end");
        writer.WriteLine("if test $consume -eq 1");
        writer.Indent++;
        writer.WriteLine("if test $current -eq 1");
        writer.Indent++;
        writer.WriteLine("break");
        writer.Indent--;
        writer.WriteLine("end");
        writer.WriteLine("set remaining (math $remaining - 1)");
        writer.WriteLine("set first_value 0");
        writer.WriteLine("if test $remaining -eq 0");
        writer.Indent++;
        writer.WriteLine("set pending 0");
        writer.Indent--;
        writer.WriteLine("end");
        writer.WriteLine("set i (math $i + 1)");
        writer.WriteLine("continue");
        writer.Indent--;
        writer.WriteLine("end");
        writer.WriteLine("set pending 0");
        writer.Indent--;
        writer.WriteLine("end");
        writer.WriteLine("if test $capture -eq 1");
        writer.Indent++;
        writer.WriteLine("set kind argument");
        writer.WriteLine("set capture_started 1");
        writer.Indent--;
        writer.WriteLine("end");
        writer.WriteLine("if test \"$kind\" = option -a $maximum -gt 0");
        writer.Indent++;
        writer.WriteLine("if test $current -eq 0 -o $inline -eq 1");
        writer.Indent++;
        writer.WriteLine("set pending $option_id");
        writer.WriteLine("set remaining $maximum");
        writer.WriteLine("set first_value 1");
        writer.WriteLine("set greedy $option_greedy");
        writer.WriteLine("set boolean $option_boolean");
        writer.WriteLine("if test $inline -eq 1 -a $current -eq 1; and test -z \"$value_prefix\"");
        writer.Indent++;
        writer.WriteLine("set value_prefix (string replace -r '^([^:=]*[:=]).*$' '$1' -- \"$word\")");
        writer.Indent--;
        writer.WriteLine("end");
        writer.WriteLine("if test $inline -eq 1 -a $current -eq 0; and string match -q -r '[:=].+' -- \"$word\"");
        writer.Indent++;
        writer.WriteLine("set remaining (math $remaining - 1)");
        writer.WriteLine("set first_value 0");
        writer.WriteLine("if test $boolean -eq 1; and not string match -qi -r '[:=](true|false)$' -- \"$word\"");
        writer.Indent++;
        writer.WriteLine("set remaining 0");
        writer.WriteLine("set position (math $position + 1)");
        writer.Indent--;
        writer.WriteLine("end");
        writer.WriteLine("if test $remaining -eq 0");
        writer.Indent++;
        writer.WriteLine("set pending 0");
        writer.Indent--;
        writer.WriteLine("end");
        writer.Indent--;
        writer.WriteLine("end");
        writer.Indent--;
        writer.WriteLine("end");
        writer.Indent--;
        writer.WriteLine("end");
        writer.WriteLine("if test $current -eq 1");
        writer.Indent++;
        writer.WriteLine("break");
        writer.Indent--;
        writer.WriteLine("end");
        writer.WriteLine("switch $kind");
        writer.Indent++;
        writer.WriteLine("case command");
        writer.Indent++;
        // Positional arguments belong to the selected command, so a subcommand starts a fresh slot sequence.
        writer.WriteLine("set state $next_state");
        writer.WriteLine("set position 0");
        writer.WriteLine("set capture_started 0");
        writer.Indent--;
        writer.WriteLine("case argument");
        writer.Indent++;
        writer.WriteLine("set position (math $position + 1)");
        writer.WriteLine("if test $capture -eq 1 -a $inline -eq 1; and string match -q -r '[:=].+' -- \"$word\"");
        writer.Indent++;
        writer.WriteLine("set position (math $position + 1)");
        writer.Indent--;
        writer.WriteLine("end");
        writer.Indent--;
        writer.WriteLine("case option");
        writer.Indent++;
        writer.WriteLine("if test $maximum -eq 0 -a $inline -eq 1; and string match -q -r '[:=].+' -- \"$word\"");
        writer.Indent++;
        writer.WriteLine("set position (math $position + 1)");
        writer.Indent--;
        writer.WriteLine("end");
        writer.Indent--;
        writer.Indent--;
        writer.WriteLine("end");
        writer.WriteLine("set i (math $i + 1)");
        writer.Indent--;
        writer.WriteLine("end");
    }

    private static void WriteOptionValueCompletions(IndentedTextWriter writer, Option[] options)
    {
        writer.WriteLine("if test $pending -gt 0");
        writer.Indent++;
        writer.WriteLine("switch $pending");
        writer.Indent++;
        for (int i = 0; i < options.Length; i++)
        {
            var option = options[i];
            if (option.Hidden || option.IsFlag()) continue;
            writer.WriteLine($"case {i + 1}");
            writer.Indent++;
            if (option.IsDynamic)
            {
                writer.WriteLine(GenerateDynamicCall() + " | while read -l candidate");
                writer.Indent++;
                writer.WriteLine("printf '%s%s\\n' \"$value_prefix\" \"$candidate\"");
                writer.Indent--;
                writer.WriteLine("end");
            }
            else
            {
                foreach (var c in option.GetCompletions(CompletionContext.Empty))
                    WriteCompletionCandidate(writer, c, prefixValue: true);
            }
            writer.Indent--;
        }
        writer.Indent--;
        writer.WriteLine("end");
        writer.WriteLine("return");
        writer.Indent--;
        writer.WriteLine("end");
        writer.WriteLineNoTabs("");
    }

    /// <summary>
    /// Generate the main completion output for each state.
    /// Emits subcommands, options, and positional argument completions for the current context.
    /// </summary>
    private static void WriteStateCompletions(IndentedTextWriter writer, List<(int id, Command cmd)> states)
    {
        writer.WriteLine("switch $state");
        writer.Indent++;

        foreach (var (stateId, cmd) in states)
        {
            writer.WriteLine($"case {stateId}");
            writer.Indent++;

            writer.WriteLine("if test $end_options -eq 0 -a $capture -eq 0");
            writer.Indent++;
            // Subcommand completions
            foreach (var sub in cmd.Subcommands.Where(c => !c.Hidden))
            {
                WriteCandidate(writer, sub.Name, SanitizeDescription(sub.Description));
            }

            // Option completions - emit all supported aliases so both -h and --help are completable
            foreach (var option in cmd.HierarchicalOptions().Where(o => !o.Hidden))
            {
                var desc = SanitizeDescription(option.Description);
                foreach (var name in SanitizeOptionNames(option.Names()))
                {
                    WriteCandidate(writer, name, desc);
                }
            }
            writer.Indent--;
            writer.WriteLine("end");

            // Arity ranges map the consumed positional count to exactly one argument, including optional and repeated arguments.
            long start = 0;
            foreach (var arg in cmd.Arguments)
            {
                long end = start + arg.Arity.MaximumNumberOfValues;
                if (arg.Hidden || start == end)
                {
                    start = end;
                    continue;
                }
                writer.WriteLine($"if test $position -ge {start} -a $position -lt {end}");
                writer.Indent++;
                if (arg.IsDynamic)
                {
                    writer.WriteLine(GenerateDynamicCall());
                }
                else
                {
                    var completions = arg.GetCompletions(CompletionContext.Empty).ToArray();
                    foreach (var c in completions)
                    {
                        WriteCompletionCandidate(writer, c);
                    }
                }
                writer.Indent--;
                writer.WriteLine("end");
                start = end;
            }

            writer.Indent--;
        }

        writer.Indent--;
        writer.WriteLine("end");
    }

    /// <summary>
    /// Write a single completion candidate line with an optional description.
    /// Fish uses tab-separated format: candidate\tdescription
    /// </summary>
    private static void WriteCompletionCandidate(IndentedTextWriter writer, CompletionItem completion, bool prefixValue = false)
    {
        var label = completion.InsertText ?? completion.Label;
        var desc = completion.Documentation ?? completion.Detail;
        WriteCandidate(writer, label, desc, prefixValue);
    }

    private static void WriteCandidate(IndentedTextWriter writer, string label, string? description, bool prefixValue = false)
    {
        var candidate = (prefixValue ? "\"$value_prefix\"" : "") + FishEscape(label);
        if (!string.IsNullOrEmpty(description))
            writer.WriteLine($"printf '%s\\t%s\\n' {candidate} {FishEscape(description)}");
        else
            writer.WriteLine($"printf '%s\\n' {candidate}");
    }

    /// <summary>
    /// Generate a dynamic completion call that invokes the application's <c>[suggest]</c> directive
    /// to resolve dynamic completions.
    /// Uses the actual command from the command line ($tokens[1]) so it works regardless of how the binary was invoked.
    /// </summary>
    internal static string GenerateDynamicCall()
    {
        return """command $tokens[1] "[suggest:"(commandline -C)"]" (commandline -cp) 2>/dev/null""";
    }

    /// <summary>
    /// Escape a string for use in a fish single-quoted string.
    /// Fish single-quoted strings support \' and \\ as escape sequences.
    /// </summary>
    internal static string FishEscape(string? s)
    {
        if (string.IsNullOrEmpty(s)) return "''";
        return "'" + s.Replace("\\", "\\\\").Replace("'", "\\'") + "'";
    }

    private static string SanitizeDescription(string? s) =>
        s?.Replace("\r\n", " ").Replace('\n', ' ').Replace('\r', ' ').Replace('\t', ' ') ?? "";
}
