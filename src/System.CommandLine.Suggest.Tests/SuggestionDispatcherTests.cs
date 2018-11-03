// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.CommandLine.Builder;
using System.CommandLine.Tests;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using FluentAssertions;
using Xunit;

namespace System.CommandLine.Suggest.Tests
{
    public class SuggestionDispatcherTests
    {
        private static string _fakedotnet = CommandLineBuilder.ExeName;

        private static RegistrationPair FakeDotnetRegistrationPair()
            => new RegistrationPair(FakeDotnetFullPath(), $"{_fakedotnet} [suggest]");

        private static string FakeDotnetFullPath() => Path.GetFullPath(_fakedotnet);

        [Fact]
        public async Task InvokeAsync_executes_completion_command_for_executable()
        {
            string[] args = $@"get -p 12 -e ""{FakeDotnetFullPath()}"" --  {_fakedotnet} add".Tokenize().ToArray();

            var suggestions = await InvokeAsync(args, new TestSuggestionRegistration(FakeDotnetRegistrationPair()));

            suggestions
                .Should()
                .Contain("package")
                .And
                .Contain("reference");
        }

        [Fact]
        public async Task InvokeAsync_with_unknown_suggestion_provider_returns_empty_string()
        {
            string[] args = @"get -p 10 -e ""testcli.exe"" -- command op".Tokenize().ToArray();
            (await InvokeAsync(args, new TestSuggestionRegistration()))
                .Should()
                .BeEmpty();
        }

        [Fact]
        public async Task When_command_suggestions_use_process_that_remains_open_it_returns_empty_string()
        {
            var provider = new TestSuggestionRegistration(new RegistrationPair(FakeDotnetFullPath(), $"{_fakedotnet} {Assembly.GetExecutingAssembly().Location}"));
            var dispatcher = new SuggestionDispatcher(provider, new TestSuggestionStore());
            dispatcher.Timeout = TimeSpan.FromMilliseconds(1);
            var testConsole = new TestConsole();

            var args = $@"get -p 0 -e ""{_fakedotnet}"" -- {_fakedotnet} add".Tokenize().ToArray();

            await dispatcher.InvokeAsync(args, testConsole);

            testConsole.Out.ToString().Should().BeEmpty();
        }

        [Fact]
        public async Task List_command_gets_all_executable_names()
        {
            TestSuggestionRegistration testSuggestionProvider;
            if (RuntimeInformation
                .IsOSPlatform(OSPlatform.Windows))
            {
                testSuggestionProvider = new TestSuggestionRegistration(
                    new RegistrationPair(@"C:\Program Files\dotnet\dotnet.exe","dotnet complete"),
                    new RegistrationPair(@"C:\Program Files\himalayan-berry.exe","himalayan-berry spread"));
            }
            else
            {
                testSuggestionProvider = new TestSuggestionRegistration(
                    new RegistrationPair(@"/bin/dotnet", "dotnet complete"),
                    new RegistrationPair(@"/bin/himalayan-berry", "himalayan-berry spread"));
            }

            var dispatcher = new SuggestionDispatcher(testSuggestionProvider);
            var testConsole = new TestConsole();

            await dispatcher.InvokeAsync(new[] {"list"}, testConsole);

            testConsole.Out.ToString().Should().Be($"dotnet himalayan-berry{Environment.NewLine}");
        }

        [Fact]
        public async Task Register_command_adds_new_suggestion_entry()
        {
            var provider = new TestSuggestionRegistration();
            var dispatcher = new SuggestionDispatcher(provider);

            await dispatcher.InvokeAsync("register --command-path \"C:\\Windows\\System32\\net.exe\" --suggestion-command \"net-suggestions complete\"".Tokenize().ToArray());

            RegistrationPair addedRegistration = provider.FindAllRegistrations().Single();
            addedRegistration.CommandPath.Should().Be(@"C:\Windows\System32\net.exe");
            addedRegistration.SuggestionCommand.Should().Be("net-suggestions complete");
        }

        private static async Task<string> InvokeAsync(
            string[] args,
            ISuggestionRegistration suggestionProvider)
        {
            var dispatcher = new SuggestionDispatcher(suggestionProvider, new TestSuggestionStore());
            var testConsole = new TestConsole();
            await dispatcher.InvokeAsync(args, testConsole);
            return testConsole.Out.ToString();
        }

        private class TestSuggestionStore : ISuggestionStore
        {
            public string GetSuggestions(string exeFileName, string suggestionTargetArguments, TimeSpan timeout)
            {
                if (timeout <= TimeSpan.FromMilliseconds(100))
                {
                    return "";
                }

                if (exeFileName != FakeDotnetFullPath())
                {
                    return $"unexpected value for {nameof(exeFileName)}: {exeFileName}";
                }

                if (suggestionTargetArguments != $"[suggest] {_fakedotnet} add")
                {
                    return $"unexpected value for {nameof(suggestionTargetArguments)}: {suggestionTargetArguments}";
                }

                return $"package{Environment.NewLine}reference";
            }
        }
    }
}
