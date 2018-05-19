using System.CommandLine.Builder;
using System.IO;
using System.Text;
using FluentAssertions;
using Xunit;
using Xunit.Sdk;

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
            {
                result.Invoke(output);
            }

            string helpView = sb.ToString();
            helpView.Should().StartWith("Usage:");
        }

        private class TestInvocationResult : IInvocationResult
        {
        }
    }
}
