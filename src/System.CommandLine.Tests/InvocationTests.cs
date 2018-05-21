// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.CommandLine.Builder;
using System.CommandLine.Invocation;
using FluentAssertions;
using Xunit;

namespace System.CommandLine.Tests
{
    public class InvocationTests
    {
        private readonly TestConsole _console = new TestConsole();

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

            result.InvokeAsync(_console);

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

            result.InvokeAsync(_console);

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

            result.InvokeAsync(_console);

            wasCalled.Should().BeTrue();
        }

        [Fact]
        public void Method_parameters_on_the_invoked_method_are_bound_to_matching_option_names()
        {
            var wasCalled = false;

            void Execute(string name, int age)
            {
                wasCalled = true;
                name.Should().Be("Gandalf");
                age.Should().Be(425);
            }

            var commandDefinition =
                new ParserBuilder()
                    .AddCommand(
                        "command", "",
                        cmd => {
                            cmd
                                .AddOption("--name", "", a => a.ExactlyOne())
                                .OnExecute<string, int>(Execute)
                                .AddOption("--age", "", a => a.ParseArgumentsAs<int>());
                        })
                    .BuildCommandDefinition();

            var result = commandDefinition.Parse("command --age 425 --name Gandalf");

            result.InvokeAsync(_console);

            wasCalled.Should().BeTrue();
        }

        [Fact]
        public void Method_parameters_on_the_invoked_lambda_are_bound_to_matching_option_names()
        {
            var wasCalled = false;

            var commandDefinition =
                new ParserBuilder()
                    .AddCommand(
                        "command", "",
                        cmd => {
                            cmd
                                .AddOption("--name", "", a => a.ExactlyOne())
                                .OnExecute<string, int>((name, age) => {
                                    wasCalled = true;
                                    name.Should().Be("Gandalf");
                                    age.Should().Be(425);
                                })
                                .AddOption("--age", "", a => a.ParseArgumentsAs<int>());
                        })
                    .BuildCommandDefinition();

            var result = commandDefinition.Parse("command --age 425 --name Gandalf");

            result.InvokeAsync(_console);

            wasCalled.Should().BeTrue();
        }

        [Fact]
        public void Invoke_chooses_the_appropriate_command()
        {
            var firstWasCalled = false;
            var secondWasCalled = false;

            var parser = new ParserBuilder()
                         .AddCommand("first", "",
                                     cmd => cmd.OnExecute<string>(_ => firstWasCalled = true))
                         .AddCommand("second", "",
                                     cmd => cmd.OnExecute<string>(_ => secondWasCalled = true))
                         .Build();

            parser.Parse("first").InvokeAsync(_console);

            firstWasCalled.Should().BeTrue();
            secondWasCalled.Should().BeFalse();
        }

        private class TestInvocationResult : IInvocationResult
        {
            public void Apply(InvocationContext context)
            {
            }
        }
    }
}
