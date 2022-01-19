// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if NET5_0_OR_GREATER

using System.CommandLine.Suggest;
using System.CommandLine.Tests.Utility;
using System.IO;
using System.Text;
using FluentAssertions;
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

    [ReleaseBuildOnlyFact]
    public void App_referencing_system_commandline_can_be_trimmed()
    {
        var stdOut = new StringBuilder();
        var stdErr = new StringBuilder();
        
        // FIX: (App_referencing_system_commandline_can_be_trimmed) trim warnings seem to require a project reference

        var exitCode = Process.RunToCompletion(
            DotnetMuxer.Path.FullName,
            $"publish -c Release -r win-x64 --self-contained /p:PublishTrimmed=true /p:SystemCommandLineDllPath=\"{_systemCommandLineDllPath}\" /p:TreatWarningsAsErrors=true -t:rebuild",
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
            workingDirectory: Path.Combine(Directory.GetCurrentDirectory(), "TrimmingTestApp"));

        stdOut.ToString().Should().NotContain("IL2104");
        exitCode.Should().Be(0);
        stdErr.ToString().Should().BeEmpty();
    }
}

#endif