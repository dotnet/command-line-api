using System.CommandLine.Builder;
using System.CommandLine.Invocation;
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

        [Fact]
        public void UseChainedCommandLineParsing_executes_all_chained_commands_correctly()
        {
            var results = new (string s, bool b)[3];
            var rootCommand = new RootCommand();
            
            var command1 = new Command("command1")
            {
                new Option<string>("--c1-string"),
                new Option<bool>("--c1-bool")
            };
            command1.Handler = CommandHandler.Create((string c1String, bool c1Bool) =>
            {
                results[0] = (c1String, c1Bool);
            });
            rootCommand.Add(command1);
            
            var command2 = new Command("command2")
            {
                new Option<string>("--c2-string"),
                new Option<bool>("--c2-bool")
            };
            command2.Handler = CommandHandler.Create((string c2String, bool c2Bool) =>
            {
                results[1] = (c2String, c2Bool);
            });
            rootCommand.Add(command2);
            
            var command3 = new Command("command3")
            {
                new Option<string>("--c3-string"),
                new Option<bool>("--c3-bool")
            };
            command3.Handler = CommandHandler.Create((string c3String, bool c3Bool) =>
            {
                results[2] = (c3String, c3Bool);
            });
            rootCommand.Add(command3);

            var parser = new CommandLineBuilder(rootCommand)
                .UseDefaults()
                .UseChainedCommandLineParsing()
                .Build();

            parser.Invoke(
                "command1 --c1-string string1 --c1-bool -- command2 --c2-string string2 --c2-bool true -- command3 --c3-string string3 --c3-bool false");

            results[0].Should().Be(("string1", true));
            results[1].Should().Be(("string2", true));
            results[2].Should().Be(("string3", false));
        }
    }
}
