// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.CommandLine.Parsing;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using FluentAssertions;
using Xunit;

namespace System.CommandLine.Suggest.Tests
{
    public class SuggestionDispatcherTests
    {
        private static readonly string _currentExeName = CliRootCommand.ExecutableName;
        
        private static readonly string _dotnetExeFullPath = 
            DotnetMuxer.Path.FullName;

        private static readonly string _dotnetFormatExeFullPath =
            RuntimeInformation.IsOSPlatform(OSPlatform.Windows)
                ? @"C:\Program Files\dotnet-format.exe"
                : "/bin/dotnet-format";

        private static readonly string _netExeFullPath =
            RuntimeInformation.IsOSPlatform(OSPlatform.Windows)
                ? @"C:\Windows\System32\net.exe"
                : "/bin/net";

        private static Registration CurrentExeRegistrationPair() => new(CurrentExeFullPath());

        private static string CurrentExeFullPath() => Path.GetFullPath(_currentExeName);

        [Fact]
        public async Task InvokeAsync_executes_registered_executable()
        {
            string receivedTargetExeName = null;

            string[] args = CliParser.SplitCommandLine($@"get -p 12 -e ""{CurrentExeFullPath()}"" -- ""{_currentExeName} add""").ToArray();

            await InvokeAsync(
                args,
                new TestSuggestionRegistration(CurrentExeRegistrationPair()),
                new AnonymousSuggestionStore(
                    (targetExeName, targetExeArgs, _) =>
                    {
                        receivedTargetExeName = targetExeName;

                        return "";
                    }));

            receivedTargetExeName.Should().Be(CurrentExeFullPath());
        }

        [Fact]
        public async Task InvokeAsync_executes_suggestion_command_for_executable()
        {
            string receivedTargetExeArgs = null;

            var args = PrepareArgs($@"get -p 58 -e ""{CurrentExeFullPath()}"" -- ""{_currentExeName} add""");

            await InvokeAsync(
                args,
                new TestSuggestionRegistration(CurrentExeRegistrationPair()),
                new AnonymousSuggestionStore(
                    (targetExeName, targetExeArgs, _) =>
                    {
                        receivedTargetExeArgs = targetExeArgs;

                        return "";
                    }));

            var expectedPosition = 57 - _currentExeName.Length;

            receivedTargetExeArgs.Should()
                                 .Be($"[suggest:{expectedPosition}] \"add\"");
        }

        [Theory]
        [InlineData("dotnet-abcdef.exe --dry", 23, "[suggest:5] \"--dry\"")]
        [InlineData("dotnet abcdef --dry", 19, "[suggest:5] \"--dry\"")]
        [InlineData("dotnet     abcdef --dry", 23, "[suggest:5] \"--dry\"")]
        [InlineData("dotnet     abcdef", 18, "[suggest:0] \"\"")]
        [InlineData("dotnet", 7, "[suggest:0] \"\"")]
        public async Task InvokeAsync_executes_suggestion_command_for_executable_called_via_dotnet_muxer(
            string scriptSendsCommand,
            int scriptSendsPosition,
            string expectToReceive)
        {
            string receivedTargetExeArgs = null;

            var args = PrepareArgs($@"get -p {scriptSendsPosition} -e ""{_dotnetExeFullPath}"" -- ""{scriptSendsCommand}""");

            await InvokeAsync(
                args,
                new TestSuggestionRegistration(CurrentExeRegistrationPair()),
                new AnonymousSuggestionStore(
                    (targetExeName, targetExeArgs, _) =>
                    {
                        receivedTargetExeArgs = targetExeArgs;

                        return "";
                    }));

            receivedTargetExeArgs.Should()
                                 .Be(expectToReceive);
        }

        private static string[] PrepareArgs(string args)
        {
            var formattableString = args.Replace("$", "");
            return CliParser.SplitCommandLine(formattableString).ToArray();
        }

        [Fact]
        public async Task InvokeAsync_with_unknown_suggestion_provider_returns_empty_string()
        {
            string[] args = Enumerable.ToArray(CliParser.SplitCommandLine(@"get -p 10 -e ""testcli.exe"" -- command op"));
            (await InvokeAsync(args, new TestSuggestionRegistration()))
                .Should()
                .BeEmpty();
        }

        [Fact]
        public async Task When_command_suggestions_use_process_that_remains_open_it_returns_empty_string()
        {
            var provider = new TestSuggestionRegistration(new Registration(CurrentExeFullPath()));
            var dispatcher = new SuggestionDispatcher(provider, new TestSuggestionStore());
            dispatcher.Timeout = TimeSpan.FromMilliseconds(1);
            dispatcher.Configuration.Output = new StringWriter();

            var args = CliParser.SplitCommandLine($@"get -p 0 -e ""{_currentExeName}"" -- {_currentExeName} add").ToArray();

            await dispatcher.InvokeAsync(args);

            dispatcher.Configuration.Output.ToString().Should().BeEmpty();
        }

        [Fact]
        public async Task List_command_gets_all_executable_names()
        {
            string _kiwiFruitExeFullPath =
                RuntimeInformation.IsOSPlatform(OSPlatform.Windows)
                    ? @"C:\Program Files\kiwi-fruit.exe"
                    : "/bin/kiwi-fruit";

            var testSuggestionProvider = new TestSuggestionRegistration(
                new Registration(_dotnetFormatExeFullPath),
                new Registration(_kiwiFruitExeFullPath));

            var dispatcher = new SuggestionDispatcher(testSuggestionProvider);
            dispatcher.Configuration.Output = new StringWriter();

            await dispatcher.InvokeAsync(new[] { "list" });

            dispatcher.Configuration.Output
                       .ToString()
                       .Should()
                       .Be($"dotnet-format{Environment.NewLine}dotnet format{Environment.NewLine}kiwi-fruit{Environment.NewLine}");
        }

        [Fact]
        public async Task Register_command_adds_new_suggestion_entry()
        {
            var provider = new TestSuggestionRegistration();
            var dispatcher = new SuggestionDispatcher(provider);

            var args = CliParser.SplitCommandLine($"register --command-path \"{_netExeFullPath}\"").ToArray();

            await dispatcher.InvokeAsync(args);

            Registration addedRegistration = provider.FindAllRegistrations().Single();
            addedRegistration.ExecutablePath.Should().Be(_netExeFullPath);
        }

        [Fact]
        public async Task Register_command_will_not_add_duplicate_entry()
        {
            var provider = new TestSuggestionRegistration();
            var dispatcher = new SuggestionDispatcher(provider);

            var args = CliParser.SplitCommandLine($"register --command-path \"{_netExeFullPath}\"").ToArray();

            await dispatcher.InvokeAsync(args);
            await dispatcher.InvokeAsync(args);

            provider.FindAllRegistrations().Should().HaveCount(1);
        }

        private static async Task<string> InvokeAsync(
            string[] args,
            ISuggestionRegistration suggestionProvider,
            ISuggestionStore suggestionStore = null)
        {
            var dispatcher = new SuggestionDispatcher(suggestionProvider, suggestionStore ?? new TestSuggestionStore());
            dispatcher.Configuration.Output = new StringWriter();
            await dispatcher.InvokeAsync(args);
            return dispatcher.Configuration.Output.ToString();
        }

        private class TestSuggestionStore : ISuggestionStore
        {
            public string GetCompletions(string exeFileName, string suggestionTargetArguments, TimeSpan timeout)
            {
                if (timeout <= TimeSpan.FromMilliseconds(100))
                {
                    return "";
                }

                if (exeFileName != CurrentExeFullPath())
                {
                    return $"unexpected value for {nameof(exeFileName)}: {exeFileName}";
                }

                if (!Regex.IsMatch(suggestionTargetArguments, @"\[suggest:\d+\] add"))
                {
                    return $"unexpected value for {nameof(suggestionTargetArguments)}: {suggestionTargetArguments}";
                }

                return $"package{Environment.NewLine}reference{Environment.NewLine}";
            }
        }

        private class AnonymousSuggestionStore : ISuggestionStore
        {
            private readonly Func<string, string, TimeSpan, string> _getSuggestions;

            public AnonymousSuggestionStore(Func<string, string, TimeSpan, string> getSuggestions)
            {
                _getSuggestions = getSuggestions;
            }

            public string GetCompletions(string exeFileName, string suggestionTargetArguments, TimeSpan timeout)
            {
                return _getSuggestions(exeFileName, suggestionTargetArguments, timeout);
            }
        }
    }
}
