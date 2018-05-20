using System.CommandLine.Builder;
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

        private class TestInvocationResult : IInvocationResult
        {
        }
    }
}
