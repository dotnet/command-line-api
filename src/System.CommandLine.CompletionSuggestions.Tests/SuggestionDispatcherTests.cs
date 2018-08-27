// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Generic;
using System.CommandLine.Builder;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using FluentAssertions;
using Xunit;

namespace System.CommandLine.CompletionSuggestions.Tests
{
    internal class TestSuggestionProvider : ISuggestionProvider
    {
        private readonly IReadOnlyCollection<SuggestionRegistration> _findAllRegistrations;
        private readonly SuggestionRegistration _findRegistration;

        public TestSuggestionProvider() : this(
            new SuggestionRegistration("C:\\Program Files\\dotnet\\dotnet.exe", "dotnet complete"))
        { }

        public TestSuggestionProvider(SuggestionRegistration regLine)
        {
            _findRegistration = regLine;
        }

        public TestSuggestionProvider(IReadOnlyCollection<SuggestionRegistration> findAllRegistrations, SuggestionRegistration findRegistration)
        {
            _findAllRegistrations = findAllRegistrations;
            _findRegistration = findRegistration;
        }

        public SuggestionRegistration FindRegistration(FileInfo soughtExecutable) => _findRegistration;
        public IReadOnlyCollection<SuggestionRegistration> FindAllRegistrations() => _findAllRegistrations ?? new SuggestionRegistration[] { _findRegistration };

        public List<SuggestionRegistration> AddedRegistrations { get; } = new List<SuggestionRegistration>();
        public void AddSuggestionRegistration(SuggestionRegistration registration)
        {
            AddedRegistrations.Add(registration);
        }
    }

    public class SuggestionDispatcherTests
    {
        private readonly string[] _args = @"-p 12 -e ""C:\Program Files\dotnet\dotnet.exe"" ""dotnet add""".Tokenize()
            .ToArray();

        [Fact]
        public void Dispatch_executes_dotnet_complete() => Dispatch(_args,
                new TestSuggestionProvider(), 20000)
            .Should()
            .Contain("package")
            .And.Contain("reference");

        [Fact]
        public void Dispatch_with_missing_position_arg_throws()
        {
            Action action = () =>
                Dispatch(
                    @"-e ""C:\Program Files\dotnet\dotnet.exe"" ""dotnet add"" -p".Tokenize().ToArray(),
                    new TestSuggestionProvider());
            action
                .Should()
                .Throw<InvalidOperationException>()
                .WithMessage("Required argument missing for option: -p");
        }

        [Fact]
        public void Dispatch_with_unknown_completion_provider_returns_empty_string() => Dispatch(
                _args,
                new TestSuggestionProvider(null))
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
            TestSuggestionProvider testSuggestionProvider;
            if (RuntimeInformation
                .IsOSPlatform(OSPlatform.Windows))
            {
                testSuggestionProvider = new TestSuggestionProvider(
                    new[] {
                        new SuggestionRegistration(@"C:\\Program Files\\dotnet\\dotnet.exe","dotnet complete"),
                        new SuggestionRegistration(@"C:\\Program Files\\himalayan-berry.exe","himalayan-berry spread")
                    },
                    new SuggestionRegistration(@"C:\\Program Files\\dotnet\\dotnet.exe", "dotnet complete"));
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

            SuggestionDispatcher.GetCompletionAvailableCommands(testSuggestionProvider)
                .Should().Be("dotnet himalayan-berry");
        }

        [Fact]
        public async Task RegisterCommandAddsNewSuggestionEntry()
        {
            var provider = new TestSuggestionProvider();
            var dispatcher = new SuggestionDispatcher(provider);

            await dispatcher.Invoke("register --command-path \"C:\\Windows\\System32\\net.exe\" --suggestion-command \"net-suggestions complete\"".Tokenize().ToArray());

            SuggestionRegistration addedRegistration = provider.AddedRegistrations.Single();
            addedRegistration.CommandPath.Should().Be(@"C:\Windows\System32\net.exe");
            addedRegistration.SuggestionCommand.Should().Be("net-suggestions complete");
        }

        private static string Dispatch(
            string[] args,
            ISuggestionProvider suggestionProvider,
            int timeoutMilliseconds = 2000)
        {
            var dispatcher = new SuggestionDispatcher(suggestionProvider);
            ParseResult parseResult = dispatcher.Parser.Parse(args);
            return SuggestionDispatcher.Dispatch(parseResult,
                                                suggestionProvider,
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
