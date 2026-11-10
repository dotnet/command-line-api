// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if NET6_0_OR_GREATER

using System.CommandLine.Suggest;
using System.CommandLine.Tests.Utility;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
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

        _systemCommandLineDllPath = typeof(Command).Assembly.Location;
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

    [ReleaseBuildOnlyFact]
    public void RootCommand_name_falls_back_to_the_AppContext_value_when_hosted_as_a_native_library()
    {
        // When System.CommandLine is hosted inside a NativeAOT shared library there is no
        // managed entry point (Assembly.GetEntryAssembly() returns null) and
        // Environment.GetCommandLineArgs() reflects the host process. RootCommand must then
        // fall back to the executable name injected into AppContext by the build targets (the
        // assembly name). This exercises that path end-to-end through a real native library build.
        var workingDirectory = Path.Combine(Directory.GetCurrentDirectory(), "TestApps", "NativeLibrary");
        var publishDirectory = Path.Combine(Path.GetTempPath(), "scl-nativelib-" + Guid.NewGuid().ToString("N"));
        var targetsPath = Path.Combine(Directory.GetCurrentDirectory(), "TestApps", "System.CommandLine.targets");
        string rId = GetPortableRuntimeIdentifier();

        Process.RunToCompletion(
            DotnetMuxer.Path.FullName,
            $"clean -c Release -r {rId} -p:SystemCommandLineTargetsPath=\"{targetsPath}\"",
            workingDirectory: workingDirectory);

        var stdOut = new StringBuilder();
        var stdErr = new StringBuilder();

        try
        {
            var exitCode = Process.RunToCompletion(
                DotnetMuxer.Path.FullName,
                string.Format(
                    "publish -c Release -r {0} --self-contained -o \"{1}\" -p:SystemCommandLineDllPath=\"{2}\" -p:SystemCommandLineTargetsPath=\"{3}\" -p:TreatWarningsAsErrors=true",
                    rId,
                    publishDirectory,
                    _systemCommandLineDllPath,
                    targetsPath),
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

            string publishOutput = $"{Environment.NewLine}STDOUT:{Environment.NewLine}{stdOut}{Environment.NewLine}STDERR:{Environment.NewLine}{stdErr}";

            stdOut.ToString().Should().NotContain(": error CS", "the native library should compile cleanly. Publish output:{0}", publishOutput);
            exitCode.Should().Be(0, "the native library should publish successfully. Publish output:{0}", publishOutput);

            string nativeLibraryPath = Path.Combine(publishDirectory, NativeLibraryFileName("NativeLibrary"));

            File.Exists(nativeLibraryPath).Should().BeTrue(
                "the published native library should exist at {0}. Publish output:{1}", nativeLibraryPath, publishOutput);

            string executableName = InvokeGetExecutableName(nativeLibraryPath);

            // Equality to the assembly name proves the AppContext fallback was taken: had
            // GetCommandLineArgs() been non-empty, the name would derive from the host path.
            executableName.Should().Be("NativeLibrary");
        }
        finally
        {
            try
            {
                if (Directory.Exists(publishDirectory))
                {
                    Directory.Delete(publishDirectory, recursive: true);
                }
            }
            catch (UnauthorizedAccessException)
            {
                // On Windows the native library may still be locked after NativeLibrary.Free
            }
        }
    }

    private static string NativeLibraryFileName(string assemblyName) =>
        OperatingSystem.IsWindows() ? $"{assemblyName}.dll"
            : OperatingSystem.IsMacOS() ? $"{assemblyName}.dylib"
            : $"{assemblyName}.so";

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    private delegate int GetExecutableNameDelegate(IntPtr buffer, int bufferLength);

    private static string InvokeGetExecutableName(string nativeLibraryPath)
    {
        IntPtr handle = NativeLibrary.Load(nativeLibraryPath);

        try
        {
            IntPtr export = NativeLibrary.GetExport(handle, "get_executable_name");
            var getExecutableName = Marshal.GetDelegateForFunctionPointer<GetExecutableNameDelegate>(export);

            int length = getExecutableName(IntPtr.Zero, 0);
            byte[] buffer = new byte[length];

            GCHandle pinned = GCHandle.Alloc(buffer, GCHandleType.Pinned);
            try
            {
                getExecutableName(pinned.AddrOfPinnedObject(), length);
            }
            finally
            {
                pinned.Free();
            }

            return Encoding.UTF8.GetString(buffer);
        }
        finally
        {
            NativeLibrary.Free(handle);
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
        return $"{osPart}-{Microsoft.DotNet.PlatformAbstractions.RuntimeEnvironment.RuntimeArchitecture}";
    }
}

#endif
