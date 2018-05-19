using System.CommandLine.Builder;
using Xunit;
using Xunit.Abstractions;

namespace System.CommandLine.Tests
{
    public class ApiFeedbackTests
    {
        private readonly ITestOutputHelper _output;

        public ApiFeedbackTests(ITestOutputHelper output)
        {
            _output = output;
        }

        [Fact(Skip = "sketch")]
        public void Parser_help_for_root_command()
        {
            // var commandDefinition = Create.RootCommandDefinition(
            //     Create.Option("-h|--help|-?", null, Accept.NoArguments())
            // );

            var command = new OptionDefinition(
                new [] {"-h", "--help", "-?"},
                "this is the help text",
                argumentDefinition: ArgumentDefinition.None);

            var parseResult = command.Parse("-?");

            if (parseResult.HasOption("?"))
            {
                var help = parseResult.CommandDefinition().HelpView();
                _output.WriteLine(help);
            }

            throw new NotImplementedException();
        }

        [Fact(Skip = "sketch")]
        public void POSIX_separators()
        {
            //  $ diff -iarcu
            //  $ diff -i arcu
            //  $ diff -i=arcu
            //  $ diff -i:arcu

            var command = new CommandDefinition("diff", "", new[] {
                new OptionDefinition(
                    "-i",
                    "",
                    argumentDefinition: new ArgumentDefinitionBuilder().ExactlyOne())
            });

            _output.WriteLine(command.Parse("diff -iarcu").Diagram());
            _output.WriteLine(command.Parse("diff -i arcu").Diagram());
            _output.WriteLine(command.Parse("diff -i=arcu").Diagram());
            _output.WriteLine(command.Parse("diff -i:arcu").Diagram());

            throw new NotImplementedException();
        }
    }
}
