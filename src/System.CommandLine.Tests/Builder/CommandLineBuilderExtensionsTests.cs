using System.CommandLine.Builder;
using System.CommandLine.Parsing;
using FluentAssertions;
using Xunit;

namespace System.CommandLine.Tests.Builder
{
    public class CommandLineBuilderExtensionsTests
    {
        [Fact]
        public void Global_options_are_added_to_the_root_command()
        {
            var globalOption = new Option("global");
            var builder = new CommandLineBuilder()
                .AddGlobalOption(globalOption);

            Parser parser = builder.Build();

            Command rootCommand = (Command)parser.Configuration.RootCommand;
            rootCommand.GlobalOptions
                .Should()
                .Contain(globalOption);
        }
    }
}
