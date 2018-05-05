using System;
using Xunit;
using Xunit.Abstractions;
using static System.CommandLine.Create;

namespace System.CommandLine.Tests
{
    public class ApiFeedbackTests
    {
        private readonly ITestOutputHelper output;

        public ApiFeedbackTests(ITestOutputHelper output)
        {
            this.output = output;
        }

        [Fact(Skip = "sketch")]
        public void Parser_help_for_root_command()
        {
            // var command = Create.RootCommand(
            //     Create.Option("-h|--help|-?", null, Accept.NoArguments())
            // );

            var command = Option("-h|--help|-?", "this is the help text", ArgumentsRule.None);

            var parseResult = command.Parse("-?");

            if (parseResult.HasOption("?"))
            {
                var help = parseResult.Command().HelpView();
                output.WriteLine(help);
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

            var command = Command("diff", "",
                                  Option("-i", "", new ArgumentRuleBuilder().ExactlyOne()));

            output.WriteLine(command.Parse("diff -iarcu").Diagram());
            output.WriteLine(command.Parse("diff -i arcu").Diagram());
            output.WriteLine(command.Parse("diff -i=arcu").Diagram());
            output.WriteLine(command.Parse("diff -i:arcu").Diagram());

            throw new NotImplementedException();
        }
    }
}
