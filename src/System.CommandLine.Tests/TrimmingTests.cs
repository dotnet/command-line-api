// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if NET6_0_OR_GREATER

using System.CommandLine.Suggest;
using System.CommandLine.Tests.Utility;
using System.IO;
using System.Text;
using FluentAssertions;
using Xunit;
using Xunit.Abstractions;

namespace System.CommandLine.Tests;

public class TrimmingTests
{
    private readonly ITestOutputHelper _output;
    private readonly string _systemCommandLineDllPath;

    public TrimmingTests(ITestOutputHelper output)
    {
        _output = output;

        _systemCommandLineDllPath = typeof(Command).Assembly.Location;
    }

    [ReleaseBuildOnlyTheory]
    [InlineData("")]
    [InlineData("-p:PublishSingleFile=true")]
    public void App_referencing_system_commandline_can_be_trimmed(string additionalArgs)
    {
        var stdOut = new StringBuilder();
        var stdErr = new StringBuilder();

        var workingDirectory = Path.Combine(Directory.GetCurrentDirectory(), "TestApps", "Trimming");
        
        Process.RunToCompletion(
            DotnetMuxer.Path.FullName,
            "clean -c Release -r win-x64",
            workingDirectory: workingDirectory);

        var commandLine = string.Format(
            "publish -c Release -r win-x64 --self-contained -p:SystemCommandLineDllPath=\"{0}\" -p:TreatWarningsAsErrors=true -p:PublishTrimmed=true {1}",
            _systemCommandLineDllPath,
            additionalArgs);

        var exitCode = Process.RunToCompletion(
            DotnetMuxer.Path.FullName,
            commandLine,
            s =>
            {
                _output.WriteLine(s);
                stdOut.Append(s);
            },
            s =>
            {
                _output.WriteLine(s);
                stdErr.Append(s);
            },
            workingDirectory);

        stdOut.ToString().Should().NotContain("warning IL");
        stdErr.ToString().Should().BeEmpty();
        exitCode.Should().Be(0);
    }
}

#endif