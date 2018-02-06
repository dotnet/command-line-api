using System;
using Xunit;
using Xunit.Abstractions;
using static Microsoft.DotNet.Cli.CommandLine.Create;

namespace Microsoft.DotNet.Cli.CommandLine.Tests
{
    internal class ApiFeedbackTests
    {
        private readonly ITestOutputHelper output;

        public ApiFeedbackTests(ITestOutputHelper output)
        {
            this.output = output;
        }

        [Fact]
        public void Parser_help_for_root_command()
        {
            // var command = Create.RootCommand(
            //     Create.Option("-h|--help|-?", null, Accept.NoArguments())
            // );

            var command = Option("-h|--help|-?", "this is the help text", Accept.NoArguments());

            var parseResult = command.Parse("-?");

            if (parseResult.HasOption("?"))
            {
                var help = parseResult.Command().HelpView();
                output.WriteLine(help);
            }

            // FIX (Parser_help) write test
            throw new NotImplementedException();
        }

        [Fact]
        public void POSIX_separators()
        {
            //  $ diff -iarcu
            //  $ diff -i arcu
            //  $ diff -i=arcu
            //  $ diff -i:arcu

            var command = Command("diff", "",
                                  Option("-i", "", Accept.ExactlyOneArgument()));

            output.WriteLine(command.Parse("diff -iarcu").Diagram());
            output.WriteLine(command.Parse("diff -i arcu").Diagram());
            output.WriteLine(command.Parse("diff -i=arcu").Diagram());
            output.WriteLine(command.Parse("diff -i:arcu").Diagram());

            // FIX (POSIX_separators) write test
            throw new NotImplementedException();
        }
    }
}
