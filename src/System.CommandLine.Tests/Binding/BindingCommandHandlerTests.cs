// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.CommandLine.Invocation;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using Xunit;

namespace System.CommandLine.Tests.Binding
{
    public class BindingCommandHandlerTests
    {
        private readonly TestConsole _console = new TestConsole();

        [Fact]
        public async Task Method_parameters_on_the_invoked_method_are_bound_to_matching_option_names()
        {
            var wasCalled = false;
            const string commandLine = "command --age 425 --name Gandalf";
            object[] expectedArgumets = { "Gandalf", 425 };

            void Execute(string name, int age)
            {
                wasCalled = true;
                name.Should().Be("Gandalf");
                age.Should().Be(425);
            }

            var command = new Command("command");
            command.AddOption(
                new Option("--name",
                           argument: new Argument { Arity = ArgumentArity.ExactlyOne }));
            command.AddOption(
                new Option("--age",
                           argument: new Argument<int>()));
            var handler = CommandHandler.Create<string, int>(Execute);
            command.Handler = handler;

            var arguments = handler.Binder.GetInvocationArguments(command.CreateBindingContext(commandLine));
            arguments.Should().BeEquivalentSequenceTo(expectedArgumets);

            await command.InvokeAsync(commandLine, _console);

            wasCalled.Should().BeTrue();
        }

        [Fact]
        public async Task Method_parameters_on_the_invoked_method_can_be_bound_to_hyphenated_option_names()
        {
            var wasCalled = false;
            const string commandLine = "command --first-name Gandalf";
            object[] expectedArgumets = { "Gandalf" };

            void Execute(string firstName)
            {
                wasCalled = true;
                firstName.Should().Be("Gandalf");
            }

            var command = new Command("command");
            command.AddOption(new Option("--first-name",
                                         argument: new Argument { Arity = ArgumentArity.ExactlyOne }));
            var handler = CommandHandler.Create<string>(Execute);
            command.Handler = handler;

            var arguments = handler.Binder.GetInvocationArguments(command.CreateBindingContext(commandLine));
            arguments.Should().BeEquivalentSequenceTo(expectedArgumets);

            await command.InvokeAsync(commandLine, _console);

            wasCalled.Should().BeTrue();
        }

        [Fact]
        public async Task Method_parameters_on_the_invoked_method_can_be_bound_to_option_names_case_insensitively()
        {
            var wasCalled = false;
            const string commandLine = "command --age 425 --NAME Gandalf";
            object[] expectedArgumets = { "Gandalf", 425 };

            void Execute(string name, int Age)
            {
                wasCalled = true;
                name.Should().Be("Gandalf");
                Age.Should().Be(425);
            }

            var command = new Command("command");
            command.AddOption(new Option("--NAME", argument: new Argument { Arity = ArgumentArity.ExactlyOne }));
            command.AddOption(new Option("--age", argument: new Argument<int>()));
            var handler = CommandHandler.Create<string, int>(Execute);
            command.Handler = handler;

            var arguments = handler.Binder.GetInvocationArguments(command.CreateBindingContext(commandLine));
            arguments.Should().BeEquivalentSequenceTo(expectedArgumets);

            await command.InvokeAsync(commandLine, _console);

            wasCalled.Should().BeTrue();
        }

        [Fact(Skip = "Waiting for fix for nulls in BeEquivalentSequenceTo")]
        public async Task Method_parameters_on_the_invoked_method_do_not_need_to_be_matched()
        {
            var wasCalled = false;
            const string commandLine = "command";
            object[] expectedArgumets = { null, 0 };

            void Execute(string name, int age)
            {
                wasCalled = true;
                name.Should().Be(null);
                age.Should().Be(0);
            }

            var command = new Command("command");
            command.AddOption(new Option("--name", argument: new Argument<string>() { Arity = ArgumentArity.ExactlyOne }));
            command.AddOption(new Option("--age", argument: new Argument<int>()));
            var handler = CommandHandler.Create<string, int>(Execute);
            command.Handler = handler;

            var arguments = handler.Binder.GetInvocationArguments(command.CreateBindingContext(commandLine));
            arguments.Should().BeEquivalentSequenceTo(expectedArgumets);

            await command.InvokeAsync(commandLine, _console);

            wasCalled.Should().BeTrue();
        }

        [Fact]
        public async Task Method_parameters_on_the_invoked_method_can_be_bound_to_option_names_by_alias()
        {
            var wasCalled = false;
            const string commandLine = "command -a 425 -n Gandalf";
            object[] expectedArgumets = { "Gandalf", 425 };

            void Execute(string name, int Age)
            {
                wasCalled = true;
                name.Should().Be("Gandalf");
                Age.Should().Be(425);
            }

            var command = new Command("command");
            command.AddOption(new Option(new[] { "-n", "--NAME" }, argument: new Argument { Arity = ArgumentArity.ExactlyOne }));
            command.AddOption(new Option(new[] { "-a", "--age" }, argument: new Argument<int>()));
            var handler = CommandHandler.Create<string, int>(Execute);
            command.Handler = handler;

            var arguments = handler.Binder.GetInvocationArguments(command.CreateBindingContext(commandLine));
            arguments.Should().BeEquivalentSequenceTo(expectedArgumets);

            await command.InvokeAsync(commandLine, _console);

            wasCalled.Should().BeTrue();
        }

        [Fact]
        public async Task Method_parameters_on_the_invoked_lambda_are_bound_to_matching_option_names()
        {
            var wasCalled = false;
            const string commandLine = "command --age 425 --name Gandalf";
            object[] expectedArgumets = { "Gandalf", 425 };

            var command = new Command("command");
            command.AddOption(new Option("--name", "", new Argument<string>()));
            command.AddOption(new Option("--age", "", new Argument<int>()));
            command.Handler = CommandHandler.Create<string, int>((name, age) =>
            {
                wasCalled = true;
                name.Should().Be("Gandalf");
                age.Should().Be(425);
            });

            var handler = command.Handler as ReflectionCommandHandler;

            var arguments = handler.Binder.GetInvocationArguments(command.CreateBindingContext(commandLine));
            arguments.Should().BeEquivalentSequenceTo(expectedArgumets);

            await command.InvokeAsync(commandLine, _console);

            wasCalled.Should().BeTrue();
        }

        [Fact]
        public async Task Method_parameters_of_type_ParseResult_receive_the_current_ParseResult_instance()
        {
            var wasCalled = false;
            const string commandLine = "command -x 123";

            var command = new Command("command");
            command.AddOption(new Option("-x", "", new Argument<int>()));
            command.Handler = CommandHandler.Create<ParseResult>(result =>
            {
                wasCalled = true;
                result.ValueForOption("-x").Should().Be(123);
            });

            var handler = command.Handler as ReflectionCommandHandler;

            var arguments = handler.Binder.GetInvocationArguments(command.CreateBindingContext(commandLine));
            arguments.Count().Should().Be(1);
            arguments.First().Should().NotBeNull();
            arguments.First().Should().BeOfType<ParseResult>();

            await command.InvokeAsync(commandLine, _console);

            wasCalled.Should().BeTrue();
        }

        [Fact]
        public async Task Method_parameters_of_type_IConsole_receive_the_current_console_instance()
        {
            var wasCalled = false;
            const string commandLine = "command";

            var command = new Command("command");
            command.AddOption(new Option("-x", "", new Argument<int>()));
            command.Handler = CommandHandler.Create<IConsole>(console =>
            {
                wasCalled = true;
                console.Out.Write("Hello!");
            });

            var handler = (ReflectionCommandHandler) command.Handler;

            var arguments = handler.Binder.GetInvocationArguments(command.CreateBindingContext(commandLine));

            arguments.Should()
                     .ContainSingle(a => a is IConsole);
           
            await command.InvokeAsync(commandLine, _console);

            wasCalled.Should().BeTrue();
            _console.Out.ToString().Should().Be("Hello!");
        }
    }
}
