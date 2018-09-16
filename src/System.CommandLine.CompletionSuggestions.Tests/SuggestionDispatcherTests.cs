// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.CommandLine.Builder;
using System.CommandLine.Invocation;
using System.CommandLine.Tests;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using FluentAssertions;
using Xunit;

namespace System.CommandLine.CompletionSuggestions.Tests
{
    public class SuggestionDispatcherTests
    {
        private static SuggestionRegistration GetDotnetSuggestionRegistration()
            => new SuggestionRegistration(GetDotnetPath(), "testdotnet [suggest]");

        private static string GetDotnetPath() => Path.GetFullPath("testdotnet");

        [Fact]
        public async Task InvokeAsync_executes_completion_command_for_executable()
        {
            string[] args = $@"-p 12 -e ""{GetDotnetPath()}"" ""testdotnet add""".Tokenize().ToArray();

            (await InvokeAsync(args, new TestSuggestionRegistration(GetDotnetSuggestionRegistration())))
                    .Should()
                    .Contain("package")
                    .And.Contain("reference");
        }

        [Fact]
        public void InvokeAsync_with_missing_position_arg_throws()
        {
            Func<Task> action = async () =>
                await InvokeAsync(
                    $@"-e ""{GetDotnetPath()}"" ""testdotnet add"" -p".Tokenize().ToArray(),
                    new TestSuggestionRegistration(GetDotnetSuggestionRegistration()));
            action
               .Should()
               .Throw<TargetInvocationException>()
               .Which
               .InnerException
               .Message
               .Should()
               .Be("Required argument missing for option: -p");
        }

        [Fact]
        public async Task InvokeAsync_with_unknown_suggestion_provider_returns_empty_string()
        {
            string[] args = @"-p 10 -e ""testcli.exe"" ""command op""".Tokenize().ToArray();
            (await InvokeAsync(args, new TestSuggestionRegistration()))
                .Should()
                .BeEmpty();
        }

        [Fact]
        public void Command_suggestions_filename_that_does_not_exist_throws_exception()
        {
            string exeFileName = Path.GetFullPath("file_that_does_not_exist_name");

            var provider = new TestSuggestionRegistration(new SuggestionRegistration(exeFileName, "missing complete command"));
            var dispatcher = new SuggestionDispatcher(provider);

            var args = $@"-p 12 -e ""{exeFileName}"" ""testdotnet add""".Tokenize().ToArray();

            Func<Task> action = async () => await dispatcher.InvokeAsync(args);

            action
                .Should()
                .Throw<TargetInvocationException>()
                .WithInnerException<ArgumentException>("System.Diagnostics.Process is nuts.")
                .Where(exception => exception.Message.Contains(
                    $"Unable to find the file '{ exeFileName }'"));
        }

        [Fact]
        public async Task When_command_suggestions_use_process_that_remains_open_it_returns_empty_string()
        {
            var provider = new TestSuggestionRegistration(new SuggestionRegistration(GetDotnetPath(), $"testdotnet {Assembly.GetExecutingAssembly().Location}"));
            var dispatcher = new SuggestionDispatcher(provider, new TestSuggestionStore());
            dispatcher.Timeout = TimeSpan.FromMilliseconds(1);
            var testConsole = new TestConsole();

            var args = $@"-p 0 -e ""testdotnet"" ""testdotnet add""".Tokenize().ToArray();

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
                    new SuggestionRegistration(@"C:\Program Files\dotnet\dotnet.exe","dotnet complete"),
                    new SuggestionRegistration(@"C:\Program Files\himalayan-berry.exe","himalayan-berry spread"));
            }
            else
            {
                testSuggestionProvider = new TestSuggestionRegistration(
                    new SuggestionRegistration(@"/bin/dotnet", "dotnet complete"),
                    new SuggestionRegistration(@"/bin/himalayan-berry", "himalayan-berry spread"));
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

            SuggestionRegistration addedRegistration = provider.FindAllRegistrations().Single();
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
                var parser = new CommandLineBuilder("testdotnet")
                            .AddCommand("add", "add description",
                                    symbols: s => s.AddCommand("package", "package description")
                                                   .AddCommand("reference", "reference description"))
                           .AddCommand("[suggest]",
                                    symbols: a => a.AddOption(new[] { "-p", "--position" },
                                    arguments: ar => ar.ParseArgumentsAs<int>()))
                            .TreatUnmatchedTokensAsErrors(false)
                            .Build();
                var parseResult = parser.Parse(suggestionTargetArguments);
                var position = parseResult.ValueForOption<int>("position");
                var suggested = parser.Parse(parseResult.UnmatchedTokens).Suggestions();
                return string.Join(
                        Environment.NewLine,
                        suggested);
            }
        }
    }
}
