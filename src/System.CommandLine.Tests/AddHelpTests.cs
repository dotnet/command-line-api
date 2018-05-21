using System.CommandLine.Builder;
using System.CommandLine.Invocation;
using System.IO;
using System.Text;
using FluentAssertions;
using Xunit;

namespace System.CommandLine.Tests
{
    public class AddHelpTests
    {
        [Fact]
        public void AddHelp_writes_help_for_the_specified_command()
        {
            var parser =
                new ParserBuilder()
                    .AddCommand("command", "",
                                command => command.AddCommand("subcommand"))
                    .AddHelp()
                    .Build();

            var result = parser.Parse("command subcommand --help");

            var sb = new StringBuilder();
            using (var output = new StringWriter(sb))
            {
                result.Invoke(output);
            }

            sb.ToString().Should().StartWith("Usage: command subcommand");
        }

        [Fact]
        public void AddHelp_interrupts_execution_of_the_specified_command()
        {
            var wasCalled = false;

            var parser =
                new ParserBuilder()
                    .AddCommand("command", "",
                                command => command.AddCommand("subcommand")
                                                  .OnExecute<string>(_ => {
                                                      wasCalled = true;
                                                  }, "a"))
                    .AddHelp()
                    .Build();

            var result = parser.Parse("command subcommand --help");

            result.Invoke();

            wasCalled.Should().BeFalse();
        }

        [Fact]
        public void AddHelp_allows_help_for_all_configured_prefixes()
        {
            var parser =
                new ParserBuilder()
                    .AddCommand("command", "")
                    .AddHelp()
                    .UsePrefixes(new[] { "~" })
                    .Build();

            var result = parser.Parse("command ~help");

            var sb = new StringBuilder();
            using (var output = new StringWriter(sb))
            {
                result.Invoke(output);
            }

            string helpView = sb.ToString();
            helpView.Should().StartWith("Usage:");
        }

        [Theory]
        [InlineData("-h")]
        [InlineData("--help")]
        [InlineData("-?")]
        [InlineData("/?")]
        public void AddHelp_accepts_default_values(string value)
        {
            var parser =
                new ParserBuilder()
                    .AddCommand("command", "")
                    .AddHelp()
                    .Build();

            var result = parser.Parse($"command {value}");

            var sb = new StringBuilder();
            using (var output = new StringWriter(sb))
            {
                result.Invoke(output);
            }

            string helpView = sb.ToString();
            helpView.Should().StartWith("Usage:");
        }

        [Fact]
        public void AddHelp_accepts_collection_of_help_options()
        {
            var parser =
                new ParserBuilder()
                    .AddCommand("command", "")
                    .AddHelp(new[] { "~cthulhu" })
                    .Build();

            var result = parser.Parse("command ~cthulhu");

            var sb = new StringBuilder();
            using (var output = new StringWriter(sb))
            {
                result.Invoke(output);
            }

            string helpView = sb.ToString();
            helpView.Should().StartWith("Usage:");
        }

        [Fact]
        public void AddHelp_does_not_display_when_option_defined_with_same_alias()
        {
            var parser =
                new ParserBuilder()
                    .AddCommand("command", "",
                                cmd => cmd.AddOption("-h"))
                    .AddHelp()
                    .Build();

            var result = parser.Parse("command -h");

            var sb = new StringBuilder();
            using (var output = new StringWriter(sb))
            {
                result.Invoke(output);
            }

            string helpView = sb.ToString();
            helpView.Should().BeEmpty();
        }
    }
}
