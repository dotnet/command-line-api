// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.CommandLine.Binding;
using System.CommandLine.Tests.Binding;
using System.CommandLine.Tests.Utility;
using System.IO;
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

        [Fact]
        public async Task Generated_boolean_parameters_will_accept_zero_arguments()
        {
            var config = new CliConfiguration(new CliRootCommand())
                         .ConfigureRootCommandFromMethod(
                             GetMethodInfo(nameof(Method_taking_bool)), this);
            config.Output = TextWriter.Null;

            await config.InvokeAsync($"{CliRootCommand.ExecutableName} --value");

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
            var config = new CliConfiguration(new CliRootCommand())
                         .ConfigureRootCommandFromMethod(
                             GetMethodInfo(nameof(Method_taking_bool)), this);
            config.Output = TextWriter.Null;

            await config.InvokeAsync(commandLine);

            _receivedValues.Should().BeEquivalentTo(expected);
        }

        [Fact]
        public async Task Single_character_parameters_generate_aliases_that_accept_a_single_dash_prefix()
        {
            var config = new CliConfiguration(new CliRootCommand())
                         .ConfigureRootCommandFromMethod(
                             GetMethodInfo(nameof(Method_with_single_letter_parameters)), this);
            config.Output = TextWriter.Null;

            await config.InvokeAsync("-x 123 -y 456");

            _receivedValues.Should()
                           .BeEquivalentSequenceTo(123, 456);
        }

        [Theory]
        [InlineData(nameof(Method_having_string_argument), 1, 1)]
        [InlineData(nameof(Method_having_string_argument_with_null_default_value), 0, 1)]
        [InlineData(nameof(Method_having_string_argument_with_non_null_default_value), 0, 1)]
        [InlineData(nameof(Method_having_string_array_arguments), 0, 100_000)]
        [InlineData(nameof(Method_having_string_array_arguments_with_default_value), 0, 100_000)]
        [InlineData(nameof(Method_having_FileInfo_argument), 1, 1)]
        [InlineData(nameof(Method_having_FileInfo_argument_with_default_value), 0, 1)]
        [InlineData(nameof(Method_having_FileInfo_array_args), 0, 100_000)]
        public void Parameters_named_arguments_generate_command_arguments_having_the_correct_arity(
            string methodName,
            int minNumberOfValues,
            int maxNumberOfValues)
        {
            var config = new CliConfiguration(new CliRootCommand())
                         .ConfigureRootCommandFromMethod(GetMethodInfo(methodName));

            var rootCommandArgument = config.RootCommand.Arguments.Single();

            rootCommandArgument.Arity
                               .Should()
                               .BeEquivalentTo(new ArgumentArity(minNumberOfValues, maxNumberOfValues));
        }

        [Theory]
        [InlineData(nameof(Method_having_string_argument), "argument")]
        [InlineData(nameof(Method_having_string_argument_with_null_default_value), "argument")]
        [InlineData(nameof(Method_having_string_array_arguments), "arguments")]
        [InlineData(nameof(Method_having_string_array_arguments_with_default_value), "arguments")]
        [InlineData(nameof(Method_having_FileInfo_argument), "argument")]
        [InlineData(nameof(Method_having_FileInfo_argument_with_default_value), "argument")]
        [InlineData(nameof(Method_having_FileInfo_array_args), "args")]
        public void Parameters_named_arguments_generate_command_arguments_having_the_correct_name(string methodName, string expectedArgName)
        {
            var config = new CliConfiguration(new CliRootCommand())
                         .ConfigureRootCommandFromMethod(GetMethodInfo(methodName));

            var rootCommandArgument = config.RootCommand.Arguments.Single();

            rootCommandArgument.Name
                               .Should()
                               .Be(expectedArgName);
        }

        [Theory]
        [InlineData(nameof(Method_having_string_argument))]
        [InlineData(nameof(Method_having_string_argument_with_null_default_value))]
        [InlineData(nameof(Method_having_string_array_arguments))]
        [InlineData(nameof(Method_having_string_array_arguments_with_default_value))]
        [InlineData(nameof(Method_having_FileInfo_argument))]
        [InlineData(nameof(Method_having_FileInfo_argument_with_default_value))]
        [InlineData(nameof(Method_having_FileInfo_array_args))]
        public void Options_are_not_generated_for_command_argument_parameters(string methodName)
        {
            var config = new CliConfiguration(new CliRootCommand())
                         .ConfigureRootCommandFromMethod(GetMethodInfo(methodName));

            var rootCommand = config.RootCommand;

            var argumentParameterNames = new[]
                                         {
                                             "arguments",
                                             "argument",
                                             "args"
                                         };

            rootCommand.Options
                       .Should()
                       .NotContain(o => argumentParameterNames.Contains(o.Name));
        }

        [Theory]
        [InlineData(nameof(Method_having_string_argument), typeof(string))]
        [InlineData(nameof(Method_having_string_argument_with_null_default_value), typeof(string))]
        [InlineData(nameof(Method_having_string_array_arguments), typeof(string[]))]
        [InlineData(nameof(Method_having_string_array_arguments_with_default_value), typeof(string[]))]
        [InlineData(nameof(Method_having_FileInfo_argument), typeof(FileInfo))]
        [InlineData(nameof(Method_having_FileInfo_argument_with_default_value), typeof(FileInfo))]
        [InlineData(nameof(Method_having_FileInfo_array_args), typeof(FileInfo[]))]
        public void Parameters_named_arguments_generate_command_arguments_having_the_correct_type(
            string methodName,
            Type expectedType)
        {
            var config = new CliConfiguration(new CliRootCommand())
                         .ConfigureRootCommandFromMethod(GetMethodInfo(methodName));

            var rootCommandArgument = config.RootCommand.Arguments.Single();

            rootCommandArgument.ValueType
                               .Should()
                               .Be(expectedType);
        }

        [Fact]
        public async Task When_method_returns_void_then_return_code_is_0()
        {
            var config = new CliConfiguration(new CliRootCommand())
                         .ConfigureRootCommandFromMethod(
                             GetMethodInfo(nameof(Method_returning_void)), this);
            config.Output = TextWriter.Null;

            var result = await config.InvokeAsync("");

            result.Should().Be(0);
        }

        [Fact]
        public async Task When_method_returns_int_then_return_code_is_set_to_return_value()
        {
            var config = new CliConfiguration(new CliRootCommand())
                         .ConfigureRootCommandFromMethod(
                             GetMethodInfo(nameof(Method_returning_int)), this);
            config.Output = TextWriter.Null;

            var result = await config.InvokeAsync("-i 123");

            result.Should().Be(123);
        }

        [Fact]
        public async Task When_method_returns_Task_of_int_then_return_code_is_set_to_return_value()
        {
            var config = new CliConfiguration(new CliRootCommand())
                         .ConfigureRootCommandFromMethod(
                             GetMethodInfo(nameof(Method_returning_Task_of_int)), this);
            config.Output = TextWriter.Null;

            var result = await config.InvokeAsync("-i 123");

            result.Should().Be(123);
        }

        [Theory]
        [InlineData(typeof(BindingContext))]
        [InlineData(typeof(ParseResult))]
        [InlineData(typeof(CancellationToken))]
        public void Options_are_not_built_for_infrastructure_types_exposed_by_method_parameters(Type type)
        {
            var targetType = typeof(ClassWithMethodHavingParameter<>).MakeGenericType(type);

            var handlerMethod = targetType.GetMethod(nameof(ClassWithMethodHavingParameter<int>.Handle));

            var options = handlerMethod.BuildOptions();

            options.Should()
                   .NotContain(o => o.GetType().IsAssignableTo(typeof(CliOption<>).MakeGenericType(type)));
        }

        [Fact]
        public async Task Method_parameters_on_the_invoked_member_method_are_bound_to_matching_option_names_by_MethodInfo_with_target()
        {
            var command = new CliCommand("test");
            command.ConfigureFromMethod(GetMethodInfo(nameof(Method_taking_bool)), this);

            await command.Parse("--value").InvokeAsync();

            _receivedValues.Should().BeEquivalentTo(true);
        }

        [Fact]
        public async Task Method_with_multiple_parameters_with_default_values_are_resolved_correctly()
        {
            var command = new CliCommand("test");
            command.ConfigureFromMethod(GetMethodInfo(nameof(Method_with_multiple_default_values)), this);

            await command.Parse("").InvokeAsync();

            _receivedValues.Should().BeEquivalentTo(1, 2);
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

        internal void Method_having_string_argument(string stringOption, int intOption, string argument)
        {
        }

        internal void Method_having_string_argument_with_null_default_value(string stringOption, int intOption, string argument = null)
        {
        }

        internal void Method_having_string_argument_with_non_null_default_value(string stringOption, int intOption, string argument = "the-default-value")
        {
        }

        internal void Method_having_string_array_arguments(string stringOption, int intOption, string[] arguments)
        {
        }

        internal void Method_having_string_array_arguments_with_default_value(string stringOption, int intOption, string[] arguments = null)
        {
        }

        internal void Method_having_FileInfo_argument(string stringOption, int intOption, FileInfo argument)
        {
        }

        internal void Method_having_FileInfo_argument_with_default_value(string stringOption, int intOption, FileInfo argument = null)
        {
        }

        internal void Method_having_FileInfo_array_args(string stringOption, int intOption, FileInfo[] args)
        {
        }

        internal void Method_with_multiple_default_values(int firstValue = 1, int secondValue = 2)
        {
            _receivedValues = new object[] { firstValue, secondValue };
        }

        private MethodInfo GetMethodInfo(string name)
        {
            return typeof(ConfigureFromMethodTests)
                   .GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)
                   .Single(m => m.Name == name);
        }
    }
}
