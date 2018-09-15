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
        public async Task General_invocation_middlware_can_be_specified_in_the_parser()
        {
            var wasCalled = false;

            var parser =
                new CommandLineBuilder()
                    .AddCommand("command", "")
                    .UseMiddleware(_ => wasCalled = true)
                    .Build();

            await parser.InvokeAsync("command", _console);

            wasCalled.Should().BeTrue();
        }

        [Fact]
        public async Task Specific_invocation_behavior_can_be_specified_in_the_command()
        {
            var wasCalled = false;

            var parser =
                new CommandLineBuilder()
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
                new CommandLineBuilder()
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
        public async Task Method_parameters_on_the_invoked_method_can_be_bound_to_hyphenated_option_names()
        {
            var wasCalled = false;

            void Execute(string firstName)
            {
                wasCalled = true;
                firstName.Should().Be("Gandalf");
            }

            var parser =
                new CommandLineBuilder()
                    .AddCommand(
                        "command", "",
                        cmd => {
                            cmd.AddOption("--first-name", "", a => a.ExactlyOne())
                               .OnExecute<string>(Execute);
                        })
                    .Build();

            await parser.InvokeAsync("command --first-name Gandalf", _console);

            wasCalled.Should().BeTrue();
        }

        [Fact]
        public async Task Method_parameters_on_the_invoked_method_can_be_bound_to_option_names_case_insensitively()
        {
            var wasCalled = false;

            void Execute(string name, int Age)
            {
                wasCalled = true;
                name.Should().Be("Gandalf");
                Age.Should().Be(425);
            }

            var parser =
                new CommandLineBuilder()
                    .AddCommand(
                        "command", "",
                        cmd => {
                            cmd.AddOption("--NAME", "", a => a.ExactlyOne())
                               .OnExecute<string, int>(Execute)
                               .AddOption("--age", "", a => a.ParseArgumentsAs<int>());
                        })
                    .Build();

            await parser.InvokeAsync("command --age 425 --NAME Gandalf", _console);

            wasCalled.Should().BeTrue();
        }

        [Fact]
        public async Task Method_parameters_on_the_invoked_method_do_not_need_to_be_matched()
        {
            var wasCalled = false;

            void Execute(string name, int age)
            {
                wasCalled = true;
                name.Should().Be(null);
                age.Should().Be(0);
            }

            var parser =
                new CommandLineBuilder()
                    .AddCommand(
                        "command", "",
                        cmd => {
                            cmd.AddOption("--name", "", a => a.ExactlyOne())
                               .AddOption("--age", "", a => a.ParseArgumentsAs<int>())
                               .OnExecute<string, int>(Execute);
                        })
                    .Build();

            await parser.InvokeAsync("command", _console);

            wasCalled.Should().BeTrue();
        }

        [Fact]
        public async Task Method_parameters_on_the_invoked_method_can_be_bound_to_option_names_by_alias()
        {
            var wasCalled = false;

            void Execute(string name, int Age)
            {
                wasCalled = true;
                name.Should().Be("Gandalf");
                Age.Should().Be(425);
            }

            var parser =
                new CommandLineBuilder()
                    .AddCommand(
                        "command", "",
                        cmd => {
                            cmd.AddOption(new[] { "-n", "--NAME" }, "", a => a.ExactlyOne())
                               .OnExecute<string, int>(Execute)
                               .AddOption(new[] { "-a", "--age" }, "", a => a.ParseArgumentsAs<int>());
                        })
                    .Build();

            await parser.InvokeAsync("command -a 425 -n Gandalf", _console);

            wasCalled.Should().BeTrue();
        }

        [Fact]
        public async Task Method_parameters_on_the_invoked_lambda_are_bound_to_matching_option_names()
        {
            var wasCalled = false;

            var parser =
                new CommandLineBuilder()
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
                new CommandLineBuilder()
                    .AddCommand(
                        "command", "",
                        cmd => {
                            cmd.AddOption("-x", "", args => args.ParseArgumentsAs<int>())
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
        public async Task Method_parameters_of_type_IConsole_receive_the_current_console_instance()
        {
            var wasCalled = false;

            var parser =
                new CommandLineBuilder()
                    .AddCommand(
                        "command", "",
                        cmd => {
                            cmd.AddOption("-x", "", args => args.ParseArgumentsAs<int>())
                               .OnExecute<IConsole>(console => {
                                   wasCalled = true;
                                   console.Out.Write("Hello!");
                               });
                        })
                    .Build();

            await parser.InvokeAsync("command", _console);

            wasCalled.Should().BeTrue();
            _console.Out.ToString().Should().Be("Hello!");
        }

        [Fact]
        public async Task Method_parameters_of_type_InvocationContext_receive_the_current_InvocationContext_instance()
        {
            var wasCalled = false;

            var parser =
                new CommandLineBuilder()
                    .AddCommand(
                        "command", "",
                        cmd => {
                            cmd.AddOption("-x", "", args => args.ParseArgumentsAs<int>())
                               .OnExecute<InvocationContext>(context => {
                                   wasCalled = true;
                                   context.ParseResult.ValueForOption("-x").Should().Be(123);
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

            var parser = new CommandLineBuilder()
                         .AddCommand("first", "",
                                     cmd => cmd.OnExecute(() => firstWasCalled = true))
                         .AddCommand("second", "",
                                     cmd => cmd.OnExecute(() => secondWasCalled = true))
                         .Build();

            await parser.InvokeAsync("first", _console);

            firstWasCalled.Should().BeTrue();
            secondWasCalled.Should().BeFalse();
        }

        [Fact]
        public void When_middleware_throws_then_InvokeAsync_does_not_handle_the_exception()
        {
            var parser = new CommandLineBuilder()
                         .AddCommand("the-command", "")
                         .UseMiddleware(_ => throw new Exception("oops!"))
                         .Build();

            Func<Task> invoke = async () => await parser.InvokeAsync("the-command", _console);

            invoke.Should()
                  .Throw<Exception>()
                  .WithMessage("oops!");
        }

        [Fact]
        public void When_command_handler_throws_then_InvokeAsync_does_not_handle_the_exception()
        {
            var parser = new CommandLineBuilder()
                         .AddCommand("the-command", "",
                                     cmd => cmd.OnExecute(() => throw new Exception("oops!")))
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

        [Fact]
        public async Task UseExceptionHandler_catches_middleware_exceptions_and_writes_details_to_standard_error()
        {
            var parser = new CommandLineBuilder()
                         .AddCommand("the-command", "")
                         .UseMiddleware(_ => throw new Exception("oops!"))
                         .UseExceptionHandler()
                         .Build();

            var resultCode = await parser.InvokeAsync("the-command", _console);

            _console.Error.ToString().Should().Contain("Unhandled exception: System.Exception: oops!");

            resultCode.Should().Be(1);
        }

        [Fact]
        public async Task UseExceptionHandler_catches_command_handler_exceptions_and_sets_result_code_to_1()
        {
            var parser = new CommandLineBuilder()
                         .AddCommand("the-command", "",
                                     cmd => cmd.OnExecute(() => throw new Exception("oops!")))
                         .UseExceptionHandler()
                         .Build();

            var resultCode = await parser.InvokeAsync("the-command", _console);

            resultCode.Should().Be(1);
        }

        [Fact]
        public async Task UseExceptionHandler_catches_command_handler_exceptions_and_writes_details_to_standard_error()
        {
            var parser = new CommandLineBuilder()
                         .AddCommand("the-command", "",
                                     cmd => cmd.OnExecute(() => throw new Exception("oops!")))
                         .UseExceptionHandler()
                         .Build();

            var resultCode = await parser.InvokeAsync("the-command", _console);

            _console.Error.ToString().Should().Contain("System.Exception: oops!");
        }

        [Fact]
        public async Task Declaration_of_UseExceptionHandler_can_come_before_other_middleware()
        {
            await new CommandLineBuilder()
                  .AddCommand("the-command", "")
                  .UseExceptionHandler()
                  .UseMiddleware(_ => throw new Exception("oops!"))
                  .Build()
                  .InvokeAsync("the-command", _console);

            _console.Error
                    .ToString()
                    .Should()
                    .Contain("oops!");
        }

        [Fact]
        public async Task Declaration_of_UseExceptionHandler_can_come_after_other_middleware()
        {
            await new CommandLineBuilder()
                  .AddCommand("the-command", "")
                  .UseMiddleware(_ => throw new Exception("oops!"))
                  .UseExceptionHandler()
                  .Build()
                  .InvokeAsync("the-command", _console);

            _console.Error
                    .ToString()
                    .Should()
                    .Contain("oops!");
        }

        [Fact]
        public async Task ParseResult_can_be_replaced_by_middleware()
        {
            var wasCalled = false;

            var parser = new CommandLineBuilder()
                         .UseMiddleware(context => {
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
