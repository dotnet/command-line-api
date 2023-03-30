// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Threading.Tasks;
using FluentAssertions;
using Xunit;

namespace System.CommandLine.Generator.Tests
{
    public class GeneratedCommandHandlerTests
    {
        [Fact]
        public async Task Can_generate_handler_for_void_returning_method()
        {
            string? boundName = default;
            int boundAge = default;

            void Execute(string fullnameOrNickname, int age)
            {
                boundName = fullnameOrNickname;
                boundAge = age;
            }

            var nameArgument = new CliArgument<string>("arg");
            var ageOption = new CliOption<int>("--age");

            var command = new CliCommand("command")
            {
                nameArgument,
                ageOption
            };

            command.SetHandler<Action<string, int>>
                (Execute, nameArgument, ageOption);

            await command.Parse("command Gandalf --age 425").InvokeAsync();

            boundName.Should().Be("Gandalf");
            boundAge.Should().Be(425);
        }   
        
        [Fact]
        public async Task Can_generate_handler_for_void_returning_delegate()
        {
            string? boundName = default;
            int boundAge = default;

            var nameArgument = new CliArgument<string>("arg");
            var ageOption = new CliOption<int>("--age");

            var command = new CliCommand("command")
            {
                nameArgument,
                ageOption
            };

            command.SetHandler<Action<string, int>>
                ((fullnameOrNickname, age) =>
                {
                    boundName = fullnameOrNickname;
                    boundAge = age;
                }, nameArgument, ageOption);

            await command.Parse("command Gandalf --age 425").InvokeAsync();

            boundName.Should().Be("Gandalf");
            boundAge.Should().Be(425);
        }

        [Fact]
        public async Task Can_generate_handler_for_method_with_model()
        {
            string? boundName = default;
            int boundAge = default;

            void Execute(Character character)
            {
                boundName = character.FullName;
                boundAge = character.Age;
            }

            var command = new CliCommand("command");
            var nameOption = new CliOption<string>("--name");
            command.Options.Add(nameOption);
            var ageOption = new CliOption<int>("--age");
            command.Options.Add(ageOption);

            command.SetHandler<Action<Character>>(Execute, nameOption, ageOption);

            await command.Parse("command --age 425 --name Gandalf").InvokeAsync();

            boundName.Should().Be("Gandalf");
            boundAge.Should().Be(425);
        }

        [Fact]
        public async Task Can_generate_handler_for_int_returning_method()
        {
            int Execute(int first, int second)
            {
                return first + second;
            }

            var command = new CliCommand("add");
            var firstArgument = new CliArgument<int>("first");
            command.Arguments.Add(firstArgument);
            var secondArgument = new CliArgument<int>("second");
            command.Arguments.Add(secondArgument);

            command.SetHandler<Func<int, int, int>>(Execute, firstArgument, secondArgument);

            int result = await command.Parse("add 1 2").InvokeAsync();

            result.Should().Be(3);
        }

        [Fact]
        public async Task Can_generate_handler_with_well_know_parameters_types()
        {
            ParseResult? boundParseResult = null;

            void Execute(
                ParseResult parseResult)
            {
                boundParseResult = parseResult;
            }

            var command = new CliCommand("command");

            command.SetHandler<Action<ParseResult>>(Execute);

            await command.Parse("command").InvokeAsync();

            boundParseResult.Should().NotBeNull();
        }

        [Fact]
        public async Task Can_generate_handler_for_async_method()
        {
            string? boundName = default;
            int boundAge = default;

            async Task ExecuteAsync(string fullnameOrNickname, int age)
            {
                await Task.Yield();
                boundName = fullnameOrNickname;
                boundAge = age;
            }

            var nameArgument = new CliArgument<string>("arg");
            var ageOption = new CliOption<int>("--age");

            var command = new CliCommand("command")
            {
                nameArgument,
                ageOption
            };

            command.SetHandler<Func<string, int, Task>>
                (ExecuteAsync, nameArgument, ageOption);

            await command.Parse("command Gandalf --age 425").InvokeAsync();

            boundName.Should().Be("Gandalf");
            boundAge.Should().Be(425);
        }

        [Fact]
        public async Task Can_generate_handler_for_async_task_of_int_returning_method()
        {
            async Task<int> Execute(int first, int second)
            {
                await Task.Yield();
                return first + second;
            }

            var firstArgument = new CliArgument<int>("first");
            var secondArgument = new CliArgument<int>("second");
            var command = new CliCommand("add")
            {
                firstArgument,
                secondArgument
            };

            command.SetHandler<Func<int, int, Task<int>>>
                (Execute, firstArgument, secondArgument);

            int result = await command.Parse("add 1 2").InvokeAsync();

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

            var command1 = new CliCommand("first");
            var argument1 = new CliArgument<string>("first-value");
            command1.Arguments.Add(argument1);
            command1.SetHandler<Action<string>>(Execute1, argument1);

            var command2 = new CliCommand("second");
            var argument2 = new CliArgument<string>("second-value");
            command2.Arguments.Add(argument2);
            command2.SetHandler<Action<string>>(Execute2, argument2);

            await command1.Parse("first v1").InvokeAsync();
            await command2.Parse("second v2").InvokeAsync();

            firstValue.Should().Be("v1");
            secondValue.Should().Be("v2");
        }

        [Fact]
        public async Task Can_generate_handler_natural_type_delegates()
        {
            string? boundName = default;
            int boundAge = default;

            void Execute(string fullnameOrNickname, int age)
            {
                boundName = fullnameOrNickname;
                boundAge = age;
            }

            var nameArgument = new CliArgument<string>("arg");
            var ageOption = new CliOption<int>("--age");

            var command = new CliCommand("command")
            {
                nameArgument,
                ageOption
            };

            command.SetHandler(Execute, nameArgument, ageOption);

            await command.Parse("command Gandalf --age 425").InvokeAsync();

            boundName.Should().Be("Gandalf");
            boundAge.Should().Be(425);
        }

        [Fact]
        public async Task Can_generate_handler_for_lambda()
        {
            string? boundName = default;
            int boundAge = default;

            var nameArgument = new CliArgument<string>("arg");
            var ageOption = new CliOption<int>("--age");

            var command = new CliCommand("command")
            {
                nameArgument,
                ageOption
            };

            command.SetHandler((string fullnameOrNickname, int age) =>
            {
                boundName = fullnameOrNickname;
                boundAge = age;
            }, nameArgument, ageOption);

            await command.Parse("command Gandalf --age 425").InvokeAsync();

            boundName.Should().Be("Gandalf");
            boundAge.Should().Be(425);
        }

        [Fact]
        public async Task Can_generate_handler_for_lambda_wth_return_type_specified()
        {
            string? boundName = default;
            int boundAge = default;

            var nameArgument = new CliArgument<string>("arg");
            var ageOption = new CliOption<int>("--age");

            var command = new CliCommand("command")
            {
                nameArgument,
                ageOption
            };

            command.SetHandler(int (string fullnameOrNickname, int age) =>
            {
                boundName = fullnameOrNickname;
                boundAge = age;
                return 42;
            }, nameArgument, ageOption);

            int rv = await command.Parse("command Gandalf --age 425").InvokeAsync();

            rv.Should().Be(42);
            boundName.Should().Be("Gandalf");
            boundAge.Should().Be(425);
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
