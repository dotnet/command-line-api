using System.CommandLine.Invocation;
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
                    .AddInvocation(_ => wasCalled = true)
                    .Build();

            var result = parser.Parse("command");

            result.Invoke();

            wasCalled.Should().BeTrue();
        }

        [Fact]
        public void First_invocation_behavior_to_set_a_result_short_circuits()
        {
            var wasCalled = false;

            var parser =
                new ParserBuilder()
                    .AddCommand("command", "")
                    .AddInvocation(context => context.InvocationResult = new TestInvocationResult())
                    .AddInvocation(_ => wasCalled = true)
                    .Build();

            var result = parser.Parse("command");

            result.Invoke();

            wasCalled.Should().BeFalse();
        }

        [Fact]
        public void Specific_invocation_behavior_can_be_specified_in_the_command_definition()
        {
            var wasCalled = false;

            var commandDefinition =
                new ParserBuilder()
                    .AddCommand(
                        "command", "",
                        cmd => cmd.OnExecute(() => wasCalled = true))
                    .BuildCommandDefinition();

            var result = commandDefinition.Parse("command");

            result.Invoke();

            wasCalled.Should().BeTrue();
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
                                  .OnExecute<string>(s => {
                                      wasCalled = true;
                                      s.Should().Be("hello");
                                  }, "-name"))
                    .BuildCommandDefinition();

            var result = commandDefinition.Parse("command -name hello");

            result.Invoke();

            wasCalled.Should().BeTrue();
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
                               .OnExecute<string, int>((s, i) => {
                                   wasCalled = true;
                                   s.Should().Be("Gandalf");
                                   i.Should().Be(425);
                               }, "-name", "-age")
                               .AddOption("-age", "", a => a.ParseArgumentsAs<int>())
                    )
                    .BuildCommandDefinition();

            var result = commandDefinition.Parse("command -age 425 -name Gandalf");

            result.Invoke();

            wasCalled.Should().BeTrue();
        }

        [Fact]
        public void Invoke_chooses_the_appropriate_command()
        {
            var firstWasCalled = false;
            var secondWasCalled = false;

            var parser = new ParserBuilder()
                         .AddCommand("first", "",
                                     cmd => cmd.OnExecute<string>(_ => firstWasCalled = true, "a"))
                         .AddCommand("second", "",
                                     cmd => cmd.OnExecute<string>(_ => secondWasCalled = true, "b"))
                         .Build();

            parser.Parse("first").Invoke();

            firstWasCalled.Should().BeTrue();
            secondWasCalled.Should().BeFalse();
        }

        private class TestInvocationResult : IInvocationResult
        {
            public int ReturnCode { get; }
            public string StandardOutput { get; }
        }
    }
}
