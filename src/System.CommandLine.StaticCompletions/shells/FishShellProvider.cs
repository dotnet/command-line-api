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

    public string GenerateCompletions(Command command)
    {
        var safeName = command.Name.MakeSafeFunctionName();

        using var textWriter = new StringWriter { NewLine = "\n" };
        using var writer = new IndentedTextWriter(textWriter);

        writer.WriteLine($"# fish completions for {command.Name}");
        writer.WriteLine();

        // Collect all commands in a flat list and assign numeric state IDs
        var states = new List<(int id, Command cmd)>();
        CollectStates(command, states);
        var stateIdByCommand = states.ToDictionary(s => s.cmd, s => s.id);

        // Write the main completion function
        writer.WriteLine($"function _{safeName}");
        writer.Indent++;

        WriteTokenization(writer);
        writer.WriteLine();
        WriteStateWalker(writer, states, stateIdByCommand);
        writer.WriteLine();
        WriteOptionValueCompletions(writer, states);
        WriteStateCompletions(writer, states);

        writer.Indent--;
        writer.WriteLine("end");
        writer.WriteLine();

        writer.WriteLine($"complete -c {command.Name} -f -a '(_{safeName})'");

        writer.Flush();
        return textWriter.ToString();
    }

    /// <summary>
    /// Recursively collect all visible commands into a flat list with numeric state IDs.
    /// State 0 is the root command.
    /// </summary>
    private static void CollectStates(Command cmd, List<(int id, Command cmd)> states)
    {
        states.Add((states.Count, cmd));
        foreach (var sub in cmd.Subcommands.Where(c => !c.Hidden))
        {
            CollectStates(sub, states);
        }
    }

    /// <summary>
    /// Write the command line tokenization logic.
    /// Uses fish's commandline builtin to get completed tokens up to the cursor.
    /// </summary>
    private static void WriteTokenization(IndentedTextWriter writer)
    {
        // -opc: tokenize, cut at cursor, only completed tokens (excludes current partial word)
        writer.WriteLine("set -l tokens (commandline -opc)");
    }

    // Options with MaximumNumberOfValues at or above this threshold are treated as unbounded
    // (i.e. consume tokens until an option-like token is encountered).
    // System.CommandLine uses 100_000 as its internal sentinel for ZeroOrMore/OneOrMore.
    private const int UnboundedArityThreshold = 100_000;

    /// <summary>
    /// Generate the state machine that walks completed tokens to determine which subcommand context we're in.
    /// For each state, we check if the current word matches a known subcommand (transitioning to that subcommand's state)
    /// or a value-taking option (skipping tokens for the option's value(s), respecting the option's arity).
    /// </summary>
    private static void WriteStateWalker(IndentedTextWriter writer, List<(int id, Command cmd)> states, Dictionary<Command, int> stateIdByCommand)
    {
        writer.WriteLine("set -l state 0");
        writer.WriteLine("set -l i 2"); // start after the command name (fish arrays are 1-based)
        writer.WriteLine("while test $i -le (count $tokens)");
        writer.Indent++;
        writer.WriteLine("set -l word $tokens[$i]");

        writer.WriteLine("switch $state");
        writer.Indent++;

        foreach (var (stateId, cmd) in states)
        {
            var visibleSubs = cmd.Subcommands.Where(c => !c.Hidden).ToArray();
            var valueOptions = cmd.HierarchicalOptions()
                .Where(o => !o.Hidden && !o.IsFlag())
                .ToArray();

            // Skip states that have no transitions to emit
            if (visibleSubs.Length == 0 && valueOptions.Length == 0)
                continue;

            writer.WriteLine($"case {stateId}");
            writer.Indent++;
            writer.WriteLine("switch $word");
            writer.Indent++;

            // Subcommand transitions
            foreach (var sub in visibleSubs)
            {
                var subStateId = stateIdByCommand[sub];
                var names = string.Join(" ", sub.Names());
                writer.WriteLine($"case {names}");
                writer.Indent++;
                writer.WriteLine($"set state {subStateId}");
                writer.Indent--;
            }

            // Single-value options (arity exactly 1): skip the next token
            var singleValueNames = valueOptions
                .Where(o => o.Arity.MaximumNumberOfValues == 1)
                .SelectMany(o => o.Names())
                .ToArray();
            if (singleValueNames.Length > 0)
            {
                writer.WriteLine($"case {string.Join(" ", singleValueNames)}");
                writer.Indent++;
                writer.WriteLine("set i (math $i + 1)");
                writer.Indent--;
            }

            // Multi-value options (arity > 1): skip up to N tokens, stopping at option-like tokens.
            // Group by arity so options with the same max can share a case branch.
            var multiValueByArity = valueOptions
                .Where(o => o.Arity.MaximumNumberOfValues > 1)
                .GroupBy(o => o.Arity.MaximumNumberOfValues);
            foreach (var group in multiValueByArity)
            {
                var names = string.Join(" ", group.SelectMany(o => o.Names()));
                writer.WriteLine($"case {names}");
                writer.Indent++;
                WriteMultiValueSkipLoop(writer, group.Key);
                writer.Indent--;
            }

            writer.Indent--;
            writer.WriteLine("end");
            writer.Indent--;
        }

        writer.Indent--;
        writer.WriteLine("end");

        writer.WriteLine("set i (math $i + 1)");
        writer.Indent--;
        writer.WriteLine("end");
    }

    /// <summary>
    /// Emit a fish loop that skips value tokens after a multi-value option.
    /// Stops when it has consumed maxValues tokens, or encounters a token starting with '-',
    /// whichever comes first. For unbounded arities, only the '-' check applies.
    /// </summary>
    private static void WriteMultiValueSkipLoop(IndentedTextWriter writer, int maxValues)
    {
        bool isBounded = maxValues < UnboundedArityThreshold;

        if (isBounded)
        {
            writer.WriteLine($"set -l skip_max {maxValues}");
            writer.WriteLine("set -l skipped 0");
            writer.WriteLine("while test $skipped -lt $skip_max -a (math $i + 1) -le (count $tokens)");
        }
        else
        {
            writer.WriteLine("while test (math $i + 1) -le (count $tokens)");
        }

        writer.Indent++;
        writer.WriteLine("set -l next $tokens[(math $i + 1)]");
        writer.WriteLine("if string match -q -- '-*' $next");
        writer.Indent++;
        writer.WriteLine("break");
        writer.Indent--;
        writer.WriteLine("end");
        writer.WriteLine("set i (math $i + 1)");
        if (isBounded)
        {
            writer.WriteLine("set skipped (math $skipped + 1)");
        }
        writer.Indent--;
        writer.WriteLine("end");
    }

    /// <summary>
    /// Generate option value completions.
    /// Scans backward through completed tokens to find the nearest option, then checks whether
    /// we're still within that option's arity. This correctly handles both single-value and
    /// multi-value options (e.g. <c>--sources foo bar</c> with arity 3 still offers completions
    /// for the third value).
    /// </summary>
    private static void WriteOptionValueCompletions(IndentedTextWriter writer, List<(int id, Command cmd)> states)
    {
        var hasAnyValueOptions = states.Any(s =>
            s.cmd.HierarchicalOptions().Any(o => !o.Hidden && !o.IsFlag()));

        if (!hasAnyValueOptions) return;

        // Scan backward through tokens to find the nearest option (token starting with -)
        writer.WriteLine("set -l opt_index 0");
        writer.WriteLine("if test (count $tokens) -ge 2");
        writer.Indent++;
        writer.WriteLine("for j in (seq (count $tokens) -1 2)");
        writer.Indent++;
        writer.WriteLine("if string match -q -- '-*' $tokens[$j]");
        writer.Indent++;
        writer.WriteLine("set opt_index $j");
        writer.WriteLine("break");
        writer.Indent--;
        writer.WriteLine("end");
        writer.Indent--;
        writer.WriteLine("end");
        writer.Indent--;
        writer.WriteLine("end");
        writer.WriteLine();

        writer.WriteLine("if test $opt_index -gt 0");
        writer.Indent++;
        writer.WriteLine("set -l opt $tokens[$opt_index]");
        // values_after = number of non-option tokens between the option and the cursor
        writer.WriteLine("set -l values_after (math (count $tokens) - $opt_index)");
        writer.WriteLine("switch $state");
        writer.Indent++;

        foreach (var (stateId, cmd) in states)
        {
            var valueOptions = cmd.HierarchicalOptions()
                .Where(o => !o.Hidden && !o.IsFlag())
                .ToArray();

            if (valueOptions.Length == 0)
                continue;

            writer.WriteLine($"case {stateId}");
            writer.Indent++;
            writer.WriteLine("switch $opt");
            writer.Indent++;

            foreach (var option in valueOptions)
            {
                var names = string.Join(" ", option.Names());
                var maxValues = option.Arity.MaximumNumberOfValues;
                bool isBounded = maxValues < UnboundedArityThreshold;

                writer.WriteLine($"case {names}");
                writer.Indent++;

                // For bounded options, check that we haven't exceeded the arity.
                // For unbounded options, always offer completions (any number of values is valid).
                if (isBounded)
                {
                    writer.WriteLine($"if test $values_after -lt {maxValues}");
                    writer.Indent++;
                }

                if (option.IsDynamic)
                {
                    writer.WriteLine(GenerateDynamicCall());
                }
                else
                {
                    var completions = option.GetCompletions(CompletionContext.Empty).ToArray();
                    foreach (var c in completions)
                    {
                        WriteCompletionCandidate(writer, c);
                    }
                }
                writer.WriteLine("return");

                if (isBounded)
                {
                    writer.Indent--;
                    writer.WriteLine("end");
                }

                writer.Indent--;
            }

            writer.Indent--;
            writer.WriteLine("end");
            writer.Indent--;
        }

        writer.Indent--;
        writer.WriteLine("end");
        writer.Indent--;
        writer.WriteLine("end");
        writer.WriteLine();
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

            // Subcommand completions
            foreach (var sub in cmd.Subcommands.Where(c => !c.Hidden))
            {
                WriteCandidate(writer, sub.Name, SanitizeDescription(sub.Description));
            }

            // Option completions - emit all aliases so both -h and --help are completable
            foreach (var option in cmd.HierarchicalOptions().Where(o => !o.Hidden))
            {
                var desc = SanitizeDescription(option.Description);
                foreach (var name in option.Names())
                {
                    WriteCandidate(writer, name, desc);
                }
            }

            // Positional argument completions
            foreach (var arg in cmd.Arguments.Where(a => !a.Hidden))
            {
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
    private static void WriteCompletionCandidate(IndentedTextWriter writer, CompletionItem completion)
    {
        var label = completion.InsertText ?? completion.Label;
        var desc = completion.Documentation ?? completion.Detail;
        WriteCandidate(writer, label, desc);
    }

    private static void WriteCandidate(IndentedTextWriter writer, string label, string? description)
    {
        if (!string.IsNullOrEmpty(description))
            writer.WriteLine($"printf '%s\\t%s\\n' {FishEscape(label)} {FishEscape(description)}");
        else
            writer.WriteLine($"printf '%s\\n' {FishEscape(label)}");
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
