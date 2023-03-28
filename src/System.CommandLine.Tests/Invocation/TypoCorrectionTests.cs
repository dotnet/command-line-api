using System.IO;
using System.Threading.Tasks;
using FluentAssertions;
using Xunit;
using static System.Environment;

namespace System.CommandLine.Tests.Invocation
{
    public class TypoCorrectionTests
    {
        [Fact]
        public async Task When_option_is_mistyped_it_is_suggested()
        {
            RootCommand rootCommand = new () 
            {
                new Option<string>("info")
            };

            CommandLineConfiguration config = new(rootCommand)
            {
                EnableTypoCorrections = true,
                Output = new StringWriter()
            };

            var result = rootCommand.Parse("niof", config);

            await result.InvokeAsync();

            config.Output.ToString().Should().Contain($"'niof' was not matched. Did you mean one of the following?{NewLine}info");
        }

        [Fact]
        public async Task When_there_are_no_matches_then_nothing_is_suggested()
        {
            var option = new Option<bool>("info");
            RootCommand rootCommand = new() { option };

            CommandLineConfiguration configuration = new(rootCommand)
            {
                EnableTypoCorrections = true,
                Output = new StringWriter()
            };

            var result = rootCommand.Parse("zzzzzzz", configuration);

            await result.InvokeAsync();

            configuration.Output.ToString().Should().NotContain("was not matched");
        }

        [Fact]
        public async Task When_command_is_mistyped_it_is_suggested()
        {
            var command = new Command("restore");
            RootCommand rootCommand = new() { command };

            CommandLineConfiguration configuration = new(rootCommand)
            {
                EnableTypoCorrections = true,
                Output = new StringWriter()
            };

            var result = rootCommand.Parse("sertor", configuration);

            await result.InvokeAsync();

            configuration.Output.ToString().Should().Contain($"'sertor' was not matched. Did you mean one of the following?{NewLine}restore");
        }

        [Fact]
        public async Task When_there_are_multiple_matches_it_picks_the_best_matches()
        {
            var fromCommand = new Command("from");
            var seenCommand = new Command("seen");
            var aOption = new Option<bool>("a");
            var beenOption = new Option<bool>("been");
            RootCommand rootCommand = new ()
            {
                fromCommand,
                seenCommand,
                aOption,
                beenOption
            };
            CommandLineConfiguration configuration = new(rootCommand)
            {
                EnableTypoCorrections = true,
                Output = new StringWriter()
            };

            var result = rootCommand.Parse("een", configuration);

            await result.InvokeAsync();

            configuration.Output.ToString().Should().Contain($"'een' was not matched. Did you mean one of the following?{NewLine}seen{NewLine}been");
        }

        [Fact]
        public async Task Hidden_commands_are_not_suggested()
        {
            var fromCommand = new Command("from");
            var seenCommand = new Command("seen") { IsHidden = true };
            var beenCommand = new Command("been");
            RootCommand rootCommand = new RootCommand
            {
                fromCommand,
                seenCommand,
                beenCommand
            };

            CommandLineConfiguration configuration = new(rootCommand)
            {
                EnableTypoCorrections = true,
                Output = new StringWriter()
            };

            var result = rootCommand.Parse("een", configuration);

            await result.InvokeAsync();

            configuration.Output.ToString().Should().Contain($"'een' was not matched. Did you mean one of the following?{NewLine}been");
        }

        [Fact]
        public async Task Arguments_are_not_suggested()
        {
            var argument = new Argument<string>("the-argument");
            var command = new Command("been");
            var rootCommand = new RootCommand
            {
                argument,
                command
            };
            CommandLineConfiguration configuration = new(rootCommand)
            {
                EnableTypoCorrections = true,
                EnableParseErrorReporting = false,
                Output = new StringWriter()
            };

            var result = rootCommand.Parse("een", configuration);

            await result.InvokeAsync();

            configuration.Output.ToString().Should().NotContain("the-argument");
        }

        [Fact]
        public async Task Hidden_options_are_not_suggested()
        {
            var fromOption = new Option<string>("from");
            var seenOption = new Option<string>("seen") { IsHidden = true };
            var beenOption = new Option<string>("been");
            var rootCommand = new RootCommand
            {
                fromOption,
                seenOption,
                beenOption
            };
            CommandLineConfiguration config = new(rootCommand)
            {
                EnableTypoCorrections = true,
                Output = new StringWriter()
            };

            var result = rootCommand.Parse("een", config);

            await result.InvokeAsync();

            config.Output.ToString().Should().Contain($"'een' was not matched. Did you mean one of the following?{NewLine}been");
        }

        [Fact]
        public async Task Suggestions_favor_matches_with_prefix()
        {
            var rootCommand = new RootCommand
            {
                new Option<string>("/call", "-call", "--call"),
                new Option<string>("/email", "-email", "--email")
            };
            CommandLineConfiguration config = new(rootCommand)
            {
                EnableTypoCorrections = true,
                Output = new StringWriter()
            };
            var result = rootCommand.Parse("-all", config);

            await result.InvokeAsync();

            config.Output.ToString().Should().Contain($"'-all' was not matched. Did you mean one of the following?{NewLine}-call");
        }
    }
}