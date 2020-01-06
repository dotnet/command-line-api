﻿// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Generic;
using System.CommandLine.Binding;
using System.CommandLine.Invocation;
using System.CommandLine.IO;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using FluentAssertions;
using Xunit;

namespace System.CommandLine.Tests.Binding
{
    public class ModelBindingCommandHandlerTests
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

            var command = new Command("the-command")
                          {
                              new Option("--value")
                              {
                                  Argument = new Argument
                                             {
                                                 Name = "value",
                                                 ArgumentType = type
                                             }
                              }
                          };

            var console = new TestConsole();

            await handler.InvokeAsync(
                new InvocationContext(command.Parse(commandLine), console));

            console.Out.ToString().Should().Be(expectedValue.ToString());
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

            var command = new Command("the-command")
                          {
                              new Option("--value")
                              {
                                  Argument = new Argument
                                             {
                                                 Name = "value",
                                                 ArgumentType = type
                                             }
                              }
                          };

            var console = new TestConsole();

            await handler.InvokeAsync(
                new InvocationContext(command.Parse(commandLine), console));

            console.Out.ToString().Should().Be($"ClassWithSetter<{type.Name}>: {expectedValue}");
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

            var command = new Command("the-command")
                          {
                              new Option("--value")
                              {
                                  Argument = new Argument
                                             {
                                                 Name = "value",
                                                 ArgumentType = type
                                             }
                              }
                          };

            var console = new TestConsole();

            await handler.InvokeAsync(
                new InvocationContext(command.Parse(commandLine), console));

            console.Out.ToString().Should().Be($"ClassWithCtorParameter<{type.Name}>: {expectedValue}");
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

            var command = new Command("the-command")
                          {
                              Argument = new Argument
                                         {
                                             Name = "value",
                                             ArgumentType = type
                                         }
                          };

            var console = new TestConsole();

            await handler.InvokeAsync(
                new InvocationContext(command.Parse(commandLine), console));

            console.Out.ToString().Should().Be(expectedValue.ToString());
        }

        [Theory]
        [InlineData(typeof(string), null)]
        [InlineData(typeof(FileInfo), null)]
        [InlineData(typeof(int), 0)]
        [InlineData(typeof(int?), null)]
        public async Task Unspecified_option_arguments_with_no_default_value_are_bound_to_type_default(
            Type parameterType,
            object expectedValue)
        {
            var captureMethod = GetType()
                                .GetMethod(nameof(CaptureMethod), BindingFlags.NonPublic | BindingFlags.Static)
                                .MakeGenericMethod(parameterType);

            var handler = CommandHandler.Create(captureMethod);

            var command = new Command("command")
                          {
                              new Option("-x")
                              {
                                  Argument = new Argument
                                  {
                                      Name = "value",
                                      ArgumentType = parameterType
                                  }
                              }
                          };

            command.Handler = handler;

            var parseResult = command.Parse("");

            var invocationContext = new InvocationContext(parseResult);

            await handler.InvokeAsync(invocationContext);

            var boundValue = ((BoundValueCapturer)invocationContext.InvocationResult).BoundValue;

            boundValue.Should().Be(expectedValue);
        }

        [Fact]
        public async Task When_argument_type_is_not_known_until_binding_then_bool_parameter_is_bound_correctly()
        {
            bool? received = null;

            var handler = CommandHandler.Create((bool x) =>
            {
                received = x;
            });

            var root = new RootCommand
            {
                new Option("-x")
            };
            root.Handler = handler;

            await root.InvokeAsync("-x");

            received.Should().BeTrue();
        }

        [Fact]
        public async Task When_argument_type_is_not_known_until_binding_then_int_parameter_is_bound_correctly()
        {
            int received = 0;

            var handler = CommandHandler.Create((int x) =>
            {
                received = x;
            });

            var root = new RootCommand
            {
                new Option("-x")
                {
                    Argument = new Argument
                    {
                        Arity = new ArgumentArity(1, 1)
                    }
                }
            };
            root.Handler = handler;

            await root.InvokeAsync("-x 123");

            received.Should().Be(123);
        }

        [Theory]
        [InlineData(typeof(ClassWithCtorParameter<int>), false)]
        [InlineData(typeof(ClassWithCtorParameter<int>), true)]
        [InlineData(typeof(ClassWithSetter<int>), false)]
        [InlineData(typeof(ClassWithSetter<int>), true)]
        [InlineData(typeof(ClassWithCtorParameter<string>), false)]
        [InlineData(typeof(ClassWithCtorParameter<string>), true)]
        [InlineData(typeof(ClassWithSetter<string>), false)]
        [InlineData(typeof(ClassWithSetter<string>), true)]
        [InlineData(typeof(FileInfo), false)]
        [InlineData(typeof(FileInfo), true)]
        [InlineData(typeof(FileInfo[]), false)]
        [InlineData(typeof(FileInfo[]), true)]
        [InlineData(typeof(string[]), false)]
        [InlineData(typeof(string[]), true)]
        [InlineData(typeof(List<string>), false)]
        [InlineData(typeof(List<string>), true)]
        [InlineData(typeof(int[]), false)]
        [InlineData(typeof(int[]), true)]
        [InlineData(typeof(List<int>), false)]
        [InlineData(typeof(List<int>), true)]
        public async Task Handler_method_receives_option_arguments_bound_to_the_specified_type(
            Type type,
            bool useDelegate)
        {
            var testCase = _bindingCases[type];

            ICommandHandler handler;
            if (!useDelegate)
            {
                var captureMethod = GetType()
                                    .GetMethod(nameof(CaptureMethod), BindingFlags.NonPublic | BindingFlags.Static)
                                    .MakeGenericMethod(testCase.ParameterType);

                handler = CommandHandler.Create(captureMethod);
            }
            else
            {
                 var createCaptureDelegate = GetType()
                                    .GetMethod(nameof(CaptureDelegate), BindingFlags.NonPublic | BindingFlags.Static)
                                    .MakeGenericMethod(testCase.ParameterType);

                 var @delegate = createCaptureDelegate.Invoke(null, null);

                 handler = CommandHandler.Create((dynamic) @delegate);
            }

            var command = new Command("command")
                          {
                              new Option("--value")
                              {
                                  Argument = new Argument
                                             {
                                                 ArgumentType = testCase.ParameterType
                                             }
                              }
                          };
            command.Handler = handler;

            var parseResult = command.Parse($"--value {testCase.CommandLine}");

            var invocationContext = new InvocationContext(parseResult);

            await handler.InvokeAsync(invocationContext);

            var boundValue = ((BoundValueCapturer)invocationContext.InvocationResult).BoundValue;

            boundValue.Should().BeOfType(testCase.ParameterType);

            testCase.AssertBoundValue(boundValue);
        }
        
        [Fact]
        public async Task When_binding_fails_due_to_parameter_naming_mismatch_then_handler_is_called_and_no_error_is_produced()
        {
            string[] received = { "this should get overwritten" };

            var o = new Option(
                new[] {  "-i" },
                "Path to an image or directory of supported images")
            {
                Argument = new Argument<string[]>()
            };

            var command = new Command("command") { o };
            command.Handler = CommandHandler.Create<string[], InvocationContext>((nameDoesNotMatch, c) =>
            {
                received = nameDoesNotMatch;
            });

            var testConsole = new TestConsole();
            var commandLine = "command -i 1 -i 2 -i 3 ";

            await command.InvokeAsync(commandLine, testConsole);

            testConsole.Error.ToString().Should().BeEmpty();

            received.Should().BeNull();
        }

        [Theory]
        [InlineData(typeof(ClassWithCtorParameter<int>))]
        [InlineData(typeof(ClassWithSetter<int>))]
        [InlineData(typeof(ClassWithCtorParameter<string>))]
        [InlineData(typeof(ClassWithSetter<string>))]
        [InlineData(typeof(FileInfo))]
        [InlineData(typeof(FileInfo[]))]
        [InlineData(typeof(string[]))]
        [InlineData(typeof(List<string>))]
        [InlineData(typeof(int[]))]
        [InlineData(typeof(List<int>))]
        public async Task Handler_method_receives_command_arguments_bound_to_the_specified_type(
          Type type)
        {
            var c = _bindingCases[type];

            var captureMethod = GetType()
                                .GetMethod(nameof(CaptureMethod), BindingFlags.NonPublic | BindingFlags.Static)
                                .MakeGenericMethod(c.ParameterType);

            var handler = CommandHandler.Create(captureMethod);

            var command = new Command(
                "command")
            {
                new Argument
                {
                    Name = "value",
                    ArgumentType = c.ParameterType
                }
            };
            command.Handler = handler;

            var parseResult = command.Parse(c.CommandLine);

            var invocationContext = new InvocationContext(parseResult);

            await handler.InvokeAsync(invocationContext);

            var boundValue = ((BoundValueCapturer)invocationContext.InvocationResult).BoundValue;

            boundValue.Should().BeOfType(c.ParameterType);

            c.AssertBoundValue(boundValue);
        }

        private static void CaptureMethod<T>(T value, InvocationContext invocationContext)
        {
            invocationContext.InvocationResult = new BoundValueCapturer(value);
        }

        private static Action<T, InvocationContext> CaptureDelegate<T>()
        {
            return (value, invocationContext) =>
            {
                invocationContext.InvocationResult = new BoundValueCapturer(value);
            };
        }

        private class BoundValueCapturer : IInvocationResult
        {
            public BoundValueCapturer(object boundValue)
            {
                BoundValue = boundValue;
            }

            public object BoundValue { get; }

            public void Apply(InvocationContext context)
            {
            }
        }

        private static readonly BindingTestSet _bindingCases = new BindingTestSet
        {
              BindingTestCase.Create<ClassWithCtorParameter<int>>(
                 "123",
                 o => o.Value.Should().Be(123)),
            
              BindingTestCase.Create<ClassWithSetter<int>>(
                 "123",
                 o => o.Value.Should().Be(123)),

            BindingTestCase.Create<ClassWithCtorParameter<string>>(
                "123",
                o => o.Value.Should().Be("123")),

            BindingTestCase.Create<ClassWithSetter<string>>(
                "123",
                o => o.Value.Should().Be("123")),

            BindingTestCase.Create<FileInfo>(
                Path.Combine(Directory.GetCurrentDirectory(), "file1.txt"),
                o => o.FullName.Should().Be(Path.Combine(Directory.GetCurrentDirectory(), "file1.txt"))),

            BindingTestCase.Create<FileInfo[]>(
                $"{Path.Combine(Directory.GetCurrentDirectory(), "file1.txt")} {Path.Combine(Directory.GetCurrentDirectory(), "file2.txt")}",
                o => o.Select(f => f.FullName)
                      .Should()
                      .BeEquivalentTo(new[] { Path.Combine(Directory.GetCurrentDirectory(), "file1.txt"), Path.Combine(Directory.GetCurrentDirectory(), "file2.txt") })),

            BindingTestCase.Create<string[]>(
                "one two",
                o => o.Should().BeEquivalentTo(new[] { "one", "two" })),

            BindingTestCase.Create<List<string>>(
                "one two",
                o => o.Should().BeEquivalentTo(new List<string> { "one", "two" })),

            BindingTestCase.Create<int[]>(
                "1 2",
                o => o.Should().BeEquivalentTo(new[] { 1, 2 })),

            BindingTestCase.Create<List<int>>(
                "1 2",
                o => o.Should().BeEquivalentTo(new List<int> { 1, 2 }))
        };
    }
}
