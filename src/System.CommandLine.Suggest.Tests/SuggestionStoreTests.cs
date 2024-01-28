// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.CommandLine.Tests.Utility;
using System.IO;
using System.Linq;
using FluentAssertions;
using Xunit.Abstractions;
using static System.Environment;

namespace System.CommandLine.Suggest.Tests
{
    public class SuggestionStoreTests : IDisposable
    {
        protected readonly ITestOutputHelper Output;
        protected readonly FileInfo EndToEndTestApp;
        protected readonly FileInfo WaitAndFailTestApp;
        protected readonly FileInfo DotnetSuggest;
        protected readonly (string, string)[] _environmentVariables;
        private readonly DirectoryInfo _dotnetHostDir = DotnetMuxer.Path.Directory;
        private static string _testRoot;
        
        public SuggestionStoreTests(ITestOutputHelper output)
        {
            Output = output;

            // delete sentinel files for TestApps in order to trigger registration when it's run
            var sentinelsDir = new DirectoryInfo(Path.Combine(Path.GetTempPath(), "system-commandline-sentinel-files"));

            if (sentinelsDir.Exists)
            {
                var sentinels = sentinelsDir
                    .EnumerateFiles()
                    .Where(f => f.Name.Contains("EndToEndTestApp") || f.Name.Contains("WaitAndFailTestApp"));

                foreach (var sentinel in sentinels)
                {
                    sentinel.Delete();
                }
            }

            var currentDirectory = Path.Combine(
                Directory.GetCurrentDirectory(),
                "TestAssets");

            EndToEndTestApp = new DirectoryInfo(currentDirectory)
                .GetFiles("EndToEndTestApp".ExecutableName())
                .SingleOrDefault();

            WaitAndFailTestApp = new DirectoryInfo(currentDirectory)
                .GetFiles("WaitAndFailTestApp".ExecutableName())
                .SingleOrDefault();
            
            DotnetSuggest = new DirectoryInfo(currentDirectory)
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
        public void GetCompletions_obtains_suggestions_successfully()
        {
            var store = new SuggestionStore();
            var completions = store.GetCompletions(EndToEndTestApp.FullName, "[suggest:1] \"a\"", TimeSpan.FromSeconds(1));
            completions.Should().Be($"--apple{NewLine}--banana{NewLine}--durian{NewLine}");
        }
        
        [ReleaseBuildOnlyFact]
        public void GetCompletions_fails_to_obtain_suggestions_because_app_takes_too_long()
        {
            var store = new SuggestionStore();
            var appHangingTimeSpanArgument = TimeSpan.FromMilliseconds(2000).ToString();
            var completions = store
                .GetCompletions(WaitAndFailTestApp.FullName, appHangingTimeSpanArgument, TimeSpan.FromMilliseconds(1000));
            completions.Should().BeEmpty();
        }
        
        [ReleaseBuildOnlyFact]
        public void GetCompletions_fails_to_obtain_suggestions_because_app_exited_with_nonzero_code()
        {
            var store = new SuggestionStore();
            var appHangingTimeSpanArgument = TimeSpan.FromMilliseconds(0).ToString();
            var completions = store
                .GetCompletions(WaitAndFailTestApp.FullName, appHangingTimeSpanArgument, TimeSpan.FromMilliseconds(1000));
            completions.Should().BeEmpty();
        }
    }
}
