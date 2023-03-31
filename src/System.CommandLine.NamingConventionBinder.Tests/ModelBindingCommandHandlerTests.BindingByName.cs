// // Copyright (c) .NET Foundation and contributors. All rights reserved.
// // Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.CommandLine.Tests.Binding;
using System.CommandLine.Utility;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Xunit;

namespace System.CommandLine.NamingConventionBinder.Tests;

public partial class ModelBindingCommandHandlerTests
{
    public class BindingByName
    {
        [Theory]
        [InlineData(typeof(bool), "--value", true)]
        [InlineData(typeof(bool), "--value false", false)]
        [InlineData(typeof(string), "--value hello", "hello")]
        [InlineData(typeof(int), "--value 123", 123)]
        public async Task Option_arguments_are_bound_by_name_to_method_parameters(
            Type type,
            string commandLine,
            object expectedValue)
        {
            var targetType = typeof(ClassWithMethodHavingParameter<>).MakeGenericType(type);

            var handlerMethod = targetType.GetMethod(nameof(ClassWithMethodHavingParameter<int>.HandleAsync));

            var handler = HandlerDescriptor.FromMethodInfo(handlerMethod)
                                           .GetCommandHandler();

            var command = new CliCommand("the-command")
            {
                OptionBuilder.CreateOption("--value", type)
            };
            command.Action = handler;
            CliConfiguration configuration = new(command)
            {
                Output = new StringWriter()
            };

            await command.Parse(commandLine, configuration).InvokeAsync(CancellationToken.None);

            configuration.Output.ToString().Should().Be(expectedValue.ToString());
        }

        [Theory]
        [InlineData(typeof(bool), "--value", true)]
        [InlineData(typeof(bool), "--value false", false)]
        [InlineData(typeof(string), "--value hello", "hello")]
        [InlineData(typeof(int), "--value 123", 123)]
        public async Task Option_arguments_are_bound_by_name_to_the_properties_of_method_parameters(
            Type type,
            string commandLine,
            object expectedValue)
        {
            var complexParameterType = typeof(ClassWithSetter<>).MakeGenericType(type);

            var handlerType = typeof(ClassWithMethodHavingParameter<>).MakeGenericType(complexParameterType);

            var handlerMethod = handlerType.GetMethod("HandleAsync");

            var handler = HandlerDescriptor.FromMethodInfo(handlerMethod)
                                           .GetCommandHandler();

            var command = new CliCommand("the-command")
            {
                OptionBuilder.CreateOption("--value", type)
            };
            command.Action = handler;
            CliConfiguration configuration = new(command)
            {
                Output = new StringWriter()
            };

            await command.Parse(commandLine, configuration).InvokeAsync(CancellationToken.None);

            configuration.Output.ToString().Should().Be($"ClassWithSetter<{type.Name}>: {expectedValue}");
        }

        [Theory]
        [InlineData(typeof(bool), "--value", true)]
        [InlineData(typeof(bool), "--value false", false)]
        [InlineData(typeof(string), "--value hello", "hello")]
        [InlineData(typeof(int), "--value 123", 123)]
        public async Task Option_arguments_are_bound_by_name_to_the_constructor_parameters_of_method_parameters(
            Type type,
            string commandLine,
            object expectedValue)
        {
            var complexParameterType = typeof(ClassWithCtorParameter<>).MakeGenericType(type);

            var handlerType = typeof(ClassWithMethodHavingParameter<>).MakeGenericType(complexParameterType);

            var handlerMethod = handlerType.GetMethod("HandleAsync");

            var handler = HandlerDescriptor.FromMethodInfo(handlerMethod)
                                           .GetCommandHandler();

            var command = new CliCommand("the-command")
            {
                OptionBuilder.CreateOption("--value", type)
            };
            command.Action = handler;
            CliConfiguration configuration = new(command)
            {
                Output = new StringWriter()
            };

            await command.Parse(commandLine, configuration).InvokeAsync(CancellationToken.None);

            configuration.Output.ToString().Should().Be($"ClassWithCtorParameter<{type.Name}>: {expectedValue}");
        }

        [Theory]
        [InlineData(typeof(string), "hello", "hello")]
        [InlineData(typeof(int), "123", 123)]
        public async Task Command_arguments_are_bound_by_name_to_handler_method_parameters(
            Type type,
            string commandLine,
            object expectedValue)
        {
            var targetType = typeof(ClassWithMethodHavingParameter<>).MakeGenericType(type);

            var handlerMethod = targetType.GetMethod(nameof(ClassWithMethodHavingParameter<int>.HandleAsync));

            var handler = HandlerDescriptor.FromMethodInfo(handlerMethod)
                                           .GetCommandHandler();

            var command = new CliCommand("the-command")
            {
                ArgumentBuilder.CreateArgument(type)
            };
            command.Action = handler;
            CliConfiguration configuration = new(command)
            {
                Output = new StringWriter()
            };

            await command.Parse(commandLine, configuration).InvokeAsync(CancellationToken.None);

            configuration.Output.ToString().Should().Be(expectedValue.ToString());
        }

        [Fact]
        public void When_argument_type_is_more_specific_than_parameter_type_then_parameter_is_bound_correctly()
        {
            FileSystemInfo received = null;

            var root = new CliRootCommand
            {
                new CliOption<DirectoryInfo>("-f")
            };
            root.Action = CommandHandler.Create<FileSystemInfo>(f => received = f);
            var path = $"{Directory.GetCurrentDirectory()}{Path.DirectorySeparatorChar}";

            root.Parse($"-f {path}").Invoke();

            received.Should()
                    .BeOfType<DirectoryInfo>()
                    .Which
                    .FullName
                    .Should()
                    .Be(path);
        }
    }
}