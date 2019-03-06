// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.CommandLine.Binding;
using System.CommandLine.Invocation;
using System.IO;
using System.Threading.Tasks;
using FluentAssertions;
using Xunit;

namespace System.CommandLine.Tests.Invocation
{
    public class CommandHandlerTests
    {
        private readonly TestConsole _console = new TestConsole();

        [Fact]
        public async Task Specific_invocation_behavior_can_be_specified_in_the_command()
        {
            var wasCalled = false;

            var command = new Command("command");
            command.Handler = CommandHandler.Create(() => wasCalled = true);

            var parser = new Parser(command);

            await parser.InvokeAsync("command", _console);

            wasCalled.Should().BeTrue();
        }

        [Fact]
        public async Task Method_parameters_on_the_invoked_method_are_bound_to_matching_option_names()
        {
            string boundName = default;
            int boundAge = default;

            void Execute(string name, int age)
            {
                boundName = name;
                boundAge = age;
            }

            var command = new Command("command");
            command.AddOption(
                new Option("--name",
                           argument: new Argument<string>()));
            command.AddOption(
                new Option("--age",
                           argument: new Argument<int>()));
            command.Handler = CommandHandler.Create<string, int>(Execute);

            await command.InvokeAsync("command --age 425 --name Gandalf", _console);

            boundName.Should().Be("Gandalf");
            boundAge.Should().Be(425);
        }

        [Fact]
        public async Task Method_parameters_on_the_invoked_method_can_be_bound_to_hyphenated_option_names()
        {
            string boundFirstName = default;

            void Execute(string firstName)
            {
                boundFirstName = firstName;
            }

            var command = new Command("command");
            command.AddOption(new Option("--first-name",
                                         argument: new Argument { Arity = ArgumentArity.ExactlyOne }));
            command.Handler = CommandHandler.Create<string>(Execute);

            await command.InvokeAsync("command --first-name Gandalf", _console);

            boundFirstName.Should().Be("Gandalf");
        }

        [Fact]
        public async Task Method_parameters_on_the_invoked_method_can_be_bound_to_option_names_case_insensitively()
        {
            string boundName = default;
            int boundAge = default;

            void Execute(string name, int AGE)
            {
                boundName = name;
                boundAge = AGE;
            }

            var command = new Command("command");
            command.AddOption(new Option("--NAME", argument: new Argument { Arity = ArgumentArity.ExactlyOne }));
            command.AddOption(new Option("--age", argument: new Argument<int>()));
            command.Handler = CommandHandler.Create<string, int>(Execute);

            await command.InvokeAsync("command --age 425 --NAME Gandalf", _console);

            boundName.Should().Be("Gandalf");
            boundAge.Should().Be(425);
        }

        [Fact]
        public async Task Method_is_invoked_when_command_line_does_not_specify_matching_options()
        {
            string boundName = default;
            int boundAge = default;

            void Execute(string name, int age)
            {
                boundName = name;
                boundAge = age;
            }

            var command = new Command("command");
            command.AddOption(new Option("--name", argument: new Argument<string>()));
            command.AddOption(new Option("--age", argument: new Argument<int>()));
            command.Handler = CommandHandler.Create<string, int>(Execute);

            await command.InvokeAsync("command", _console);

            boundName.Should().Be(null);
            boundAge.Should().Be(0);
        }

        [Fact]
        public async Task Method_parameters_on_the_invoked_method_can_be_bound_to_option_names_by_alias()
        {
            string boundName = default;
            int boundAge = default;

            void Execute(string name, int age)
            {
                boundName = name;
                boundAge = age;
            }

            var command = new Command("command");
            command.AddOption(new Option(new[] { "-n", "--NAME" }, argument: new Argument { Arity = ArgumentArity.ExactlyOne }));
            command.AddOption(new Option(new[] { "-a", "--age" }, argument: new Argument<int>()));
            command.Handler = CommandHandler.Create<string, int>(Execute);

            await command.InvokeAsync("command -a 425 -n Gandalf", _console);

            boundName.Should().Be("Gandalf");
            boundAge.Should().Be(425);
        }

        [Fact]
        public async Task Method_parameters_on_the_invoked_lambda_are_bound_to_matching_option_names()
        {
            string boundName = default;
            int boundAge = default;

            var command = new Command("command");
            command.AddOption(new Option("--name", "", new Argument<string>()));
            command.AddOption(new Option("--age", "", new Argument<int>()));
            command.Handler = CommandHandler.Create<string, int>((name, age) =>
            {
                boundName = name;
                boundAge = age;
            });

            await command.InvokeAsync("command --age 425 --name Gandalf", _console);

            boundName.Should().Be("Gandalf");
            boundAge.Should().Be(425);
        }

        [Fact]
        public async Task Nullable_parameters_are_bound_to_correct_value_when_option_is_specified()
        {
            int? boundAge = default;

            var command = new Command("command");
            command.AddOption(new Option("--age", "", new Argument<int?>()));
            command.Handler = CommandHandler.Create<int?>(age =>
            {
                boundAge = age;
            });

            await command.InvokeAsync("command --age 425", _console);

            boundAge.Should().Be(425);
        }

        [Fact]
        public async Task Nullable_parameters_are_bound_to_null_when_option_is_not_specified()
        {
            var wasCalled = false;
            int? boundAge = default;

            var command = new Command("command");
            command.AddOption(new Option("--age", "", new Argument<int?>()));
            command.Handler = CommandHandler.Create<int?>(age =>
            {
                wasCalled = true;
                boundAge = age;
            });

            await command.InvokeAsync("command", _console);

            wasCalled.Should().BeTrue();
            boundAge.Should().BeNull();
        }

        [Fact]
        public async Task Method_parameters_of_types_having_constructors_accepting_a_single_string_are_bound_using_handler_parameter_name()
        {
            DirectoryInfo boundDirectoryInfo = default;
            var tempPath = Path.GetTempPath();

            var command = new Command("command");
            command.AddOption(new Option("--dir", "", new Argument<DirectoryInfo>()));
            command.Handler = CommandHandler.Create<DirectoryInfo>(dir =>
            {
                boundDirectoryInfo = dir;
            });

            await command.InvokeAsync($"command --dir \"{tempPath}\"", _console);

            boundDirectoryInfo.FullName.Should().Be(tempPath);
        }

        [Fact]
        public async Task Method_parameters_of_type_ParseResult_receive_the_current_ParseResult_instance()
        {
            ParseResult boundParseResult = default;

            var command = new Command("command");
            command.AddOption(new Option("-x", "", new Argument<int>()));
            command.Handler = CommandHandler.Create<ParseResult>(result => { boundParseResult = result; });

            await command.InvokeAsync("command -x 123", _console);

            boundParseResult.ValueForOption("-x").Should().Be(123);
        }

        [Fact]
        public async Task Method_parameters_of_type_ParseResult_receive_the_current_BindingContext_instance()
        {
            BindingContext boundContext = default;

            var command = new Command("command");
            command.AddOption(new Option("-x", "", new Argument<int>()));
            command.Handler = CommandHandler.Create<BindingContext>(context => { boundContext = context; });

            await command.InvokeAsync("command -x 123", _console);

            boundContext.ParseResult.ValueForOption("-x").Should().Be(123);
        }

        [Fact]
        public async Task Method_parameters_of_type_IConsole_receive_the_current_console_instance()
        {
            var command = new Command("command");
            command.AddOption(new Option("-x", "", new Argument<int>()));
            command.Handler = CommandHandler.Create<IConsole>(console => { console.Out.Write("Hello!"); });

            await command.InvokeAsync("command", _console);

            _console.Out.ToString().Should().Be("Hello!");
        }

        [Fact]
        public async Task Method_parameters_of_type_InvocationContext_receive_the_current_InvocationContext_instance()
        {
            InvocationContext boundContext = default;

            var command = new Command("command");
            command.AddOption(new Option("-x", "", new Argument<int>()));
            command.Handler = CommandHandler.Create<InvocationContext>(context => { boundContext = context; });

            await command.InvokeAsync("command -x 123", _console);

            boundContext.ParseResult.ValueForOption("-x").Should().Be(123);
        }
    }
}
