﻿// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.CommandLine.Invocation;
using FluentAssertions;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;

namespace System.CommandLine.Tests
{
    public class ConfigureFromMethodTests
    {
        private object[] _receivedValues;
        private readonly TestConsole _testConsole = new TestConsole();
        private readonly ITestOutputHelper _output;

        public ConfigureFromMethodTests(ITestOutputHelper output)
        {
            _output = output;
        }

        [Fact]
        public async Task Generated_boolean_parameters_will_accept_zero_arguments()
        {
            var rootCommand = new RootCommand();
            rootCommand.ConfigureFromMethod(GetMethodInfo(nameof(Method_taking_bool)), this);

            await rootCommand.InvokeAsync($"{RootCommand.ExeName} --value", _testConsole);

            _receivedValues.Should().BeEquivalentTo(true);
        }

        [Theory]
        [InlineData("--value true", true)]
        [InlineData("--value false", false)]
        [InlineData("--value:true", true)]
        [InlineData("--value:false", false)]
        [InlineData("--value=true", true)]
        [InlineData("--value=false", false)]
        public async Task Generated_boolean_parameters_will_accept_one_argument(string commandLine, bool expected)
        {
            var rootCommand = new RootCommand();
            rootCommand.ConfigureFromMethod(GetMethodInfo(nameof(Method_taking_bool)), this);

            await rootCommand.InvokeAsync(commandLine, _testConsole);

            _receivedValues.Should().BeEquivalentTo(expected);
        }

        [Fact]
        public async Task Single_character_parameters_generate_aliases_that_accept_a_single_dash_prefix()
        {
            var command = new Command("the-command");
            command.ConfigureFromMethod(GetMethodInfo(nameof(Method_with_single_letter_parameters)), this);

            await command.InvokeAsync("-x 123 -y 456", _testConsole);

            _receivedValues.Should()
                           .BeEquivalentSequenceTo(123, 456);
        }

        [Fact]
        public async Task When_method_returns_void_then_return_code_is_0()
        {
            var command = new Command("the-command");
            command.ConfigureFromMethod(GetMethodInfo(nameof(Method_returning_void)), this);

            var result = await command.InvokeAsync("", _testConsole);

            result.Should().Be(0);
        }

        [Fact]
        public async Task When_method_returns_int_then_return_code_is_set_to_return_value()
        {
            var command = new Command("the-command");
            command.ConfigureFromMethod(GetMethodInfo(nameof(Method_returning_int)), this);

            var result = await command.InvokeAsync("-i 123", _testConsole);

            result.Should().Be(123);
        }

        [Fact]
        public async Task When_method_returns_Task_of_int_then_return_code_is_set_to_return_value()
        {
            var command = new Command("the-command");
            command.ConfigureFromMethod(GetMethodInfo(nameof(Method_returning_Task_of_int)), this);

            var result = await command.InvokeAsync("-i 123", _testConsole);

            result.Should().Be(123);
        }

        internal void Method_taking_bool(bool value = false)
        {
            _receivedValues = new object[] { value };
        }

        internal void Method_with_single_letter_parameters(
            int x,
            int y)
        {
            _receivedValues = new object[] { x, y };
        }

        internal void Method_returning_void()
        {
        }

        internal int Method_returning_int(int i)
        {
            return i;
        }

        internal async Task<int> Method_returning_Task_of_int(int i)
        {
            await Task.Yield();
            return i;
        }

        private MethodInfo GetMethodInfo(string name)
        {
            return typeof(ConfigureFromMethodTests)
                   .GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)
                   .Single(m => m.Name == name);
        }
    }
}
