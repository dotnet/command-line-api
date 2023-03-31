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
            CliRootCommand rootCommand = new () 
            {
                new CliOption<string>("info")
            };

            CliConfiguration config = new(rootCommand)
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
            var option = new CliOption<bool>("info");
            CliRootCommand rootCommand = new() { option };

            CliConfiguration configuration = new(rootCommand)
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
            var command = new CliCommand("restore");
            CliRootCommand rootCommand = new() { command };

            CliConfiguration configuration = new(rootCommand)
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
            var fromCommand = new CliCommand("from");
            var seenCommand = new CliCommand("seen");
            var aOption = new CliOption<bool>("a");
            var beenOption = new CliOption<bool>("been");
            CliRootCommand rootCommand = new ()
            {
                fromCommand,
                seenCommand,
                aOption,
                beenOption
            };
            CliConfiguration configuration = new(rootCommand)
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
            var fromCommand = new CliCommand("from");
            var seenCommand = new CliCommand("seen") { Hidden = true };
            var beenCommand = new CliCommand("been");
            CliRootCommand rootCommand = new CliRootCommand
            {
                fromCommand,
                seenCommand,
                beenCommand
            };

            CliConfiguration configuration = new(rootCommand)
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
            var argument = new CliArgument<string>("the-argument");
            var command = new CliCommand("been");
            var rootCommand = new CliRootCommand
            {
                argument,
                command
            };
            CliConfiguration configuration = new(rootCommand)
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
            var fromOption = new CliOption<string>("from");
            var seenOption = new CliOption<string>("seen") { Hidden = true };
            var beenOption = new CliOption<string>("been");
            var rootCommand = new CliRootCommand
            {
                fromOption,
                seenOption,
                beenOption
            };
            CliConfiguration config = new(rootCommand)
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
            var rootCommand = new CliRootCommand
            {
                new CliOption<string>("/call", "-call", "--call"),
                new CliOption<string>("/email", "-email", "--email")
            };
            CliConfiguration config = new(rootCommand)
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