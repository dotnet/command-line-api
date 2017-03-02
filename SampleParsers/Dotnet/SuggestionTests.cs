using FluentAssertions;
using Xunit;
using static CommandLine.SampleParsers.Dotnet.Create;

namespace CommandLine.SampleParsers.Dotnet
{
    public class SuggestionTests
    {
        [Fact]
        public void dotnet_add()
        {
            var result = DotnetCommand().Parse("dotnet add ");
            result.Suggestions()
                  .Should()
                  .BeEquivalentTo("reference", "package", "-h", "--help");
        }

        [Fact(Skip="Bug")]
        public void dotnet_sln_add()
        {
            var command = DotnetCommand();

            var result = command.Parse("dotnet sln add ");

            result.Suggestions()
                  .Should()
                  .BeEquivalentTo("-h", "--help");
        }
    }
}