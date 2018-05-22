using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Reflection;
using FluentAssertions;
using Xunit;

namespace System.CommandLine.CompletionSuggestions.Tests
{
    internal class TestSuggestionFileProvider : ISuggestionFileProvider
    {
        private readonly string _regLine;

        public TestSuggestionFileProvider() : this("C:\\Program Files\\dotnet\\dotnet.exe=dotnet complete")
        {
        }

        public TestSuggestionFileProvider(string regLine)
        {
            _regLine = regLine;
        }

        public IReadOnlyCollection<string> RegistrationConfigurationFilePaths => new string[] { };
        public void AddRegistrationConfigurationFilePath(string configFilePath) => throw new NotImplementedException();

        public string FindRegistration(FileInfo soughtExecutable) => _regLine;
    }

    public class SuggestionDispatcherTests
    {
        private readonly string[] _args = @"-p 12 -e ""C:\Program Files\dotnet\dotnet.exe"" ""dotnet add""".Tokenize()
            .ToArray();

        [Fact]
        public void Dispatch_executes_dotnet_complete() => SuggestionDispatcher.Dispatch(_args,
                new TestSuggestionFileProvider())
            .Should()
            .Contain("-h")
            .And.Contain("--help")
            .And.Contain("package")
            .And.Contain("reference");

        [Fact]
        public void Dispatch_with_badly_formatted_completion_provider_throws()
        {
            Action action = () => SuggestionDispatcher.Dispatch(_args, new TestSuggestionFileProvider("foo^^bar"));
            action
                .Should()
                .Throw<FormatException>()
                .WithMessage("Syntax for configuration of 'foo^^bar' is not of the format '<command>=<value>'");
        }

        [Fact]
        public void Dispatch_with_missing_position_arg_throws()
        {
            Action action = () =>
                SuggestionDispatcher.Dispatch(
                    @"-e ""C:\Program Files\dotnet\dotnet.exe"" ""dotnet add"" -p".Tokenize().ToArray(),
                    new TestSuggestionFileProvider());
            action
                .Should()
                .Throw<InvalidOperationException>()
                .WithMessage("Required argument missing for option: -p");
        }

        [Fact]
        public void Dispatch_with_unknown_completion_provider_returns_empty_string() => SuggestionDispatcher.Dispatch(
                _args,
                new TestSuggestionFileProvider(string.Empty))
            .Should()
            .BeEmpty();

        [Fact]
        public void GetCompletionSuggestions_executes_dotnet_complete() => SuggestionDispatcher
            .GetSuggestions("dotnet", "complete --position 12 \"dotnet add\"")
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
    }
}
