// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

#nullable enable

using System.Diagnostics;
using System.Text;
using System.Text.Json;
using System.CommandLine.Completions;
using System.CommandLine.StaticCompletions.Shells;

namespace System.CommandLine.StaticCompletions.Tests;

public class ShellExecutionTests
{
    private const char RecordSeparator = '\u001e';

    [Fact]
    public async Task Bash_preserves_static_and_dynamic_candidates_without_evaluation()
    {
        if (OperatingSystem.IsWindows() || FindExecutable("bash") is not string bash)
        {
            return;
        }

        using var fixture = new CompletionExecutable();
        var command = CreateCommand(fixture.SideEffectPath);
        var generatedScript = new BashShellProvider().GenerateCompletions(command);
        var script = $$"""
            {{generatedScript}}
            COMP_WORDS=(completion-test-command "")
            COMP_CWORD=1
            COMP_LINE='completion-test-command '
            COMP_POINT=${#COMP_LINE}
            _completion_test_command
            printf '%s\x1e' "${COMPREPLY[@]}"
            """;

        var result = await RunShell(bash, ["--noprofile", "--norc", "-c", script], fixture.DirectoryPath);

        result.ExitCode.Should().Be(0, result.StandardError);
        ParseRecords(result.StandardOutput).Should().BeEquivalentTo(
            ExpectedStaticCandidates(fixture.SideEffectPath).Concat(ExpectedDynamicCandidates(fixture.SideEffectPath)));
        File.Exists(fixture.SideEffectPath).Should().BeFalse();
    }

    [Fact]
    public async Task Zsh_preserves_dynamic_candidates_without_evaluation()
    {
        if (OperatingSystem.IsWindows() || FindExecutable("zsh") is not string zsh)
        {
            return;
        }

        using var fixture = new CompletionExecutable();
        var script = $$"""
            result=$(completion-test-command)
            completions=()
            for line in ${(f)result}; do
                completions+=("$line")
            done
            printf '%s\x1e' "${completions[@]}"
            """;

        var result = await RunShell(zsh, ["-f", "-c", script], fixture.DirectoryPath);

        result.ExitCode.Should().Be(0, result.StandardError);
        ParseRecords(result.StandardOutput).Should().BeEquivalentTo(
            ExpectedDynamicCandidates(fixture.SideEffectPath),
            options => options.WithStrictOrdering());
        File.Exists(fixture.SideEffectPath).Should().BeFalse();
    }

    [Fact]
    public async Task Fish_preserves_static_and_dynamic_candidates_without_evaluation()
    {
        if (OperatingSystem.IsWindows() || FindExecutable("fish") is not string fish)
        {
            return;
        }

        using var fixture = new CompletionExecutable();
        var generatedScript = new FishShellProvider().GenerateCompletions(CreateCommand(fixture.SideEffectPath));
        var script = $$"""
            {{generatedScript}}
            complete -C 'completion-test-command ' |
                while read -l candidate
                    printf '%s\x1e' "$candidate"
                end
            """;

        var result = await RunShell(fish, ["--no-config", "-c", script], fixture.DirectoryPath);

        result.ExitCode.Should().Be(0, result.StandardError);
        ParseRecords(result.StandardOutput).Should().BeEquivalentTo(
            ExpectedStaticCandidates(fixture.SideEffectPath)
                .Concat(ExpectedDynamicCandidates(fixture.SideEffectPath))
                .Distinct());
        File.Exists(fixture.SideEffectPath).Should().BeFalse();
    }

    [Fact]
    public async Task PowerShell_preserves_candidates_and_treats_wildcard_prefixes_literally()
    {
        if (OperatingSystem.IsWindows() || FindExecutable("pwsh") is not string pwsh)
        {
            return;
        }

        using var fixture = new CompletionExecutable();
        var generatedScript = new PowerShellShellProvider().GenerateCompletions(CreateCommand(fixture.SideEffectPath));
        var cursorPosition = "completion-test-command ".Length;
        var script = $$"""
            {{generatedScript}}
            $completion = [System.Management.Automation.CommandCompletion]::CompleteInput(
                'completion-test-command ',
                {{cursorPosition}},
                $null)
            foreach ($match in $completion.CompletionMatches) {
                [Console]::Out.Write($match.CompletionText)
                [Console]::Out.Write([char]0x1e)
            }
            $literalPrefixMatches = [System.Management.Automation.CommandCompletion]::CompleteInput(
                'completion-test-command [',
                {{cursorPosition + 1}},
                $null).CompletionMatches
            if ($literalPrefixMatches.Count -ne 1 -or
                $literalPrefixMatches[0].CompletionText -ne '[literal') {
                throw 'PowerShell did not treat the completion prefix as literal text.'
            }
            """;

        var result = await RunShell(
            pwsh,
            ["-NoProfile", "-NonInteractive", "-Command", script],
            fixture.DirectoryPath);

        result.ExitCode.Should().Be(0, result.StandardError);
        ParseRecords(result.StandardOutput).Should().BeEquivalentTo(
            ExpectedStaticCandidates(fixture.SideEffectPath).Concat(ExpectedDynamicCandidates(fixture.SideEffectPath)));
        File.Exists(fixture.SideEffectPath).Should().BeFalse();
    }

    [Fact]
    public async Task Nushell_preserves_dynamic_candidates_without_evaluation()
    {
        if (OperatingSystem.IsWindows() || FindExecutable("nu") is not string nu)
        {
            return;
        }

        using var fixture = new CompletionExecutable();
        var generatedScript = new NushellShellProvider().GenerateCompletions(CreateCommand(fixture.SideEffectPath));
        var script = $$"""
            {{generatedScript}}
            nu-complete completion-test-command "" | to json --raw
            """;

        var result = await RunShell(nu, ["--no-config-file", "-c", script], fixture.DirectoryPath);

        result.ExitCode.Should().Be(0, result.StandardError);
        JsonSerializer.Deserialize<string[]>(result.StandardOutput).Should().BeEquivalentTo(
            ExpectedDynamicCandidates(fixture.SideEffectPath),
            options => options.WithStrictOrdering());
        File.Exists(fixture.SideEffectPath).Should().BeFalse();
    }

    private static Command CreateCommand(string sideEffectPath)
    {
        var staticArgument = new Argument<string>("static");
        staticArgument.CompletionSources.Add(_ =>
            ExpectedStaticCandidates(sideEffectPath).Select(value => new CompletionItem(value)));

        return new("completion-test-command")
        {
            staticArgument,
            new Argument<string>("dynamic") { IsDynamic = true }
        };
    }

    private static string[] ExpectedStaticCandidates(string sideEffectPath) =>
    [
        "static space",
        "static 'quote'",
        $"$(touch {sideEffectPath})",
        @"static*?[abc]\value"
    ];

    private static IEnumerable<string> ExpectedDynamicCandidates(string sideEffectPath) =>
        CompletionExecutable.DynamicCandidates.Append($"$(touch {sideEffectPath})");

    private static string[] ParseRecords(string output) =>
        output.Split(RecordSeparator, StringSplitOptions.RemoveEmptyEntries);

    private static async Task<ShellResult> RunShell(
        string executable,
        IEnumerable<string> arguments,
        string fixtureDirectory)
    {
        var startInfo = new ProcessStartInfo
        {
            FileName = executable,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false
        };
        foreach (var argument in arguments)
        {
            startInfo.ArgumentList.Add(argument);
        }
        startInfo.Environment["PATH"] = fixtureDirectory + Path.PathSeparator + startInfo.Environment["PATH"];

        using var process = Process.Start(startInfo)!;
        var standardOutput = process.StandardOutput.ReadToEndAsync();
        var standardError = process.StandardError.ReadToEndAsync();
        await process.WaitForExitAsync();
        return new(process.ExitCode, await standardOutput, await standardError);
    }

    private static string? FindExecutable(string name)
    {
        foreach (var directory in (Environment.GetEnvironmentVariable("PATH") ?? "").Split(Path.PathSeparator))
        {
            var path = Path.Combine(directory, name);
            if (File.Exists(path))
            {
                return path;
            }
        }
        return null;
    }

    private sealed class CompletionExecutable : IDisposable
    {
        internal static readonly string[] DynamicCandidates =
        [
            "dynamic space",
            "dynamic 'quote'",
            @"dynamic*?[abc]\value",
            "[literal"
        ];

        internal CompletionExecutable()
        {
            if (OperatingSystem.IsWindows())
            {
                throw new PlatformNotSupportedException();
            }

            DirectoryPath = Path.Combine(Path.GetTempPath(), $"system-commandline-completions-{Guid.NewGuid():N}");
            Directory.CreateDirectory(DirectoryPath);
            SideEffectPath = Path.Combine(DirectoryPath, "side-effect");

            var executablePath = Path.Combine(DirectoryPath, "completion-test-command");
            var content = new StringBuilder("#!/bin/sh\n");
            foreach (var candidate in ExpectedDynamicCandidates(SideEffectPath))
            {
                content.Append("printf '%s\\n' '")
                    .Append(candidate.Replace("'", "'\\''"))
                    .Append("'\n");
            }
            File.WriteAllText(executablePath, content.ToString());
            File.SetUnixFileMode(
                executablePath,
                UnixFileMode.UserRead | UnixFileMode.UserWrite | UnixFileMode.UserExecute);
        }

        internal string DirectoryPath { get; }

        internal string SideEffectPath { get; }

        public void Dispose() => Directory.Delete(DirectoryPath, recursive: true);
    }

    private sealed record ShellResult(int ExitCode, string StandardOutput, string StandardError);
}
