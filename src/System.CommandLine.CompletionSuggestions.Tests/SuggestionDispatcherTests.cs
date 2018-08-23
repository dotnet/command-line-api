// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Generic;
using System.CommandLine.Builder;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using FluentAssertions;
using Xunit;

namespace System.CommandLine.CompletionSuggestions.Tests
{
    internal class TestSuggestionFileProvider : ISuggestionFileProvider
    {
        private readonly IReadOnlyCollection<string> _findAllRegistrations;
        private readonly string _findRegistration;

        public TestSuggestionFileProvider() : this("C:\\Program Files\\dotnet\\dotnet.exe=dotnet complete")
        {
        }

        public TestSuggestionFileProvider(string regLine)
        {
            _findRegistration = regLine;
        }

        public TestSuggestionFileProvider(IReadOnlyCollection<string> findAllRegistrations, string findRegistration)
        {
            _findAllRegistrations = findAllRegistrations;
            _findRegistration = findRegistration;
        }

        public IReadOnlyCollection<string> RegistrationConfigurationFilePaths => new string[] { };
        public void AddRegistrationConfigurationFilePath(string configFilePath) => throw new NotImplementedException();

        public string FindRegistration(FileInfo soughtExecutable) => _findRegistration;
        public IReadOnlyCollection<string> FindAllRegistrations() => _findAllRegistrations ?? new string[] {_findRegistration};
    }

    public class SuggestionDispatcherTests
    {
        private readonly string[] _args = @"-p 12 -e ""C:\Program Files\dotnet\dotnet.exe"" ""dotnet add""".Tokenize()
            .ToArray();

        [Fact]
        public void Dispatch_executes_dotnet_complete() => Dispatch(_args,
                new TestSuggestionFileProvider(), 20000)
            .Should()
            .Contain("package")
            .And.Contain("reference");

        [Fact]
        public void Dispatch_with_badly_formatted_completion_provider_throws()
        {
            Action action = () => Dispatch(_args, new TestSuggestionFileProvider("foo^^bar"));
            action
                .Should()
                .Throw<FormatException>()
                .WithMessage("Syntax for configuration of 'foo^^bar' is not of the format '<command>=<value>'");
        }

        [Fact]
        public void Dispatch_with_missing_position_arg_throws()
        {
            Action action = () =>
                Dispatch(
                    @"-e ""C:\Program Files\dotnet\dotnet.exe"" ""dotnet add"" -p".Tokenize().ToArray(),
                    new TestSuggestionFileProvider());
            action
                .Should()
                .Throw<InvalidOperationException>()
                .WithMessage("Required argument missing for option: -p");
        }

        [Fact]
        public void Dispatch_with_unknown_completion_provider_returns_empty_string() => Dispatch(
                _args,
                new TestSuggestionFileProvider(String.Empty))
            .Should()
            .BeEmpty();

        [Fact]
        public void GetCompletionSuggestions_executes_dotnet_complete() =>
            SuggestionDispatcher
                .GetSuggestions("dotnet", "complete --position 12 \"dotnet add\"", 20000)
                .Should()
                .Contain("-h")
                .And.Contain("--help")
                .And.Contain("package")
                .And.Contain("reference");

        [Fact]
        public void GetCompletionSuggestions_withbogusfilename_throws_FileNotFound()
        {
            string exeFileName = "Bogus file name";
            Action action = () =>
                SuggestionDispatcher.GetSuggestions(exeFileName, "");
            action
                .Should()
                .Throw<ArgumentException>("System.Diagnostics.Process is nuts.")
                .Where(exception => exception.Message.Contains(
                    $"Unable to find the file '{ exeFileName }'"));
        }

        [Fact]
        public void GetCompletionSuggestions_UseProcessThatRemainsOpen_ReturnsEmptyString()
        {
            SuggestionDispatcher.GetSuggestions(
                    "dotnet"
                    , suggestionTargetArguments: $"{Assembly.GetExecutingAssembly().Location}", millisecondsTimeout: 1)
                .Should().BeEmpty();
        }

        [Fact]
        public void GetCompletionAvailableCommands_GetsAllExecutableNames()
        {
            TestSuggestionFileProvider testSuggestionProvider;
            if (RuntimeInformation
                .IsOSPlatform(OSPlatform.Windows))
            {
                testSuggestionProvider = new TestSuggestionFileProvider(
                    new[] {
                        @"C:\\Program Files\\dotnet\\dotnet.exe=dotnet complete",
                        @"C:\\Program Files\\himalayan-berry.exe=himalayan-berry spread"
                    },
                    @"C:\\Program Files\\dotnet\\dotnet.exe=dotnet complete");
            }
            else
            {
                testSuggestionProvider = new TestSuggestionFileProvider(
                    new[] {
                        @"/bin/dotnet=dotnet complete",
                        @"/bin/himalayan-berry=himalayan-berry spread"
                    },
                    @"/bin/dotnet=dotnet complete");
            }

            SuggestionDispatcher.GetCompletionAvailableCommands(testSuggestionProvider)
                .Should().Be("dotnet himalayan-berry");
        }

        private static string Dispatch(
            string[] args,
            ISuggestionFileProvider suggestionFileProvider,
            int timeoutMilliseconds = 2000)
        {
            ParseResult parseResult = SuggestionDispatcher.Parser.Parse(args);

            return SuggestionDispatcher.Dispatch(parseResult,
                                                suggestionFileProvider,
                                                GetSuggestionsSimulator,
                                                timeoutMilliseconds);
        }

        private static string GetSuggestionsSimulator(string exeFileName,
                                            string suggestionTargetArguments,
                                            int millisecondsTimeout = 5000)
        {
            var parser = new CommandLineBuilder("dotnet")
                            .AddCommand("add", "add description",
                                    symbols: s => s.AddCommand("package", "package description")
                                                   .AddCommand("reference", "reference description"))
                           .AddCommand("complete",
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
