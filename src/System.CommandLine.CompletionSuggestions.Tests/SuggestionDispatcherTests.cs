// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.CommandLine.Tests;
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
        private readonly string[] _args = @"-p 12 -e ""C:\Program Files\dotnet\dotnet.exe"" ""dotnet add""".Tokenize()
            .ToArray();

        [Fact]
        public async Task Dispatch_executes_dotnet_complete()
        {
            (await InvokeAsync(_args, new TestSuggestionProvider(), 20000))
                    .Should()
                    .Contain("package")
                    .And.Contain("reference");
        }

        [Fact]
        public void Dispatch_with_missing_position_arg_throws()
        {
            Func<Task> action = async () =>
                await InvokeAsync(
                    @"-e ""C:\Program Files\dotnet\dotnet.exe"" ""dotnet add"" -p".Tokenize().ToArray(),
                    new TestSuggestionProvider());
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
        public async Task Dispatch_with_unknown_completion_provider_returns_empty_string()
        {
            (await InvokeAsync(_args, new TestSuggestionProvider(null)))
                .Should()
                .BeEmpty();
        }

        [Fact]
        public void Command_suggestions_with_bogus_filename_throws_exception()
        {
            string exeFileName = "bogus file name";

            var provider = new TestSuggestionProvider(new SuggestionRegistration(exeFileName, "missing commplete command"));
            var dispatcher = new SuggestionDispatcher(provider);

            var args = $@"-p 12 -e ""{exeFileName}"" ""dotnet add""".Tokenize().ToArray();

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
            var provider = new TestSuggestionProvider(new SuggestionRegistration("dotnet", $"dotnet {Assembly.GetExecutingAssembly().Location}"));
            var dispatcher = new SuggestionDispatcher(provider);
            dispatcher.Timeout = TimeSpan.FromMilliseconds(1);
            var testConsole = new TestConsole();

            var args = $@"-p 0 -e ""dotnet"" ""dotnet add""".Tokenize().ToArray();

            await dispatcher.InvokeAsync(_args, testConsole);

            testConsole.Out.ToString().Should().BeEmpty();
        }

        [Fact]
        public async Task List_command_gets_all_executable_names()
        {
            TestSuggestionProvider testSuggestionProvider;
            if (RuntimeInformation
                .IsOSPlatform(OSPlatform.Windows))
            {
                testSuggestionProvider = new TestSuggestionProvider(
                    new[] {
                        new SuggestionRegistration(@"C:\Program Files\dotnet\dotnet.exe","dotnet complete"),
                        new SuggestionRegistration(@"C:\Program Files\himalayan-berry.exe","himalayan-berry spread")
                    },
                    new SuggestionRegistration(@"C:\Program Files\dotnet\dotnet.exe", "dotnet complete"));
            }
            else
            {
                testSuggestionProvider = new TestSuggestionProvider(
                    new[] {
                        new SuggestionRegistration(@"/bin/dotnet", "dotnet complete"),
                        new SuggestionRegistration(@"/bin/himalayan-berry", "himalayan-berry spread")
                    },
                    new SuggestionRegistration(@"/bin/dotnet", "dotnet complete"));
            }
            
            var dispatcher = new SuggestionDispatcher(testSuggestionProvider);
            var testConsole = new TestConsole();

            await dispatcher.InvokeAsync(new[] {"list"}, testConsole);

            testConsole.Out.ToString().Should().Be($"dotnet himalayan-berry{Environment.NewLine}");
        }

        [Fact]
        public async Task Register_command_adds_new_suggestion_entry()
        {
            var provider = new TestSuggestionProvider();
            var dispatcher = new SuggestionDispatcher(provider);

            await dispatcher.InvokeAsync("register --command-path \"C:\\Windows\\System32\\net.exe\" --suggestion-command \"net-suggestions complete\"".Tokenize().ToArray());

            SuggestionRegistration addedRegistration = provider.AddedRegistrations.Single();
            addedRegistration.CommandPath.Should().Be(@"C:\Windows\System32\net.exe");
            addedRegistration.SuggestionCommand.Should().Be("net-suggestions complete");
        }

        private static async Task<string> InvokeAsync(
            string[] args,
            ISuggestionProvider suggestionProvider,
            int timeoutMilliseconds = 2000)
        {
            var dispatcher = new SuggestionDispatcher(suggestionProvider);
            var testConsole = new TestConsole();
            await dispatcher.InvokeAsync(args, testConsole);
            return testConsole.Out.ToString();
        }
    }
}
