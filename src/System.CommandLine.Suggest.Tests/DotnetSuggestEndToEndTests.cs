// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using FluentAssertions;
using System.CommandLine.Tests.Utility;
using System.Text;
using Xunit.Abstractions;
using static System.Environment;
using Process = System.CommandLine.Tests.Utility.Process;

namespace System.CommandLine.Suggest.Tests
{
    public class DotnetSuggestEndToEndTests : TestsWithTestApps
    {
        public DotnetSuggestEndToEndTests(ITestOutputHelper output) : base(output)
        {
        }

        [ReleaseBuildOnlyFact]
        public void Test_app_supplies_suggestions()
        {
            var stdOut = new StringBuilder();
            
            Output.WriteLine($"_endToEndTestApp.FullName: {EndToEndTestApp.FullName}");
            
            Process.RunToCompletion(
                EndToEndTestApp.FullName,
                "[suggest:1] \"a\"",
                stdOut: value => stdOut.AppendLine(value),
                environmentVariables: EnvironmentVariables);

            stdOut.ToString()
                  .Should()
                  .Be($"--apple{NewLine}--banana{NewLine}--durian{NewLine}");
        }

        [ReleaseBuildOnlyFact]
        public void Dotnet_suggest_provides_suggestions_for_app()
        {
            // run "dotnet-suggest register" in explicit way
            Process.RunToCompletion(
                DotnetSuggest.FullName,
                $"register --command-path \"{EndToEndTestApp.FullName}\"",
                stdOut: s => Output.WriteLine(s),
                stdErr: s => Output.WriteLine(s),
                environmentVariables: EnvironmentVariables).Should().Be(0);

            var stdOut = new StringBuilder();
            var stdErr = new StringBuilder();

            var commandLineToComplete = "a";

            Process.RunToCompletion(
                DotnetSuggest.FullName,
                $"get -e \"{EndToEndTestApp.FullName}\" --position {commandLineToComplete.Length} -- \"{commandLineToComplete}\"",
                stdOut: value => stdOut.AppendLine(value),
                stdErr: value => stdErr.AppendLine(value),
                environmentVariables: EnvironmentVariables);

            Output.WriteLine($"stdOut:{NewLine}{stdOut}{NewLine}");
            Output.WriteLine($"stdErr:{NewLine}{stdErr}{NewLine}");

            stdErr.ToString()
                  .Should()
                  .BeEmpty();

            stdOut.ToString()
                  .Should()
                  .Be($"--apple{NewLine}--banana{NewLine}--durian{NewLine}");
        }

        [ReleaseBuildOnlyFact]
        public void Dotnet_suggest_provides_suggestions_for_app_with_only_commandname()
        {
            // run "dotnet-suggest register" in explicit way
            Process.RunToCompletion(
                DotnetSuggest.FullName,
                $"register --command-path \"{EndToEndTestApp.FullName}\"",
                stdOut: s => Output.WriteLine(s),
                stdErr: s => Output.WriteLine(s),
                environmentVariables: EnvironmentVariables).Should().Be(0);

            var stdOut = new StringBuilder();
            var stdErr = new StringBuilder();

            var commandLineToComplete = "a ";

            Process.RunToCompletion(
                DotnetSuggest.FullName,
                $"get -e \"{EndToEndTestApp.FullName}\" --position {commandLineToComplete.Length} -- \"{commandLineToComplete}\"",
                stdOut: value => stdOut.AppendLine(value),
                stdErr: value => stdErr.AppendLine(value),
                environmentVariables: EnvironmentVariables);

            Output.WriteLine($"stdOut:{NewLine}{stdOut}{NewLine}");
            Output.WriteLine($"stdErr:{NewLine}{stdErr}{NewLine}");

            stdErr.ToString()
                  .Should()
                  .BeEmpty();

            stdOut.ToString()
                  .Should()
                  .Be($"--apple{NewLine}--banana{NewLine}--cherry{NewLine}--durian{NewLine}--help{NewLine}--version{NewLine}-?{NewLine}-h{NewLine}/?{NewLine}/h{NewLine}");
        }
        
        [ReleaseBuildOnlyFact]
        public void Dotnet_suggest_fails_to_provide_suggestions_because_app_faulted()
        {
            // run "dotnet-suggest register" in explicit way
            Process.RunToCompletion(
                DotnetSuggest.FullName,
                $"register --command-path \"{WaitAndFailTestApp.FullName}\"",
                stdOut: s => Output.WriteLine(s),
                stdErr: s => Output.WriteLine(s),
                environmentVariables: EnvironmentVariables).Should().Be(0);

            var stdOut = new StringBuilder();
            var stdErr = new StringBuilder();
            
            var commandLineToComplete = "a";
            
            Process.RunToCompletion(
                DotnetSuggest.FullName,
                $"get -e \"{WaitAndFailTestApp.FullName}\" --position {commandLineToComplete.Length} -- \"{commandLineToComplete}\"",
                stdOut: value => stdOut.AppendLine(value),
                stdErr: value => stdErr.AppendLine(value),
                environmentVariables: EnvironmentVariables);
            
            Output.WriteLine($"stdOut:{NewLine}{stdOut}{NewLine}");
            Output.WriteLine($"stdErr:{NewLine}{stdErr}{NewLine}");
            
            stdErr.ToString()
                .Should()
                .BeEmpty();
            
            stdOut.ToString()
                .Should()
                .BeEmpty();
        }
    }
}
