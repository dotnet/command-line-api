// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.CommandLine.Invocation;
using System.CommandLine.Tests;
using System.IO;
using FluentAssertions;
using System.Linq;
using System.Threading.Tasks;
using Xunit.Abstractions;

namespace System.CommandLine.Suggest.Tests
{
    public class DotnetSuggestEndToEndTests
    {
        private readonly ITestOutputHelper _output;
        private readonly FileInfo _endToEndTestApp;
        private readonly FileInfo _dotnetSuggest;

        public DotnetSuggestEndToEndTests(ITestOutputHelper output)
        {
            _output = output;

            // delete sentinel files for EndToEndTestApp in order to trigger registration when it's run
            var sentinels = Directory.GetFiles(Path.GetTempPath(), "*EndToEndTestApp*");
            foreach (var sentinel in sentinels)
            {
                File.Delete(sentinel);
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
        }

        [ReleaseBuildOnlyFact]
        public async Task Test_app_supplies_completions()
        {
            var (exitCode, stdOut, stdErr) = await Process.ExecuteAsync(
                                                 _endToEndTestApp.FullName,
                                                 "[suggest] a");

            stdOut.Should().Be($"--apple{Environment.NewLine}--banana{Environment.NewLine}--durian{Environment.NewLine}");
        }

        [ReleaseBuildOnlyFact]
        public async Task dotnet_suggest_provides_completions_for_app()
        {
            // run once to trigger a call to dotnet-suggest register
            await Process.ExecuteAsync
            (_endToEndTestApp.FullName,
             "-h",
             stdOut: s => _output.WriteLine(s),
             stdErr: s => _output.WriteLine(s)
            );

            var (exitCode, stdOut, stdErr) = await Process.ExecuteAsync(
                                                 _dotnetSuggest.FullName,
                                                 $"-e \"{_endToEndTestApp.FullName}\" -p 0 a");

            stdOut.Should().Be($"--apple{Environment.NewLine}--banana{Environment.NewLine}--durian{Environment.NewLine}");
        }
    }
}
