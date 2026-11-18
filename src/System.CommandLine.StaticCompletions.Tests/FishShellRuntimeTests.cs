// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Diagnostics;
using System.CommandLine.StaticCompletions.Shells;

namespace System.CommandLine.StaticCompletions.Tests;

public class FishShellRuntimeTests
{
    [FishTheory]
    [InlineData("mycommand ", "slot-root-first")]
    [InlineData("mycommand one ", "slot-root-second")]
    [InlineData("mycommand one two ", "")]
    [InlineData("mycommand send ", "slot-first")]
    [InlineData("mycommand s one ", "slot-first")]
    [InlineData("mycommand send one two ", "slot-second")]
    [InlineData("mycommand one send ", "slot-first")]
    [InlineData("mycommand s nested --config one ", "slot-nested")]
    [InlineData("mycommand send one nested ", "slot-nested")]
    [InlineData("mycommand --config send ", "slot-root-first")]
    [InlineData("mycommand /config send ", "slot-root-first")]
    [InlineData("mycommand -c send ", "slot-root-first")]
    [InlineData("mycommand --config value-config send ", "slot-first")]
    [InlineData("mycommand send --config ", "value-config")]
    [InlineData("mycommand send /config ", "value-config")]
    [InlineData("mycommand send --config=-x ", "slot-first")]
    [InlineData("mycommand send --config \"two words\" one two ", "slot-second")]
    [InlineData("mycommand send \"two words\" ", "slot-first")]
    [InlineData("mycommand send /config:value-config one two ", "slot-second")]
    [InlineData("mycommand send --config= ", "value-config")]
    [InlineData("mycommand send --many a ", "value-many")]
    [InlineData("mycommand send --many a b ", "value-many")]
    [InlineData("mycommand send --many a b c ", "slot-first")]
    [InlineData("mycommand send --many=a b c ", "slot-first")]
    [InlineData("mycommand send --many a b c --many d e f one two ", "slot-second")]
    [InlineData("mycommand send --many -unknown -9 ", "value-many")]
    [InlineData("mycommand send --many a --config x ", "slot-first")]
    [InlineData("mycommand send --single-many a ", "slot-first")]
    [InlineData("mycommand --all a send ", "slot-first")]
    [InlineData("mycommand send --all a b -unknown ", "value-all")]
    [InlineData("mycommand send --all a /config x ", "slot-first")]
    [InlineData("mycommand --local send ", "slot-root-first")]
    [InlineData("mycommand send --local one ", "slot-second")]
    [InlineData("mycommand send --secret value one two ", "slot-second")]
    [InlineData("mycommand send --secret ", "")]
    [InlineData("mycommand send --optional send ", "value-optional")]
    [InlineData("mycommand --optional send ", "slot-first")]
    [InlineData("mycommand --optional /config x send ", "slot-first")]
    [InlineData("mycommand --flag send ", "slot-first")]
    [InlineData("mycommand send --flag one two ", "slot-second")]
    [InlineData("mycommand send --flag true one two ", "slot-second")]
    [InlineData("mycommand send --flag false one two ", "slot-second")]
    [InlineData("mycommand send --flag=not-bool one ", "slot-second")]
    [InlineData("mycommand send --zero=value one ", "slot-second")]
    [InlineData("mycommand send -zcvalue one two ", "slot-second")]
    [InlineData("mycommand send -zf true one two ", "slot-second")]
    [InlineData("mycommand --config -zf send ", "slot-first")]
    [InlineData("mycommand send -zsend ", "slot-first")]
    [InlineData("mycommand send -- --config ", "slot-first")]
    [InlineData("mycommand -- send ", "slot-root-second")]
    [InlineData("mycommand send --many a -- one two ", "slot-second")]
    [InlineData("mycommand hidden ", "")]
    [InlineData("mycommand hidden secret ", "slot-visible")]
    [InlineData("mycommand optional ", "slot-optional")]
    [InlineData("mycommand optional one ", "slot-rest")]
    [InlineData("mycommand optional one two three ", "")]
    [InlineData("mycommand capture ", "slot-head")]
    [InlineData("mycommand capture head ", "slot-capture")]
    [InlineData("mycommand capture head --config ", "slot-capture")]
    [InlineData("mycommand capture head -- send ", "slot-capture")]
    [InlineData("mycommand capture head a b c ", "slot-tail")]
    [InlineData("mycommand capture head --config=x y ", "slot-tail")]
    [InlineData("mycommand capture-first ", "slot-capture-first")]
    [InlineData("mycommand capture-first --config x ", "slot-capture-first")]
    [InlineData("mycommand capture-first one --config ", "slot-tail")]
    [InlineData("mycommand unlimited a b c d ", "slot-unlimited")]
    [InlineData("mycommand dynamic ", "slot-dynamic")]
    [InlineData("mycommand dynamic done ", "slot-after-dynamic")]
    [InlineData("mycommand send --runtime ", "slot-dynamic")]
    public async Task CompletesOnlyTheOwningSlot(string commandLine, string expected)
    {
        var command = CreateCommand();
        var completions = await Complete(command, commandLine);
        Assert.Equal(expected.Length == 0 ? [] : [expected],
            completions.Where(c => c.StartsWith("slot-", StringComparison.Ordinal) ||
                                   c.StartsWith("value-", StringComparison.Ordinal)));
        if (expected.StartsWith("slot-", StringComparison.Ordinal) &&
            !commandLine.Contains("--runtime", StringComparison.Ordinal))
        {
            var parsed = command.Parse(commandLine);
            var owner = parsed.CommandResult.Command.Arguments.FirstOrDefault(
                argument => (parsed.GetResult(argument)?.Tokens.Count ?? 0) < argument.Arity.MaximumNumberOfValues);
            Assert.Equal(expected, $"slot-{owner?.Name}");
        }
    }

    [FishTheory]
    [InlineData("mycommand send --config=v", "--config=value-config")]
    [InlineData("mycommand send /config:v", "/config:value-config")]
    [InlineData("mycommand send --runtime=slot", "--runtime=slot-dynamic")]
    [InlineData("mycommand send -zcv", "-zcvalue-config")]
    [InlineData("mycommand send -zc=v", "-zc=value-config")]
    public async Task CompletesInlineValues(string commandLine, string expected)
    {
        Assert.Contains(expected, await Complete(CreateCommand(), commandLine));
    }

    [FishTheory]
    [InlineData("mycommand send -- ", false)]
    [InlineData("mycommand capture head ", false)]
    [InlineData("mycommand send ", true)]
    [InlineData("mycommand capture-first ", true)]
    [InlineData("mycommand capture-first --config x ", true)]
    [InlineData("mycommand capture-first one ", false)]
    public async Task OffersOptionsOnlyBeforeEndOfOptionsOrCapture(string commandLine, bool expected)
    {
        Assert.Equal(expected, (await Complete(CreateCommand(), commandLine)).Contains("--config"));
    }

    [FishTheory]
    [InlineData("mycommand send --flag t", "True")]
    [InlineData("mycommand send --flag=f", "--flag=False")]
    public async Task PreservesBooleanValueCompletions(string commandLine, string expected)
    {
        Assert.Contains(expected, await Complete(CreateCommand(), commandLine));
    }

    internal static Command CreateCommand()
    {
        var command = new Command("mycommand")
        {
            Argument("root-first"),
            Argument("root-second"),
            new Command("send")
            {
                Argument("first", new ArgumentArity(0, 2)),
                Argument("second"),
                new Command("nested") { Argument("nested") }
            },
            new Command("hidden")
            {
                new Argument<string>("secret") { Hidden = true },
                Argument("visible")
            },
            new Command("optional")
            {
                Argument("zero", ArgumentArity.Zero),
                Argument("optional", ArgumentArity.ZeroOrOne),
                Argument("rest", new ArgumentArity(0, 2))
            },
            new Command("capture")
            {
                Argument("head"),
                new Argument<string[]>("capture")
                {
                    Arity = new ArgumentArity(0, 3),
                    CaptureRemainingTokens = true,
                    CompletionSources = { _ => [new("slot-capture")] }
                },
                Argument("tail")
            },
            new Command("unlimited")
            {
                Argument("unlimited", ArgumentArity.ZeroOrMore),
                Argument("unreachable")
            },
            new Command("dynamic")
            {
                new Argument<string>("dynamic") { IsDynamic = true },
                Argument("after-dynamic")
            }
        };
        command.Subcommands[0].Aliases.Add("s");
        command.Options.Add(ValueOption("--config", ArgumentArity.ExactlyOne, false, "-c", "/config"));
        command.Options.Add(ValueOption("--many", new ArgumentArity(1, 3), true));
        command.Options.Add(ValueOption("--single-many", new ArgumentArity(1, 3), false));
        command.Options.Add(ValueOption("--optional", new ArgumentArity(0, 3), true));
        command.Options.Add(ValueOption("--all", ArgumentArity.OneOrMore, true));
        command.Options.Add(new Option<bool>("--flag", "-f") { Recursive = true });
        command.Options.Add(new Option<bool>("--zero", "-z") { Arity = ArgumentArity.Zero, Recursive = true });
        command.Options.Add(new Option<string>("--runtime") { Recursive = true, IsDynamic = true });
        command.Options.Add(new Option<string>("--local"));
        command.Options.Add(new Option<string>("--secret") { Recursive = true, Hidden = true });
        var firstCapture = Argument("capture-first", new ArgumentArity(0, 2));
        firstCapture.CaptureRemainingTokens = true;
        command.Subcommands.Add(new Command("capture-first") { firstCapture, Argument("tail") });
        return command;

        static Argument<string[]> Argument(string name, ArgumentArity? arity = null) =>
            new(name)
            {
                Arity = arity ?? ArgumentArity.ExactlyOne,
                CompletionSources = { _ => [new($"slot-{name}")] }
            };

        static Option<string[]> ValueOption(string name, ArgumentArity arity, bool multiple, params string[] aliases) =>
            new(name, aliases)
            {
                Arity = arity,
                AllowMultipleArgumentsPerToken = multiple,
                Recursive = true,
                CompletionSources = { _ => [new($"value-{name[2..]}")] }
            };
    }

    private static async Task<string[]> Complete(Command command, string commandLine)
    {
        var startInfo = new ProcessStartInfo
        {
            FileName = OperatingSystem.IsWindows() ? "wsl.exe" : "fish",
            RedirectStandardInput = true,
            RedirectStandardOutput = true,
            RedirectStandardError = true
        };
        if (OperatingSystem.IsWindows())
        {
            startInfo.ArgumentList.Add("--distribution");
            startInfo.ArgumentList.Add(Environment.GetEnvironmentVariable("FISH_TEST_WSL_DISTRO")!);
            startInfo.ArgumentList.Add("--exec");
            startInfo.ArgumentList.Add("fish");
        }
        startInfo.ArgumentList.Add("--no-config");
        using var process = Process.Start(startInfo)!;
        var stdout = process.StandardOutput.ReadToEndAsync();
        var stderr = process.StandardError.ReadToEndAsync();
        // Fish's noninteractive `complete -C` supplies tokens but not a cursor position.
        await process.StandardInput.WriteLineAsync($$"""
            function commandline
                if string match -q -- -C "$argv[1]"
                    echo {{commandLine.Length}}
                else
                    builtin commandline $argv
                end
            end
            """);
        await process.StandardInput.WriteLineAsync("""
            set -l app_dir (mktemp -d)
            printf '%s\n' '#!/bin/sh' 'printf "%s\t%s\n" slot-dynamic "Dynamic candidate"' > "$app_dir/mycommand"
            chmod +x "$app_dir/mycommand"
            set -gx PATH "$app_dir" $PATH
            """);
        await process.StandardInput.WriteLineAsync(new FishShellProvider().GenerateCompletions(command));
        var escaped = commandLine.Replace("\\", "\\\\").Replace("'", "\\'");
        await process.StandardInput.WriteLineAsync($"complete -C '{escaped}'");
        await process.StandardInput.WriteLineAsync("""
            rm "$app_dir/mycommand"
            rmdir "$app_dir"
            """);
        process.StandardInput.Close();
        using var timeout = new CancellationTokenSource(TimeSpan.FromSeconds(30));
        try
        {
            await process.WaitForExitAsync(timeout.Token);
        }
        finally
        {
            if (!process.HasExited)
                process.Kill(entireProcessTree: true);
        }
        Assert.Equal(0, process.ExitCode);
        Assert.Equal("", await stderr);
        return (await stdout).Split('\n', StringSplitOptions.RemoveEmptyEntries)
            .Select(line => line.Split('\t')[0]).ToArray();
    }

    private sealed class FishTheoryAttribute : TheoryAttribute
    {
        public FishTheoryAttribute()
        {
            if (OperatingSystem.IsWindows())
            {
                if (string.IsNullOrEmpty(Environment.GetEnvironmentVariable("FISH_TEST_WSL_DISTRO")))
                    Skip = "Set FISH_TEST_WSL_DISTRO to run Fish tests in a WSL distribution.";
            }
            else if (!(Environment.GetEnvironmentVariable("PATH") ?? "").Split(Path.PathSeparator)
                         .Any(directory => File.Exists(Path.Combine(directory, "fish"))))
            {
                Skip = "Fish is not installed.";
            }
        }
    }
}
