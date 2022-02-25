﻿// Copyright (c) .NET Foundation and contributors. All rights reserved.
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

public class NativeAOTTests
{
    private readonly ITestOutputHelper _output;
    private readonly string _systemCommandLineDllPath;

    public NativeAOTTests(ITestOutputHelper output)
    {
        _output = output;

        _systemCommandLineDllPath = typeof(Command).Assembly.Location;
    }

    [ReleaseBuildOnlyTheory]
    [InlineData("-p:IlcOptimizationPreference=Speed")]
    [InlineData("-p:IlcOptimizationPreference=Size")]
    public void App_referencing_system_commandline_can_be_compiled_ahead_of_time(string additionalArgs)
    {
        var stdOut = new StringBuilder();
        var stdErr = new StringBuilder();

        var workingDirectory = Path.Combine(Directory.GetCurrentDirectory(), "TestApps", "NativeAOT");
        
        Process.RunToCompletion(
            DotnetMuxer.Path.FullName,
            "clean -c Release -r win-x64",
            workingDirectory: workingDirectory);

        var commandLine = string.Format(
            "publish -c Release -r win-x64 --self-contained -p:SystemCommandLineDllPath=\"{0}\" -p:TreatWarningsAsErrors=true {1}",
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

        stdOut.ToString().Should().NotContain("AOT analysis warning");
        stdErr.ToString().Should().BeEmpty();
        exitCode.Should().Be(0);
    }
}

#endif