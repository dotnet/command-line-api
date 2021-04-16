// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using FluentAssertions;
using System.CommandLine.Tests.Utility;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit.Abstractions;
using static System.Environment;

namespace System.CommandLine.Suggest.Tests
{
    public class DotnetSuggestEndToEndTests : IDisposable
    {
        private readonly ITestOutputHelper _output;
        private readonly FileInfo _endToEndTestApp;
        private readonly FileInfo _dotnetSuggest;
        private readonly (string, string)[] _environmentVariables;
        private readonly DirectoryInfo _dotnetHostDir = DotnetMuxer.Path.Directory;
        private static string _testRoot;

        public DotnetSuggestEndToEndTests(ITestOutputHelper output)
        {
            _output = output;

            // delete sentinel files for EndToEndTestApp in order to trigger registration when it's run
            var sentinelsDir = new DirectoryInfo(Path.Combine(Path.GetTempPath(), "system-commandline-sentinel-files"));

            if (sentinelsDir.Exists)
            {
                var sentinels = sentinelsDir.GetFiles("*EndToEndTestApp*");

                foreach (var sentinel in sentinels)
                {
                    sentinel.Delete();
                }
            }

            var currentDirectory = Path.Combine(
                Directory.GetCurrentDirectory(),
                "TestAssets");

            _endToEndTestApp = new DirectoryInfo(currentDirectory)
                               .GetFiles("EndToEndTestApp".ExecutableName())
                               .SingleOrDefault();

            _dotnetSuggest = new DirectoryInfo(currentDirectory)
                             .GetFiles("dotnet-suggest".ExecutableName())
                             .SingleOrDefault();

            PrepareTestHomeDirectoryToAvoidPolluteBuildMachineHome();

            _environmentVariables = new[] {
                ("DOTNET_ROOT", _dotnetHostDir.FullName),
                ("INTERNAL_TEST_DOTNET_SUGGEST_HOME", _testRoot)};
        }

        public void Dispose()
        {
            if (_testRoot != null && Directory.Exists(_testRoot))
            {
                Directory.Delete(_testRoot, recursive: true);
            }
        }

        private static void PrepareTestHomeDirectoryToAvoidPolluteBuildMachineHome()
        {
            _testRoot = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
            Directory.CreateDirectory(_testRoot);
        }

        [ReleaseBuildOnlyFact]
        public async Task Test_app_supplies_suggestions()
        {
            var stdOut = new StringBuilder();

            await ExecuteAsync(
                _endToEndTestApp.FullName,
                "[suggest:1] \"a\"",
                stdOut: value => stdOut.AppendLine(value),
                environmentVariables: _environmentVariables);

            stdOut.ToString()
                  .Should()
                  .Be($"--apple{NewLine}--banana{NewLine}--durian{NewLine}");
        }

        [ReleaseBuildOnlyFact]
        public async Task Dotnet_suggest_provides_suggestions_for_app()
        {
            // run once to trigger a call to dotnet-suggest register
            await ExecuteAsync(
                _endToEndTestApp.FullName,
                "-h",
                stdOut: s => _output.WriteLine(s),
                stdErr: s => _output.WriteLine(s),
                environmentVariables: _environmentVariables);

            var stdOut = new StringBuilder();
            var stdErr = new StringBuilder();

            var commandLineToComplete = "a";

            await ExecuteAsync(
                _dotnetSuggest.FullName,
                $"get -e \"{_endToEndTestApp.FullName}\" --position {commandLineToComplete.Length} -- \"{commandLineToComplete}\"",
                stdOut: value => stdOut.AppendLine(value),
                stdErr: value => stdErr.AppendLine(value),
                environmentVariables: _environmentVariables);

            _output.WriteLine($"stdOut:{NewLine}{stdOut}{NewLine}");
            _output.WriteLine($"stdErr:{NewLine}{stdErr}{NewLine}");

            stdErr.ToString()
                  .Should()
                  .BeEmpty();

            stdOut.ToString()
                  .Should()
                  .Be($"--apple{NewLine}--banana{NewLine}--durian{NewLine}");
        }

        [ReleaseBuildOnlyFact]
        public async Task Dotnet_suggest_provides_suggestions_for_app_with_only_commandname()
        {
            // run once to trigger a call to dotnet-suggest register
            await ExecuteAsync(
                _endToEndTestApp.FullName,
                "-h",
                stdOut: s => _output.WriteLine(s),
                stdErr: s => _output.WriteLine(s),
                environmentVariables: _environmentVariables);

            var stdOut = new StringBuilder();
            var stdErr = new StringBuilder();

            var commandLineToComplete = "a ";

            await ExecuteAsync(
                _dotnetSuggest.FullName,
                $"get -e \"{_endToEndTestApp.FullName}\" --position {commandLineToComplete.Length} -- \"{commandLineToComplete}\"",
                stdOut: value => stdOut.AppendLine(value),
                stdErr: value => stdErr.AppendLine(value),
                environmentVariables: _environmentVariables);

            _output.WriteLine($"stdOut:{NewLine}{stdOut}{NewLine}");
            _output.WriteLine($"stdErr:{NewLine}{stdErr}{NewLine}");

            stdErr.ToString()
                .Should()
                .BeEmpty();

            stdOut.ToString()
                .Should()
                .Be($"--apple{NewLine}--banana{NewLine}--cherry{NewLine}--durian{NewLine}--help{NewLine}--version{NewLine}-?{NewLine}-h{NewLine}/?{NewLine}/h{NewLine}");
        }

        private static async Task ExecuteAsync(
            string command,
            string args,
            Action<string> stdOut = null,
            Action<string> stdErr = null,
            params (string key, string value)[] environmentVariables)
        {
            args ??= "";

            var process = new Diagnostics.Process
            {
                StartInfo =
                {
                    Arguments = args,
                    FileName = command,
                    RedirectStandardError = true,
                    RedirectStandardOutput = true,
                    RedirectStandardInput = true,
                    UseShellExecute = false
                }
            };

            if (environmentVariables.Length > 0)
            {
                for (var i = 0; i < environmentVariables.Length; i++)
                {
                    var (key, value) = environmentVariables[i];
                    process.StartInfo.Environment.Add(key, value);
                }
            }

            if (stdOut != null)
            {
                process.OutputDataReceived += (sender, eventArgs) =>
                {
                    if (eventArgs.Data != null)
                    {
                        stdOut(eventArgs.Data);
                    }
                };
            }

            if (stdErr != null)
            {
                process.ErrorDataReceived += (sender, eventArgs) =>
                {
                    if (eventArgs.Data != null)
                    {
                        stdErr(eventArgs.Data);
                    }
                };
            }

            process.Start();

            process.BeginOutputReadLine();
            process.BeginErrorReadLine();

            await process.WaitForExitAsync();
        }
    }
}
