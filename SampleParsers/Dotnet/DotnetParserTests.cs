using FluentAssertions;
using Xunit;
using static System.Console;
using static CommandLine.SampleParsers.Dotnet.Create;

namespace CommandLine.SampleParsers.Dotnet
{
    public class AddReferenceTests
    {
        [Fact]
        public void dotnet_add_reference_correctly_assigns_arguments_to_subcommands()
        {
            var result = DotnetCommand().Parse("dotnet add foo.csproj reference bar1.csproj bar2.csproj");

            WriteLine(result.Diagram());

            result["dotnet"]["add"]
                .Arguments
                .Should()
                .BeEquivalentTo("foo.csproj");

            result["dotnet"]["add"]["reference"]
                .Arguments
                .Should()
                .BeEquivalentTo("bar1.csproj", "bar2.csproj");
        }


    }
}