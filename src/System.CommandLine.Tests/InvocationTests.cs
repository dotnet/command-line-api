// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.CommandLine.Builder;
using System.CommandLine.Invocation;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using FluentAssertions;
using Xunit;

namespace System.CommandLine.Tests
{
    public class InvocationTests
    {
        private readonly TestConsole _console = new TestConsole();

        [Fact]
        public async Task General_invocation_middlware_can_be_specified_in_the_parser_definition()
        {
            var wasCalled = false;

            var parser =
                new ParserBuilder()
                    .AddCommand("command", "")
                    .AddMiddleware(_ => wasCalled = true)
                    .Build();

            await parser.InvokeAsync("command", _console);

            wasCalled.Should().BeTrue();
        }

        [Fact]
        public async Task Specific_invocation_behavior_can_be_specified_in_the_command_definition()
        {
            var wasCalled = false;

            var parser =
                new ParserBuilder()
                    .AddCommand(
                        "command", "",
                        cmd => cmd.OnExecute(() => wasCalled = true))
                    .Build();

            await parser.InvokeAsync("command", _console);

            wasCalled.Should().BeTrue();
        }

        [Fact]
        public async Task Method_parameters_on_the_invoked_method_are_bound_to_matching_option_names()
        {
            var wasCalled = false;

            void Execute(string name, int age)
            {
                wasCalled = true;
                name.Should().Be("Gandalf");
                age.Should().Be(425);
            }

            var parser =
                new ParserBuilder()
                    .AddCommand(
                        "command", "",
                        cmd => {
                            cmd.AddOption("--name", "", a => a.ExactlyOne())
                               .OnExecute<string, int>(Execute)
                               .AddOption("--age", "", a => a.ParseArgumentsAs<int>());
                        })
                    .Build();

            await parser.InvokeAsync("command --age 425 --name Gandalf", _console);

            wasCalled.Should().BeTrue();
        }

        [Fact]
        public async Task Method_parameters_on_the_invoked_lambda_are_bound_to_matching_option_names()
        {
            var wasCalled = false;

            var parser =
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
                    .Build();

            await parser.InvokeAsync("command --age 425 --name Gandalf", _console);

            wasCalled.Should().BeTrue();
        }

        [Fact]
        public async Task Method_parameters_of_type_ParseResult_receive_the_current_ParseResult_instance()
        {
            var wasCalled = false;

            var parser =
                new ParserBuilder()
                    .AddCommand(
                        "command", "",
                        cmd => {
                            cmd
                                .AddOption("-x", "", args => args.ParseArgumentsAs<int>())
                                .OnExecute<ParseResult>(result => {
                                    wasCalled = true;
                                    result.ValueForOption("-x").Should().Be(123);
                                });
                        })
                    .Build();

            await parser.InvokeAsync("command -x 123", _console);

            wasCalled.Should().BeTrue();
        }

        [Fact]
        public async Task InvokeAsync_chooses_the_appropriate_command()
        {
            var firstWasCalled = false;
            var secondWasCalled = false;

            var parser = new ParserBuilder()
                         .AddCommand("first", "",
                                     cmd => cmd.OnExecute<string>(_ => firstWasCalled = true))
                         .AddCommand("second", "",
                                     cmd => cmd.OnExecute<string>(_ => secondWasCalled = true))
                         .Build();

            await parser.InvokeAsync("first", _console);

            firstWasCalled.Should().BeTrue();
            secondWasCalled.Should().BeFalse();
        }

        [Fact]
        public void When_middleware_throws_then_InvokeAsync_does_not_handle_the_exception()
        {
            var parser = new ParserBuilder()
                         .AddCommand("the-command", "")
                         .AddMiddleware(_ => throw new Exception("oops!"))
                         .Build();

            Func<Task> invoke = async () => await parser.InvokeAsync("the-command", _console);

            invoke.Should()
                  .Throw<Exception>()
                  .WithMessage("oops!");
        }

        [Fact]
        public void When_command_handler_throws_then_InvokeAsync_does_not_handle_the_exception()
        {
            var parser = new ParserBuilder()
                         .AddCommand("the-command", "",
                                     cmd => cmd.OnExecute<string>(_ => throw new Exception("oops!")))
                         .Build();

            Func<Task> invoke = async () => await parser.InvokeAsync("the-command", _console);

            invoke.Should()
                  .Throw<TargetInvocationException>()
                  .Which
                  .InnerException
                  .Message
                  .Should()
                  .Be("oops!");
        }

        private class TestInvocationResult : IInvocationResult
        {
            public void Apply(InvocationContext context)
            {
            }
        [Fact]
        public async Task ParseResult_can_be_replaced_by_middleware()
        {
            var wasCalled = false;

            var parser = new ParserBuilder()
                         .AddMiddleware(context => {
                             var tokensAfterFirst = context.ParseResult.Tokens.Skip(1).ToArray();
                             var reparsed = context.Parser.Parse(tokensAfterFirst);
                             context.ParseResult = reparsed;
                         })
                         .AddCommand("the-command", "",
                                     cmd => cmd.OnExecute<ParseResult>(result => {
                                         wasCalled = true;
                                         result.Errors.Should().BeEmpty();
                                     }))
                         .Build();

            await parser.InvokeAsync("!my-directive the-command", new TestConsole());

            wasCalled.Should().BeTrue();
        }
    }
}
