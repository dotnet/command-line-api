using FluentAssertions;
using Xunit;
using static Microsoft.DotNet.Cli.CommandLine.SampleParsers.Dotnet.Create;

namespace Microsoft.DotNet.Cli.CommandLine.SampleParsers.Dotnet
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