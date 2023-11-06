// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if NET6_0_OR_GREATER

using System.CommandLine.Suggest;
using System.CommandLine.Tests.Utility;
using System.IO;
using System.Text;
using FluentAssertions;
using Microsoft.DotNet.PlatformAbstractions;
using Xunit;
using Xunit.Abstractions;

namespace System.CommandLine.Tests;

public class CompilationTests
{
    private readonly ITestOutputHelper _output;
    private readonly string _systemCommandLineDllPath;

    public CompilationTests(ITestOutputHelper output)
    {
        _output = output;

        _systemCommandLineDllPath = typeof(CliCommand).Assembly.Location;
    }

    [ReleaseBuildOnlyTheory]
    [InlineData("")]
    [InlineData("-p:PublishSingleFile=true")]
    public void App_referencing_system_commandline_can_be_trimmed(string additionalArgs)
        => PublishAndValidate("Trimming", "warning IL", additionalArgs);

    [ReleaseBuildOnlyFact]
    public void App_referencing_system_commandline_can_be_compiled_ahead_of_time()
    {
        // TODO: Re-enable OSX validation when TFM is upgraded to net8.0.
        if (!System.Runtime.InteropServices.RuntimeInformation.IsOSPlatform(System.Runtime.InteropServices.OSPlatform.OSX))
        {
            PublishAndValidate("NativeAOT", "AOT analysis warning");
        }
    }

    private void PublishAndValidate(string appName, string warningText, string additionalArgs = null)
    {
        var stdOut = new StringBuilder();
        var stdErr = new StringBuilder();

        var workingDirectory = Path.Combine(Directory.GetCurrentDirectory(), "TestApps", appName);

        string rId = GetPortableRuntimeIdentifier();

        Process.RunToCompletion(
            DotnetMuxer.Path.FullName,
            $"clean -c Release -r {rId}",
            workingDirectory: workingDirectory);

        string publishCommand = string.Format(
            "publish -c Release -r {0} --self-contained -p:SystemCommandLineDllPath=\"{1}\" -p:TreatWarningsAsErrors=true {2}",
            rId,
            _systemCommandLineDllPath,
            additionalArgs);

        var exitCode = Process.RunToCompletion(
            DotnetMuxer.Path.FullName,
            publishCommand,
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

        stdOut.ToString().Should().NotContain(": error CS");
        stdOut.ToString().Should().NotContain(warningText);
        stdErr.ToString().Should().BeEmpty();
        exitCode.Should().Be(0);
    }

    private static string GetPortableRuntimeIdentifier()
    {
        string osPart = OperatingSystem.IsWindows() ? "win" : (OperatingSystem.IsMacOS() ? "osx" : "linux");
        return $"{osPart}-{RuntimeEnvironment.RuntimeArchitecture}";
    }
}

#endif
