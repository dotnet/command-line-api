// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.CommandLine.Binding;
using System.CommandLine.Help;
using System.CommandLine.Invocation;
using System.CommandLine.IO;
using System.Threading.Tasks;
using FluentAssertions;
using Xunit;

namespace System.CommandLine.Generator.Tests
{
    public class GeneratedCommandHandlerTests
    {
        private readonly TestConsole _console = new();

        [Fact]
        public async Task Can_generate_handler_for_void_returning_method()
        {
            string? boundName = default;
            int boundAge = default;
            IConsole? boundConsole = null;

            void Execute(string fullnameOrNickname, IConsole console, int age)
            {
                boundName = fullnameOrNickname;
                boundConsole = console;
                boundAge = age;
            }

            var nameArgument = new Argument<string>();
            var ageOption = new Option<int>("--age");

            var command = new Command("command")
            {
                nameArgument,
                ageOption
            };

            command.SetHandler<Action<string, IConsole, int>>
                (Execute, nameArgument, ageOption);

            await command.InvokeAsync("command Gandalf --age 425", _console);

            boundName.Should().Be("Gandalf");
            boundAge.Should().Be(425);
            boundConsole.Should().NotBeNull();
        }   
        
        [Fact]
        public async Task Can_generate_handler_for_void_returning_delegate()
        {
            string? boundName = default;
            int boundAge = default;
            IConsole? boundConsole = null;

            var nameArgument = new Argument<string>();
            var ageOption = new Option<int>("--age");

            var command = new Command("command")
            {
                nameArgument,
                ageOption
            };

            command.SetHandler<Action<string, IConsole, int>>
                ((fullnameOrNickname, console, age) =>
                {
                    boundName = fullnameOrNickname;
                    boundConsole = console;
                    boundAge = age;
                }, nameArgument, ageOption);

            await command.InvokeAsync("command Gandalf --age 425", _console);

            boundName.Should().Be("Gandalf");
            boundAge.Should().Be(425);
            boundConsole.Should().NotBeNull();
        }

        [Fact]
        public async Task Can_generate_handler_for_method_with_model()
        {
            string? boundName = default;
            int boundAge = default;
            IConsole? boundConsole = null;

            void Execute(Character character, IConsole console)
            {
                boundName = character.FullName;
                boundConsole = console;
                boundAge = character.Age;
            }

            var command = new Command("command");
            var nameOption = new Option<string>("--name");
            command.AddOption(nameOption);
            var ageOption = new Option<int>("--age");
            command.AddOption(ageOption);

            command.SetHandler<Action<Character, IConsole>>(Execute, nameOption, ageOption);

            await command.InvokeAsync("command --age 425 --name Gandalf", _console);

            boundName.Should().Be("Gandalf");
            boundAge.Should().Be(425);
            boundConsole.Should().NotBeNull();
        }

        [Fact]
        public async Task Can_generate_handler_for_int_returning_method()
        {
            int Execute(int first, int second)
            {
                return first + second;
            }

            var command = new Command("add");
            var firstArgument = new Argument<int>("first");
            command.AddArgument(firstArgument);
            var secondArgument = new Argument<int>("second");
            command.AddArgument(secondArgument);

            command.SetHandler<Func<int, int, int>>(Execute, firstArgument, secondArgument);

            int result = await command.InvokeAsync("add 1 2", _console);

            result.Should().Be(3);
        }

        [Fact]
        public async Task Can_generate_handler_with_well_know_parameters_types()
        {
            InvocationContext? boundInvocationContext = null;
            IConsole? boundConsole = null;
            ParseResult? boundParseResult = null;
            HelpBuilder? boundHelpBuilder = null;
            BindingContext? boundBindingContext = null;

            void Execute(
                InvocationContext invocationContext,
                IConsole console,
                ParseResult parseResult,
                HelpBuilder helpBuilder,
                BindingContext bindingContext)
            {
                boundInvocationContext = invocationContext;
                boundConsole = console;
                boundParseResult = parseResult;
                boundHelpBuilder = helpBuilder;
                boundBindingContext = bindingContext;
            }

            var command = new Command("command");

            command.SetHandler<Action<InvocationContext, IConsole, ParseResult, HelpBuilder, BindingContext>>(Execute);

            await command.InvokeAsync("command", _console);

            boundInvocationContext.Should().NotBeNull();
            boundConsole.Should().Be(_console);
            boundParseResult.Should().NotBeNull();
            boundHelpBuilder.Should().NotBeNull();
            boundBindingContext.Should().NotBeNull();
        }

        [Fact]
        public async Task Can_generate_handler_for_async_method()
        {
            string? boundName = default;
            int boundAge = default;
            IConsole? boundConsole = null;

            async Task ExecuteAsync(string fullnameOrNickname, IConsole console, int age)
            {
                await Task.Yield();
                boundName = fullnameOrNickname;
                boundConsole = console;
                boundAge = age;
            }

            var nameArgument = new Argument<string>();
            var ageOption = new Option<int>("--age");

            var command = new Command("command")
            {
                nameArgument,
                ageOption
            };

            command.SetHandler<Func<string, IConsole, int, Task>>
                (ExecuteAsync, nameArgument, ageOption);

            await command.InvokeAsync("command Gandalf --age 425", _console);

            boundName.Should().Be("Gandalf");
            boundAge.Should().Be(425);
            boundConsole.Should().NotBeNull();
        }

        [Fact]
        public async Task Can_generate_handler_for_async_task_of_int_returning_method()
        {
            async Task<int> Execute(int first, int second)
            {
                await Task.Yield();
                return first + second;
            }

            var firstArgument = new Argument<int>("first");
            var secondArgument = new Argument<int>("second");
            var command = new Command("add")
            {
                firstArgument,
                secondArgument
            };

            command.SetHandler<Func<int, int, Task<int>>>
                (Execute, firstArgument, secondArgument);

            int result = await command.InvokeAsync("add 1 2", _console);

            result.Should().Be(3);
        }

        [Fact]
        public async Task Can_generate_handler_for_multiple_commands_with_the_same_signature()
        {
            string firstValue = "";

            void Execute1(string value)
            {
                firstValue = value;
            }

            string secondValue = "";

            void Execute2(string value)
            {
                secondValue = value;
            }

            var command1 = new Command("first");
            var argument1 = new Argument<string>("first-value");
            command1.AddArgument(argument1);
            command1.SetHandler<Action<string>>(Execute1, argument1);

            var command2 = new Command("second");
            var argument2 = new Argument<string>("second-value");
            command2.AddArgument(argument2);
            command2.SetHandler<Action<string>>(Execute2, argument2);

            await command1.InvokeAsync("first v1", _console);
            await command2.InvokeAsync("second v2", _console);

            firstValue.Should().Be("v1");
            secondValue.Should().Be("v2");
        }

        [Fact]
        public async Task Can_generate_handler_natural_type_delegates()
        {
            string? boundName = default;
            int boundAge = default;
            IConsole? boundConsole = null;

            void Execute(string fullnameOrNickname, IConsole console, int age)
            {
                boundName = fullnameOrNickname;
                boundConsole = console;
                boundAge = age;
            }

            var nameArgument = new Argument<string>();
            var ageOption = new Option<int>("--age");

            var command = new Command("command")
            {
                nameArgument,
                ageOption
            };

            command.SetHandler(Execute, nameArgument, ageOption);

            await command.InvokeAsync("command Gandalf --age 425", _console);

            boundName.Should().Be("Gandalf");
            boundAge.Should().Be(425);
            boundConsole.Should().NotBeNull();
        }

        [Fact]
        public async Task Can_generate_handler_for_lambda()
        {
            string? boundName = default;
            int boundAge = default;
            IConsole? boundConsole = null;

            var nameArgument = new Argument<string>();
            var ageOption = new Option<int>("--age");

            var command = new Command("command")
            {
                nameArgument,
                ageOption
            };

            command.SetHandler((string fullnameOrNickname, IConsole console, int age) =>
            {
                boundName = fullnameOrNickname;
                boundConsole = console;
                boundAge = age;
            }, nameArgument, ageOption);

            await command.InvokeAsync("command Gandalf --age 425", _console);

            boundName.Should().Be("Gandalf");
            boundAge.Should().Be(425);
            boundConsole.Should().NotBeNull();
        }

        [Fact]
        public async Task Can_generate_handler_for_lambda_wth_return_type_specified()
        {
            string? boundName = default;
            int boundAge = default;
            IConsole? boundConsole = null;

            var nameArgument = new Argument<string>();
            var ageOption = new Option<int>("--age");

            var command = new Command("command")
            {
                nameArgument,
                ageOption
            };

            command.SetHandler(int (string fullnameOrNickname, IConsole console, int age) =>
            {
                boundName = fullnameOrNickname;
                boundConsole = console;
                boundAge = age;
                return 42;
            }, nameArgument, ageOption);

            int rv = await command.InvokeAsync("command Gandalf --age 425", _console);

            rv.Should().Be(42);
            boundName.Should().Be("Gandalf");
            boundAge.Should().Be(425);
            boundConsole.Should().NotBeNull();
        }

        public class Character
        {
            public Character(string? fullName, int age)
            {
                FullName = fullName;
                Age = age;
            }

            public Character()
            {
            }

            public string? FullName { get; set; }
            public int Age { get; set; }
        }
    }
}
