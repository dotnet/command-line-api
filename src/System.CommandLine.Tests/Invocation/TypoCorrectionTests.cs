using FluentAssertions;
using System.CommandLine.Builder;
using System.CommandLine.IO;
using System.CommandLine.Parsing;
using System.Threading.Tasks;
using Xunit;
using static System.Environment;

namespace System.CommandLine.Tests.Invocation
{
    public class TypoCorrectionTests
    {
        private readonly TestConsole _console = new TestConsole();

        [Fact]
        public async Task When_option_is_mistyped_it_is_suggested()
        {
            var option = new Option("info");

            var parser =
                new CommandLineBuilder()
                    .AddOption(option)
                    .UseTypoCorrections()
                    .Build();

            var result = parser.Parse("niof");

            await result.InvokeAsync(_console);

            _console.Out.ToString().Should().Contain($"'niof' was not matched. Did you mean one of the following?{NewLine}info");
        }

        [Fact]
        public async Task When_there_are_no_matches_then_nothing_is_suggested()
        {
            var option = new Option("info");

            var parser =
                new CommandLineBuilder()
                    .AddOption(option)
                    .UseTypoCorrections()
                    .Build();

            var result = parser.Parse("zzzzzzz");

            await result.InvokeAsync(_console);

            _console.Out.ToString().Should().NotContain("was not matched");
        }

        [Fact]
        public async Task When_command_is_mistyped_it_is_suggested()
        {
            var command = new Command("restore");

            var parser =
                new CommandLineBuilder()
                    .AddCommand(command)
                    .UseTypoCorrections()
                    .Build();

            var result = parser.Parse("sertor");

            await result.InvokeAsync(_console);

            _console.Out.ToString().Should().Contain($"'sertor' was not matched. Did you mean one of the following?{NewLine}restore");
        }

        [Fact]
        public async Task When_there_are_multiple_matches_it_picks_the_best_matches()
        {
            var parser =
                new CommandLineBuilder()
                    .AddCommand(new Command("from"))
                    .AddCommand(new Command("seen"))
                    .AddOption(new Option("a"))
                    .AddOption(new Option("been"))
                    .UseTypoCorrections()
                    .Build();

            var result = parser.Parse("een");

            await result.InvokeAsync(_console);

            _console.Out.ToString().Should().Contain($"'een' was not matched. Did you mean one of the following?{NewLine}seen{NewLine}been");
        }

        [Fact]
        public async Task Hidden_commands_are_not_suggested()
        {
            var parser =
                new CommandLineBuilder()
                    .AddCommand(new Command("from"))
                    .AddCommand(new Command("seen") { IsHidden = true })
                    .AddCommand(new Command("been"))
                    .UseTypoCorrections()
                    .Build();

            var result = parser.Parse("een");

            await result.InvokeAsync(_console);

            _console.Out.ToString().Should().Contain($"'een' was not matched. Did you mean one of the following?{NewLine}been");
        }

        [Fact]
        public async Task Arguments_are_not_suggested()
        {
            var parser =
                new CommandLineBuilder()
                    .AddArgument(new Argument("the-argument"))
                    .AddCommand(new Command("been"))
                    .UseTypoCorrections()
                    .Build();

            var result = parser.Parse("een");

            await result.InvokeAsync(_console);

            _console.Out.ToString().Should().NotContain("the-argument");
        }

        [Fact]
        public async Task Hidden_options_are_not_suggested()
        {
            var parser =
                new CommandLineBuilder()
                    .AddOption(new Option("from"))
                    .AddOption(new Option("seen") { IsHidden = true })
                    .AddOption(new Option("been"))
                    .UseTypoCorrections()
                    .Build();
            var result = parser.Parse("een");

            await result.InvokeAsync(_console);

            _console.Out.ToString().Should().Contain($"'een' was not matched. Did you mean one of the following?{NewLine}been");
        }

        [Fact]
        public async Task Suggestions_favor_matches_with_prefix()
        {
            var parser =
                new CommandLineBuilder()
                    .AddOption(new Option(new [] {"/call", "-call", "--call"}))
                    .AddOption(new Option(new [] {"/email", "-email", "--email"}))
                    .UseTypoCorrections()
                    .Build();
            var result = parser.Parse("-all");

            await result.InvokeAsync(_console);

            _console.Out.ToString().Should().Contain($"'-all' was not matched. Did you mean one of the following?{NewLine}-call");
        }
    }
}
