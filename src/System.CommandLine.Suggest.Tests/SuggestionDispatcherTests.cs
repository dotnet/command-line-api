// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

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
        private static readonly string _currentExeName = RootCommand.ExeName;

        private static RegistrationPair CurrentExeRegistrationPair()
            => new RegistrationPair(CurrentExeFullPath(), $"{_currentExeName} [suggest]");

        private static string CurrentExeFullPath() => Path.GetFullPath(_currentExeName);

        [Fact]
        public async Task InvokeAsync_executes_registered_executable()
        {
            string receivedTargetExeName = null;

            string[] args = $@"get -p 12 -e ""{CurrentExeFullPath()}"" -- {_currentExeName} add".Tokenize().ToArray();

            var suggestions = await InvokeAsync(
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

            var args = PrepareArgs($@"get -p 12 -e ""{CurrentExeFullPath()}"" -- {_currentExeName} add");

            await InvokeAsync(
                args.args,
                new TestSuggestionRegistration(CurrentExeRegistrationPair()),
                new AnonymousSuggestionStore(
                    (targetExeName, targetExeArgs, _) =>
                    {
                        receivedTargetExeArgs = targetExeArgs;

                        return "";
                    }));

            receivedTargetExeArgs.Should().Be("[suggest:12] add");
        }

        private static (string[] args, int cursorPosition) PrepareArgs(string args)
        {
            var formattableString = args.Replace("$", "");
            var cursorPosition = args.IndexOf("$");
            return (formattableString.Tokenize().ToArray(), cursorPosition);
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
        public async Task List_command_gets_all_executable_names()
        {
            var testSuggestionProvider = new TestSuggestionRegistration(
                new RegistrationPair(_dotnetExeFullPath, "dotnet complete"),
                new RegistrationPair(_himalayanBerryExeFullPath, "himalayan-berry spread"));

            var dispatcher = new SuggestionDispatcher(testSuggestionProvider);
            var testConsole = new TestConsole();

            await dispatcher.InvokeAsync(new[] { "list" }, testConsole);

            testConsole.Out.ToString().Should().Be($"dotnet himalayan-berry{Environment.NewLine}");
        }

        private static readonly string _dotnetExeFullPath =
            RuntimeInformation.IsOSPlatform(OSPlatform.Windows)
                ? @"C:\Program Files\dotnet\dotnet.exe"
                : "/bin/dotnet";

        private static readonly string _netExeFullPath =
            RuntimeInformation.IsOSPlatform(OSPlatform.Windows)
                ? @"C:\Windows\System32\net.exe"
                : "/bin/net";

        private static readonly string _himalayanBerryExeFullPath =
            RuntimeInformation.IsOSPlatform(OSPlatform.Windows)
                ? @"C:\Program Files\himalayan-berry.exe"
                : "/bin/himalayan-berry";

        [Fact]
        public async Task Register_command_adds_new_suggestion_entry()
        {
            var provider = new TestSuggestionRegistration();
            var dispatcher = new SuggestionDispatcher(provider);

            var args = $"register --command-path \"{_netExeFullPath}\" --suggestion-command \"net-suggestions complete\"".Tokenize().ToArray();

            await dispatcher.InvokeAsync(args);

            RegistrationPair addedRegistration = provider.FindAllRegistrations().Single();
            addedRegistration.CommandPath.Should().Be(_netExeFullPath);
            addedRegistration.SuggestionCommand.Should().Be("net-suggestions complete");
        }

        [Fact]
        public async Task Register_command_will_not_add_duplicate_entry()
        {
            var provider = new TestSuggestionRegistration();
            var dispatcher = new SuggestionDispatcher(provider);

            var args = $"register --command-path \"{_netExeFullPath}\" --suggestion-command \"net-suggestions complete\"".Tokenize().ToArray();

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
            private Func<string, string, TimeSpan, string> _getSuggestions;

            public AnonymousSuggestionStore(Func<string, string, TimeSpan, string> getSuggestions)
            {
                _getSuggestions = getSuggestions;
            }

            public string GetSuggestions(string exeFileName, string suggestionTargetArguments, TimeSpan timeout)
            {
                return _getSuggestions(exeFileName, suggestionTargetArguments, timeout);
            }
        }
    }
}
