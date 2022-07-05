using System.CommandLine.IO;
using System.CommandLine.Parsing;
using System.Threading.Tasks;
using FluentAssertions;
using Xunit;
using static System.Environment;

namespace System.CommandLine.Tests.Invocation
{
    public class TypoCorrectionTests
    {
        private readonly TestConsole _console = new();

        [Fact]
        public async Task When_option_is_mistyped_it_is_suggested()
        {
            var option = new Option<string>("info");

            var parser =
                new CommandLineBuilder(new RootCommand
                    {
                        option
                    })
                    .UseTypoCorrections()
                    .Build();

            var result = parser.Parse("niof");

            await result.InvokeAsync(_console);

            _console.Out.ToString().Should().Contain($"'niof' was not matched. Did you mean one of the following?{NewLine}info");
        }

        [Fact]
        public async Task When_there_are_no_matches_then_nothing_is_suggested()
        {
            var option = new Option<bool>("info");

            var parser =
                new CommandLineBuilder(new RootCommand
                    {
                        option
                    })
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
                new CommandLineBuilder(new RootCommand
                    {
                        command
                    })
                    .UseTypoCorrections()
                    .Build();

            var result = parser.Parse("sertor");

            await result.InvokeAsync(_console);

            _console.Out.ToString().Should().Contain($"'sertor' was not matched. Did you mean one of the following?{NewLine}restore");
        }

        [Fact]
        public async Task When_there_are_multiple_matches_it_picks_the_best_matches()
        {
            var fromCommand = new Command("from");
            var seenCommand = new Command("seen");
            var aOption = new Option<bool>("a");
            var beenOption = new Option<bool>("been");
            var parser =
                new CommandLineBuilder(new RootCommand
                    {
                        fromCommand,
                        seenCommand,
                        aOption,
                        beenOption
                    })
                    .UseTypoCorrections()
                    .Build();

            var result = parser.Parse("een");

            await result.InvokeAsync(_console);

            _console.Out.ToString().Should().Contain($"'een' was not matched. Did you mean one of the following?{NewLine}seen{NewLine}been");
        }

        [Fact]
        public async Task Hidden_commands_are_not_suggested()
        {
            var fromCommand = new Command("from");
            var seenCommand = new Command("seen") { IsHidden = true };
            var beenCommand = new Command("been");

            var parser =
                new CommandLineBuilder(new RootCommand
                    {
                        fromCommand,
                        seenCommand,
                        beenCommand
                    })
                    .UseTypoCorrections()
                    .Build();

            var result = parser.Parse("een");

            await result.InvokeAsync(_console);

            _console.Out.ToString().Should().Contain($"'een' was not matched. Did you mean one of the following?{NewLine}been");
        }

        [Fact]
        public async Task Arguments_are_not_suggested()
        {
            var argument = new Argument<string>("the-argument");
            var command = new Command("been");

            var parser =
                new CommandLineBuilder(new RootCommand
                    {
                        argument,
                        command
                    })
                    .UseTypoCorrections()
                    .Build();

            var result = parser.Parse("een");

            await result.InvokeAsync(_console);

            _console.Out.ToString().Should().NotContain("the-argument");
        }

        [Fact]
        public async Task Hidden_options_are_not_suggested()
        {
            var fromOption = new Option<string>("from");
            var seenOption = new Option<string>("seen") { IsHidden = true };
            var beenOption = new Option<string>("been");

            var parser =
                new CommandLineBuilder(new RootCommand
                    {
                        fromOption,
                        seenOption,
                        beenOption
                    })
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
                new CommandLineBuilder(new RootCommand
                    {
                        new Option<string>(new[] { "/call", "-call", "--call" }),
                        new Option<string>(new[] { "/email", "-email", "--email" })
                    })
                    .UseTypoCorrections()
                    .Build();
            var result = parser.Parse("-all");

            await result.InvokeAsync(_console);

            _console.Out.ToString().Should().Contain($"'-all' was not matched. Did you mean one of the following?{NewLine}-call");
        }
    }
}