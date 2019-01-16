using System.CommandLine.Builder;
using System.CommandLine.Invocation;
using System.Threading.Tasks;
using FluentAssertions;
using Xunit;

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

            await parser.InvokeAsync(result, _console);

            _console.Out.ToString().Should().Contain("'niof' was not matched, did you mean 'info'?");
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

            await parser.InvokeAsync(result, _console);

            _console.Out.ToString().Should().Contain("'sertor' was not matched, did you mean 'restore'?");
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

            await parser.InvokeAsync(result, _console);

            _console.Out.ToString().Should().Contain("'een' was not matched, did you mean 'seen', or 'been'?");
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

            await parser.InvokeAsync(result, _console);

            _console.Out.ToString().Should().Contain("'een' was not matched, did you mean 'been'?");
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

            await parser.InvokeAsync(result, _console);

            _console.Out.ToString().Should().Contain("'een' was not matched, did you mean 'been'?");
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

            await parser.InvokeAsync(result, _console);

            _console.Out.ToString().Should().Contain("'-all' was not matched, did you mean '-call'?");
        }
    }
}
