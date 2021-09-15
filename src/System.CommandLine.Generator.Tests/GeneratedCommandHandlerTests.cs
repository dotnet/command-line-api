﻿// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.CommandLine.Binding;
using System.CommandLine.Help;
using System.CommandLine.Invocation;
using System.CommandLine.IO;
using System.CommandLine.Parsing;
using System.Threading.Tasks;
using FluentAssertions;
using Xunit;
using static System.CommandLine.Invocation.CommandHandlerGenerator;

#nullable enable
namespace System.CommandLine.Generator.Tests
{
    public class CommandHandlerTests
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

            var command = new Command("command");
            var nameArgument = new Argument<string>();
            command.AddArgument(nameArgument);
            var ageOption = new Option<int>("--age");
            command.AddOption(ageOption);

            command.Handler = GeneratedHandler.Create<Action<string, IConsole, int>>
                (Execute, nameArgument, ageOption);

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

            command.Handler = GeneratedHandler.Create<Action<Character, IConsole>>
                (Execute, nameOption, ageOption);

            await command.InvokeAsync("command --age 425 --name Gandalf", _console);

            boundName.Should().Be("Gandalf");
            boundAge.Should().Be(425);
            boundConsole.Should().NotBeNull();
        }

        [Fact]
        public async Task Can_generate_handler_for_method_with_model_property_binding()
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

            command.Handler = GeneratedHandler.Create<Action<Character, IConsole>, Character>
                (Execute, context => new Character
                {
                    FullName = context.ParseResult.ValueForOption(nameOption),
                    Age = context.ParseResult.ValueForOption(ageOption),
                });

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

            command.Handler = GeneratedHandler.Create<Func<int, int, int>>
                (Execute, firstArgument, secondArgument);

            int result = await command.InvokeAsync("add 1 2", _console);

            result.Should().Be(3);
        }

        [Fact]
        public async Task Can_generate_handler_with_well_know_parameters_types()
        {
            InvocationContext? boundInvocationContext = null;
            IConsole? boundConsole = null;
            ParseResult? boundParseResult = null;
            IHelpBuilder? boundHelpBuilder = null;
            BindingContext? boundBindingContext = null;

            void Execute(
                InvocationContext invocationContext,
                IConsole console, 
                ParseResult parseResult,
                IHelpBuilder helpBuilder,
                BindingContext bindingContext)
            {
                boundInvocationContext = invocationContext;
                boundConsole = console;
                boundParseResult = parseResult;
                boundHelpBuilder = helpBuilder;
                boundBindingContext = bindingContext;
            }

            var command = new Command("command");

            command.Handler = GeneratedHandler.Create<Action<InvocationContext, IConsole, ParseResult, IHelpBuilder, BindingContext>>(Execute);

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
                //Just long enough to make sure the taks is be awaited
                await Task.Delay(100);
                boundName = fullnameOrNickname;
                boundConsole = console;
                boundAge = age;
            }

            var command = new Command("command");
            var nameArgument = new Argument<string>();
            command.AddArgument(nameArgument);
            var ageOption = new Option<int>("--age");
            command.AddOption(ageOption);

            command.Handler = GeneratedHandler.Create<Func<string, IConsole, int, Task>>
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
                await Task.Delay(100);
                return first + second;
            }

            var command = new Command("add");
            var firstArgument = new Argument<int>("first");
            command.AddArgument(firstArgument);
            var secondArgument = new Argument<int>("second");
            command.AddArgument(secondArgument);

            command.Handler = GeneratedHandler.Create<Func<int, int, Task<int>>>
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
            command1.Handler = GeneratedHandler.Create<Action<string>>
                (Execute1, argument1);

            var command2 = new Command("second");
            var argument2 = new Argument<string>("second-value");
            command2.AddArgument(argument2);
            command2.Handler = GeneratedHandler.Create<Action<string>>
                (Execute2, argument2);

            await command1.InvokeAsync("first v1", _console);
            await command2.InvokeAsync("second v2", _console);

            firstValue.Should().Be("v1");
            secondValue.Should().Be("v2");
        }

        public class Character
        {
            public Character(string? fullName, int age)
            {
                FullName = fullName;
                Age = age;
            }

            public Character()
            { }

            public string? FullName { get; set; }
            public int Age { get; set; }
        }

    }
}
#nullable restore
