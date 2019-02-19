// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.CommandLine.Binding;
using System.CommandLine.Builder;
using System.CommandLine.Invocation;
using System.CommandLine.Tests;
using System.CommandLine.Tests.Binding;
using FluentAssertions;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace System.CommandLine.DragonFruit.Tests
{
    public class ConfigureFromMethodTests
    {
        private object[] _receivedValues;
        private readonly TestConsole _testConsole = new TestConsole();

        [Fact]
        public async Task Generated_boolean_parameters_will_accept_zero_arguments()
        {
            var parser = new CommandLineBuilder()
                         .ConfigureRootCommandFromMethod(
                             GetMethodInfo(nameof(Method_taking_bool)), this)
                         .Build();

            await parser.InvokeAsync($"{RootCommand.ExeName} --value", _testConsole);

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
            var parser = new CommandLineBuilder()
                         .ConfigureRootCommandFromMethod(
                             GetMethodInfo(nameof(Method_taking_bool)), this)
                         .Build();

            await parser.InvokeAsync(commandLine, _testConsole);

            _receivedValues.Should().BeEquivalentTo(expected);
        }

        [Fact]
        public async Task Single_character_parameters_generate_aliases_that_accept_a_single_dash_prefix()
        {
            var parser = new CommandLineBuilder()
                         .ConfigureRootCommandFromMethod(
                             GetMethodInfo(nameof(Method_with_single_letter_parameters)), this)
                         .Build();

            await parser.InvokeAsync("-x 123 -y 456", _testConsole);

            _receivedValues.Should()
                           .BeEquivalentSequenceTo(123, 456);
        }

        [Fact]
        public async Task When_method_returns_void_then_return_code_is_0()
        {
            var parser = new CommandLineBuilder()
                         .ConfigureRootCommandFromMethod(
                             GetMethodInfo(nameof(Method_returning_void)), this)
                         .Build();

            var result = await parser.InvokeAsync("", _testConsole);

            result.Should().Be(0);
        }

        [Fact]
        public async Task When_method_returns_int_then_return_code_is_set_to_return_value()
        {
            var parser = new CommandLineBuilder()
                         .ConfigureRootCommandFromMethod(
                             GetMethodInfo(nameof(Method_returning_int)), this)
                         .Build();

            var result = await parser.InvokeAsync("-i 123", _testConsole);

            result.Should().Be(123);
        }

        [Fact]
        public async Task When_method_returns_Task_of_int_then_return_code_is_set_to_return_value()
        {
            var parser = new CommandLineBuilder()
                         .ConfigureRootCommandFromMethod(
                             GetMethodInfo(nameof(Method_returning_Task_of_int)), this)
                         .Build();

            var result = await parser.InvokeAsync("-i 123", _testConsole);

            result.Should().Be(123);
        }

        [Theory]
        [InlineData(typeof(IConsole))]
        [InlineData(typeof(InvocationContext))]
        [InlineData(typeof(BindingContext))]
        [InlineData(typeof(ParseResult))]
        [InlineData(typeof(CancellationToken))]
        public void Options_are_not_built_for_infrastructure_types_exposed_by_method_parameters(Type type)
        {
            var targetType = typeof(ClassWithMethodHavingParameter<>).MakeGenericType(type);

            var handlerMethod = targetType.GetMethod(nameof(ClassWithMethodHavingParameter<int>.Handle));

            var options = handlerMethod.BuildOptions();

            options.Should()
                   .NotContain(o => o.Argument.ArgumentType == type);
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
