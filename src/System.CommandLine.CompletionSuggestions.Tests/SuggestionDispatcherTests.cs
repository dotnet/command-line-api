using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using FluentAssertions;
using Xunit;

namespace System.CommandLine.CompletionSuggestions.Tests
{
    internal class TestCompletionFileProvider : ICompletionFileProvider
    {
        private readonly string _regLine;

        public TestCompletionFileProvider() : this("C:\\Program Files\\dotnet\\dotnet.exe=dotnet complete")
        {
        }

        public TestCompletionFileProvider(string regLine)
        {
            _regLine = regLine;
        }

        public IReadOnlyCollection<string> RegistrationConfigFilePaths => new string[] { };
        public void AddRegistrationConfigFilePath(string configFilePath) => throw new NotImplementedException();

        public string FindCompletionRegistration(FileInfo soughtExecutible) => _regLine;
    }

    public class SuggestionDispatcherTests
    {
        private readonly string[] _args = @"-p 12 -e ""C:\Program Files\dotnet\dotnet.exe"" ""dotnet add""".Tokenize()
            .ToArray();

        [Fact]
        public void Dispatch_executes_dotnet_complete() => SuggestionDispatcher.Dispatch(_args,
                new TestCompletionFileProvider())
            .Should()
            .Be(@"-h
--help
package
reference
");

        [Fact]
        public void Dispatch_with_badly_formatted_completion_provider_throws()
        {
            Action act = () => SuggestionDispatcher.Dispatch(_args, new TestCompletionFileProvider("foo^^bar"));
            act
                .Should()
                .Throw<FormatException>()
                .WithMessage("Syntax for configuration of 'foo^^bar' is not of the format '<command>=<value>'");
        }

        [Fact]
        public void Dispatch_with_missing_position_arg_throws()
        {
            Action act = () =>
                SuggestionDispatcher.Dispatch(
                    @"-e ""C:\Program Files\dotnet\dotnet.exe"" ""dotnet add"" -p".Tokenize().ToArray(),
                    new TestCompletionFileProvider());
            act
                .Should()
                .Throw<InvalidOperationException>()
                .WithMessage("Required argument missing for option: -p");
        }

        [Fact]
        public void Dispatch_with_unknown_completion_provider_returns_empty_string() => SuggestionDispatcher.Dispatch(
                _args,
                new TestCompletionFileProvider(string.Empty))
            .Should()
            .BeEmpty();

        [Fact]
        public void GetCompletionSuggestions_executes_dotnet_complete() => SuggestionDispatcher
            .GetCompletionSuggestions("dotnet", "complete --position 12 \"dotnet add\"")
            .Should()
            .Contain("-h")
            .And.Contain("--help")
            .And.Contain("package")
            .And.Contain("reference");

        [Fact]
        public void GetCompletionSuggestions_withbogusfilename_throws_FileNotFound()
        {
            Action act = () =>
                SuggestionDispatcher.GetCompletionSuggestions("Bogus file name", "");
            act
                .Should()
                .Throw<Win32Exception>("System.Diagnostics.Process is nuts.")
                .WithMessage("The system cannot find the file specified");
        }
    }
}
