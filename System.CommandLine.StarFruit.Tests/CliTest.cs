using System.CommandLine.StarFruit;
using System.CommandLine.Builder;
using FluentAssertions;
using Xunit;


namespace System.CommandLine.StarFruit.Tests
{
    public class CliTests
    {
        [Fact]
        public void CanLoadCli()
        {
            var def = StarFruitBuilder.Build<Dotnet>();
            var help = def.HelpView();
            help.Should().NotBeNull();
            help = def.Subcommand("Sln").HelpView();
            help.Should().NotBeNull();
            help = def.Subcommand("New").HelpView();
            help.Should().NotBeNull();
        }
    }


}
