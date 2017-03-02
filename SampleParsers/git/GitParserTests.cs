using System;
using FluentAssertions;
using Xunit;

namespace Microsoft.DotNet.Cli.CommandLine.SampleParsers.git
{
    public class GitParserTests
    {
        private readonly string[] git_status_help = root.HelpText.Split(new[] { '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries);

        [Fact]
        public void HelpText_describes_configured_options()
        {
            var parser = Create.Parser();

            var helpText = parser.DefinedOptions["git"]["status"].HelpView();

            var helpTextLines = helpText
                .Split(new[] { '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries);

            helpTextLines.Should().HaveCount(git_status_help.Length);

            for (var i = 0; i < git_status_help.Length; i++)
            {
                var helpLine = helpTextLines[i];
                helpLine.Should().Be(git_status_help[i]);
            }
        }
    }
}