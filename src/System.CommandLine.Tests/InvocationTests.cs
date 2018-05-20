using System.CommandLine.Builder;
using System.IO;
using System.Text;
using FluentAssertions;
using Xunit;

namespace System.CommandLine.Tests
{
    public class InvocationTests
    {
        [Fact]
        public void General_invocation_behaviors_can_be_specified_in_the_parser_definition()
        {
            var wasCalled = false;

            var parser =
                new ParserBuilder()
                    .AddCommand("command", "")
                    .AddInvocation(InvokeMe)
                    .Build();

            var result = parser.Parse("command");

            result.Invoke();

            wasCalled.Should().BeTrue();

            void InvokeMe(InvocationContext context)
            {
                wasCalled = true;
            }
        }

        [Fact]
        public void First_invocation_behavior_to_set_a_result_short_circuits()
        {
            var wasCalled = false;

            var parser =
                new ParserBuilder()
                    .AddCommand("command", "")
                    .AddInvocation(InvokeMe1)
                    .AddInvocation(InvokeMe2)
                    .Build();

            var result = parser.Parse("command");

            result.Invoke();

            wasCalled.Should().BeFalse();

            void InvokeMe1(InvocationContext context)
            {
                context.InvocationResult = new TestInvocationResult();
            }

            void InvokeMe2(InvocationContext context)
            {
                wasCalled = true;
            }
        }

        [Fact]
        public void Specific_invocation_behavior_can_be_specified_in_the_command_definition()
        {
            var wasCalled = false;

            var commandDefinition =
                new ParserBuilder()
                    .AddCommand(
                        "command", "",
                        cmd => cmd.OnExecute(InvokeMe))
                    .BuildCommandDefinition();

            var result = commandDefinition.Parse("command");

            result.Invoke();

            wasCalled.Should().BeTrue();

            void InvokeMe()
            {
                wasCalled = true;
            }
        }

        [Fact]
        public void Specific_invocation_behavior_can_be_specified_in_the_command_definition_with_argument()
        {
            var wasCalled = false;

            var commandDefinition =
                new ParserBuilder()
                    .AddCommand(
                        "command", "",
                        cmd => cmd.AddOption("-name", "", a => a.ExactlyOne())
                            .OnExecute<string>(InvokeMe, "-name"))
                    .BuildCommandDefinition();

            var result = commandDefinition.Parse("command -name hello");

            result.Invoke();

            wasCalled.Should().BeTrue();
            

            void InvokeMe(string name)
            {
                wasCalled = true;
                name.Should().Be("hello");
            }
        }

        [Fact]
        public void Specific_invocation_behaviors_can_be_specified_in_the_command_definition_in_any_order()
        {
            var wasCalled = false;

            var commandDefinition =
                new ParserBuilder()
                    .AddCommand(
                        "command", "",
                        cmd => cmd
                            .AddOption("-name", "", a => a.ExactlyOne())
                            .OnExecute<string, int>(InvokeMe, "-name", "-age")
                            .AddOption("-age", "", a => a.ParseArgumentsAs<int>())
                        )
                    .BuildCommandDefinition();

            var result = commandDefinition.Parse("command -age 425 -name Gandalf");

            result.Invoke();

            wasCalled.Should().BeTrue();

            void InvokeMe(string name, int age)
            {
                wasCalled = true;
                name.Should().Be("Gandalf");
                age.Should().Be(425);
            }
        }

        //[Fact]
        public void Invoke_chooses_the_appropriate_command()
        {
            // FIX (Invoke_chooses_the_appropriate_command) write test
            throw new NotImplementedException();
        }

        [Fact]
        public void Invocation_pipeline_can_be_short_circuited_by_the_presence_of_an_option()
        {
            // dotnet add -h

            var parser =
                new ParserBuilder()
                    .AddCommand("command", "")
                    .AddHelp()
                    .Build();

            var result = parser.Parse("command --help");

            var sb = new StringBuilder();
            using (var output = new StringWriter(sb))
            {                result.Invoke(output);
            }

            string helpView = sb.ToString();
            helpView.Should().StartWith("Usage:");
        }

        [Fact]
        public void Add_help_allows_help_for_all_configured_prefixes()
        {
            // dotnet add -h

            var parser =
                new ParserBuilder()
                    .AddCommand("command", "")
                    .AddHelp()
                    .UsePrefixes(new[] {"~"})
                    .Build();
            
            var result = parser.Parse("command ~help");

            var sb = new StringBuilder();
            using (var output = new StringWriter(sb))
            {
                result.Invoke(output);
            }

            string helpView = sb.ToString();
            helpView.Should().StartWith("Usage:");
        }

        [Theory]
        [InlineData("-h")]
        [InlineData("--help")]
        [InlineData("-?")]
        [InlineData("/?")]
        public void Add_help_accepts_default_values(string value)
        {
            var parser =
                new ParserBuilder()
                    .AddCommand("command", "")
                    .AddHelp()
                    .Build();

            var result = parser.Parse($"command {value}");

            var sb = new StringBuilder();
            using (var output = new StringWriter(sb))
            {
                result.Invoke(output);
            }

            string helpView = sb.ToString();
            helpView.Should().StartWith("Usage:");
        }

        [Fact]
        public void Add_help_accepts_collection_of_help_options()
        {
            var parser =
                new ParserBuilder()
                    .AddCommand("command", "")
                    .AddHelp(new[]{"~cthulhu"})
                    .Build();

            var result = parser.Parse("command ~cthulhu");

            var sb = new StringBuilder();
            using (var output = new StringWriter(sb))
            {
                result.Invoke(output);
            }

            string helpView = sb.ToString();
            helpView.Should().StartWith("Usage:");
        }

        [Fact]
        public void Add_help_does_not_display_when_option_defined_with_same_alias()
        {
            // dotnet add -h

            var parser =
                new ParserBuilder()
                    .AddCommand("command", "",
                        cmd => cmd.AddOption("-h"))
                    .AddHelp()
                    .Build();

            var result = parser.Parse("command -h");

            var sb = new StringBuilder();
            using (var output = new StringWriter(sb))
            {
                result.Invoke(output);
            }

            string helpView = sb.ToString();
            helpView.Should().BeEmpty();
        }

        private class TestInvocationResult : IInvocationResult
        {
        }
    }
}
