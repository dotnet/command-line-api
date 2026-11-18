// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

#nullable disable

namespace System.CommandLine.StaticCompletions.Tests;

using System.CommandLine.StaticCompletions.Shells;
using System.Diagnostics;
using System.Text.Json;

public class PowershellProviderTests(ITestOutputHelper log)
{
    private IShellProvider provider = new PowerShellShellProvider();

    [Fact]
    public async Task GenericCompletions()
    {
        await provider.Verify(new("mycommand"), log);
    }

    [Fact]
    public async Task SimpleOptionCompletion()
    {
        await provider.Verify(new("mycommand") {
            new Option<string>("--name")
        }, log);
    }

    [Fact]
    public async Task SubcommandAndOptionInTopLevelList()
    {
        await provider.Verify(new("mycommand") {
                new Option<string>("--name"),
                new Command("subcommand")
            }, log);
    }

    [Fact]
    public async Task NestedSubcommandCompletion()
    {
        await provider.Verify(new("mycommand") {
            new Command("subcommand") {
                new Command("nested")
            }
        }, log);
    }

    [Fact]
    public async Task DynamicCompletionsGeneration()
    {
        var dynamicArg = new Argument<string>("target") { IsDynamic = true };
        await provider.Verify(new("mycommand") { dynamicArg }, log);
    }

    [Fact]
    public async Task TrackingContext()
    {
        await provider.Verify(TrackingCommand(), log);
    }

    private static Command TrackingCommand()
    {
        var child = new Command("child", "Child command")
        {
            new Option<bool>("--child-only"),
            new Argument<string>("first"),
            new Argument<string[]>("rest"),
            new Command("nested") { new Option<bool>("--nested-only") }
        };
        child.Aliases.Add("c");
        return new Command("scltest")
        {
            new Option<bool>("--switch", "-s"),
            new Option<bool>("-a"),
            new Option<bool>("-A"),
            new Option<string>("--name", "-n", "/name") { Recursive = true },
            new Option<string>("--optional") { Arity = ArgumentArity.ZeroOrOne },
            new Option<string[]>("--values") { Arity = new(1, 3), AllowMultipleArgumentsPerToken = true },
            new Option<string[]>("--single") { Arity = new(0, 3) },
            new Option<string[]>("--many") { Arity = ArgumentArity.OneOrMore, AllowMultipleArgumentsPerToken = true },
            new Option<string>("--secret") { Hidden = true },
            child
        };
    }

    [PowerShellTheory]
    [InlineData("--switch child ", 1, null, 0, 0, false)]
    [InlineData("-s child ", 1, null, 0, 0, false)]
    [InlineData("--switch false c ", 1, null, 0, 0, false)]
    [InlineData("--switch child nested ", 2, null, 0, 0, false)]
    [InlineData("--name value child ", 1, null, 0, 0, false)]
    [InlineData("--name child ", 0, null, 0, 0, false)]
    [InlineData("--name --switch child ", 1, null, 0, 0, false)]
    [InlineData("--name=child child ", 1, null, 0, 0, false)]
    [InlineData("--name:child c ", 1, null, 0, 0, false)]
    [InlineData("--name= child ", 0, null, 0, 0, false)]
    [InlineData("/name child ", 0, null, 0, 0, false)]
    [InlineData("/name:value child ", 1, null, 0, 0, false)]
    [InlineData("--optional child ", 1, null, 0, 0, false)]
    [InlineData("--optional value child ", 1, null, 0, 0, false)]
    [InlineData("--values first child ", 1, null, 0, 0, false)]
    [InlineData("--values child ", 0, "--values", 0, 0, false)]
    [InlineData("--values one two three child ", 1, null, 0, 0, false)]
    [InlineData("--values=a b child ", 1, null, 0, 0, false)]
    [InlineData("--values a -unknown ", 0, "--values", 0, 0, false)]
    [InlineData("--single value child ", 1, null, 0, 0, false)]
    [InlineData("--many one two three child ", 1, null, 0, 0, false)]
    [InlineData("--secret child ", 0, null, 0, 0, false)]
    [InlineData("--secret value child ", 1, null, 0, 0, false)]
    [InlineData("-as child ", 1, null, 0, 0, false)]
    [InlineData("-anvalue child ", 1, null, 0, 0, false)]
    [InlineData("child --name value nested ", 2, null, 0, 0, false)]
    [InlineData("--SWITCH child ", 1, null, 0, 0, false)]
    [InlineData("-- child ", 0, null, 0, 0, true)]
    [InlineData("--name -- child ", 0, null, 0, 0, true)]
    [InlineData("child -- nested ", 1, null, 1, 0, true)]
    [InlineData("child first second third ", 1, null, 1, 2, false)]
    [InlineData("child 'first value' ", 1, null, 1, 0, false)]
    [InlineData("--name 'two words' child ", 1, null, 0, 0, false)]
    [InlineData("--name child child ", 1, null, 0, 0, false)]
    [InlineData("child first --name value second ", 1, null, 1, 1, false)]
    [InlineData("child --optional nested ", 2, null, 0, 0, false)]
    public async Task Tracks_parser_context(string input, int state, string pendingOption, int argumentIndex, int argumentValueCount, bool endOfOptions)
    {
        var command = TrackingCommand();
        command.Parse("scltest " + input).CommandResult.Command.Name.Should().Be(state switch { 0 => "scltest", 1 => "child", _ => "nested" });
        var result = await ReadContext(command, "scltest " + input);
        result.GetProperty("State").GetInt32().Should().Be(state);
        result.GetProperty("Option").GetString().Should().Be(pendingOption);
        result.GetProperty("ArgumentIndex").GetInt32().Should().Be(argumentIndex);
        result.GetProperty("ArgumentValueCount").GetInt32().Should().Be(argumentValueCount);
        result.GetProperty("EndOfOptions").GetBoolean().Should().Be(endOfOptions);
    }

    [PowerShellTheory]
    [InlineData("--name ", "--name", 0)]
    [InlineData("--values a ", "--values", 1)]
    [InlineData("--values a b ", "--values", 2)]
    [InlineData("--values one two three ", null, 3)]
    [InlineData("--single a ", null, 1)]
    [InlineData("--switch ", "--switch", 0)]
    [InlineData("--switch true ", null, 1)]
    [InlineData("--switch not-a-bool ", null, 0)]
    public async Task Tracks_option_value_arity(string input, string option, int count)
    {
        var result = await ReadContext(TrackingCommand(), "scltest " + input);
        result.GetProperty("Option").GetString().Should().Be(option);
        result.GetProperty("OptionValueCount").GetInt32().Should().Be(count);
    }

    [PowerShellTheory]
    [InlineData("scltest child --na|me=value nested", 1, null, "", "--na")]
    [InlineData("scltest --name=va|lue child", 0, "--name", "--name=", "va")]
    [InlineData("scltest child |nested", 1, null, "", "")]
    [InlineData("scltest child first fi|rst", 1, null, "", "fi")]
    public async Task Tracks_cursor_instead_of_matching_word_text(string input, int state, string option, string prefix, string value)
    {
        int cursor = input.IndexOf('|');
        input = input.Remove(cursor, 1);
        var result = await ReadContext(TrackingCommand(), input, cursor);
        result.GetProperty("State").GetInt32().Should().Be(state);
        result.GetProperty("CompletionOption").GetString().Should().Be(option);
        result.GetProperty("ValuePrefix").GetString().Should().Be(prefix);
        result.GetProperty("ValueToComplete").GetString().Should().Be(value);
    }

    [PowerShellTheory]
    [InlineData("scltest --name value child --ch", "--child-only")]
    [InlineData("scltest -s c --ch", "--child-only")]
    [InlineData("scltest child --name value nested --ne", "--nested-only")]
    [InlineData("scltest --name 'value with spaces' child --ch", "--child-only")]
    [InlineData("scltest 'child' --ch", "--child-only")]
    [InlineData("scltest child --ch|ild-only", "--child-only")]
    [InlineData("scltest --switch child --ch| --name later", "--child-only")]
    public async Task Completes_subcommands_after_options(string input, string expected)
    {
        var script = provider.GenerateCompletions(TrackingCommand());
        int cursor = input.IndexOf('|');
        if (cursor >= 0)
        {
            input = input.Remove(cursor, 1);
        }
        else
        {
            cursor = input.Length;
        }
        var output = await RunPowerShell(script + $"""

$result = [CommandCompletion]::CompleteInput('{input.Replace("'", "''")}', {cursor}, $null)
$result.CompletionMatches.CompletionText | ConvertTo-Json -Compress -AsArray
""");
        JsonSerializer.Deserialize<string[]>(output).Should().Contain(expected);
    }

    [PowerShellTheory]
    [InlineData("scltest --switch ", 17)]
    [InlineData("$null = 1; scltest --switch ", 17)]
    public async Task Preserves_static_and_dynamic_argument_completions(string input, int suggestPosition)
    {
        var argument = new Argument<string>("value") { IsDynamic = true };
        var staticArgument = new Argument<string>("static-value");
        staticArgument.CompletionSources.Add(_ => ["static-value"]);
        var command = new Command("scltest") { new Option<bool>("--switch"), argument, staticArgument };
        string script = provider.GenerateCompletions(command).Replace(
            "Register-ArgumentCompleter -Native -CommandName 'scltest' -ScriptBlock {", "$completer = {");
        var output = await RunPowerShell(script + $$"""

function scltest($directive, $text) {
    if ($directive -ne '[suggest:{{suggestPosition}}]' -or $text -ne 'scltest --switch') { throw "Unexpected suggestion request: $directive $text" }
    'runtime-value'
}
$ast = [Parser]::ParseInput('{{input}}', [ref]$null, [ref]$null)
$commandAst = $ast.EndBlock.Statements[-1].PipelineElements[0]
@(& $completer '' $commandAst {{input.Length}}).CompletionText | ConvertTo-Json -Compress -AsArray
""");
        JsonSerializer.Deserialize<string[]>(output).Should().Contain(["static-value", "runtime-value"]);
    }

    [PowerShellTheory]
    [InlineData("scltest first child ", true)]
    [InlineData("scltest first -- child ", true)]
    public async Task Capture_remaining_arguments_do_not_change_command(string input, bool capturing)
    {
        var command = new Command("scltest")
        {
            new Argument<string>("first"),
            new Argument<string[]>("rest") { CaptureRemainingTokens = true },
            new Command("child")
        };
        var result = await ReadContext(command, input);
        result.GetProperty("State").GetInt32().Should().Be(0);
        result.GetProperty("Capturing").GetBoolean().Should().Be(capturing);
        result.GetProperty("ArgumentValueCount").GetInt32().Should().Be(1);
    }

    private async Task<JsonElement> ReadContext(Command command, string input, int? cursor = null)
    {
        var script = provider.GenerateCompletions(command).Replace("$completions = @()", """
@{
    State = $state
    Option = $pendingOption.Name
    OptionValueCount = $optionValueCount
    ArgumentIndex = $argumentIndex
    ArgumentValueCount = $argumentValueCount
    EndOfOptions = $endOfOptions
    Capturing = $capturing
    CompletionOption = $completionOption.Name
    ValuePrefix = $valuePrefix
    ValueToComplete = $valueToComplete
} | ConvertTo-Json -Compress
return
""");
        string output = await RunPowerShell(script + $"""

$result = [CommandCompletion]::CompleteInput('{input.Replace("'", "''")}', {cursor ?? input.Length}, $null)
$result.CompletionMatches.CompletionText
""");
        return JsonDocument.Parse(output).RootElement.Clone();
    }

    private static async Task<string> RunPowerShell(string script)
    {
        var startInfo = new ProcessStartInfo("pwsh")
        {
            RedirectStandardInput = true,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false
        };
        startInfo.ArgumentList.Add("-NoLogo");
        startInfo.ArgumentList.Add("-NoProfile");
        startInfo.ArgumentList.Add("-NonInteractive");
        startInfo.ArgumentList.Add("-Command");
        startInfo.ArgumentList.Add("$ErrorActionPreference = 'Stop'; & ([scriptblock]::Create([Console]::In.ReadToEnd()))");
        using var process = Process.Start(startInfo);
        var output = process.StandardOutput.ReadToEndAsync();
        var error = process.StandardError.ReadToEndAsync();
        await process.StandardInput.WriteAsync(script);
        process.StandardInput.Close();
        try
        {
            await process.WaitForExitAsync().WaitAsync(TimeSpan.FromSeconds(30));
        }
        finally
        {
            if (!process.HasExited)
            {
                process.Kill(entireProcessTree: true);
            }
        }
        process.ExitCode.Should().Be(0, await error);
        return await output;
    }
}

internal sealed class PowerShellTheoryAttribute : TheoryAttribute
{
    public PowerShellTheoryAttribute()
    {
        string executable = OperatingSystem.IsWindows() ? "pwsh.exe" : "pwsh";
        if (!(Environment.GetEnvironmentVariable("PATH") ?? "").Split(Path.PathSeparator).Any(directory => File.Exists(Path.Combine(directory, executable))))
        {
            Skip = "PowerShell Core is not installed.";
        }
    }
}
