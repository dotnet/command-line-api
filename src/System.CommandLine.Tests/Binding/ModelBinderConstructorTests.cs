// Copyright (c) .NET Foundation and contributors. All rights reserved.
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
using FluentAssertions.Execution;
using Xunit;
using common = System.CommandLine.Tests.Binding.ModelBindingCommandHandlerTests;

namespace System.CommandLine.Tests.Binding
{
    public class ModelBinderConstructorTests
    {

        [Theory]
        //[InlineData(typeof(ClassWithCtorParameter<int>))]
        [InlineData(typeof(ClassWithSetter<int>))]
        //[InlineData(typeof(ClassWithCtorParameter<string>))]
        [InlineData(typeof(ClassWithSetter<string>))]
        [InlineData(typeof(FileInfo))]
        [InlineData(typeof(FileInfo[]))]
        [InlineData(typeof(string[]))]
        [InlineData(typeof(List<string>))]
        [InlineData(typeof(int[]))]
        [InlineData(typeof(List<int>))]
        public async Task Handler_constructor_receives_option_arguments_bound_to_the_specified_type(
            Type type)
        {
            var testCase = common.BindingCases[type];
            ICommandHandler handler = CommandHandler.Create(
                                                MakeGenericType(typeof(TesttCommandHandler<>), testCase.ParameterType)
                                                    .GetMethod(nameof(TesttCommandHandler<int>.Invoke)));
            Command command = GetSingleArgumentCommand(testCase);
            command.Handler = handler;

            var parseResult = command.Parse($"--value {testCase.CommandLine}");
            var invocationContext = new InvocationContext(parseResult);
            await handler.InvokeAsync(invocationContext);

            var boundValue = ((BoundValueCapturer)invocationContext.InvocationResult).BoundValue;
            boundValue.Should().BeAssignableTo(testCase.ParameterType);
            testCase.AssertBoundValue(boundValue);
        }

        [Theory]
        //[InlineData(typeof(ClassWithCtorParameter<int>))]
        //[InlineData(typeof(ClassWithSetter<int>))]
        //[InlineData(typeof(ClassWithCtorParameter<string>))]
        //[InlineData(typeof(ClassWithSetter<string>))]
        [InlineData(typeof(FileInfo))]
        [InlineData(typeof(FileInfo[]))]
        [InlineData(typeof(string[]))]
        [InlineData(typeof(List<string>))]
        [InlineData(typeof(int[]))]
        [InlineData(typeof(List<int>))]
        public async Task Model_constructor_receives_option_arguments_bound_to_the_specified_type(
                    Type type)
        {
            var testCase = common.BindingCases[type];
            var typeToCreate = MakeGenericType(typeof(TestModel<>), testCase.ParameterType);
            Command command = GetSingleArgumentCommand(testCase);

            var binder = new ModelBinder(typeToCreate);
            var commandLine = $"--value {testCase.CommandLine}";
            var bindingContext = new BindingContext(command.Parse(commandLine));
            var instance = binder.CreateInstance(bindingContext) as TestModelBase;

            instance.Value.Should().BeAssignableTo(testCase.ParameterType);
            testCase.AssertBoundValue(instance.Value);
        }

        private static Command GetSingleArgumentCommand(BindingTestCase testCase)
        {
            return new Command("command")
                          {
                              new Option("--value")
                              {
                                  Argument = new Argument
                                             {
                                                 ArgumentType = testCase.ParameterType
                                             }
                              }
                          };
        }

        private MethodInfo MakeGenericMethod(Type type, string methodName, params Type[] typeParameters)
            => type.GetMethod(methodName, BindingFlags.Public | BindingFlags.Instance)
                       .MakeGenericMethod(typeParameters);

        private Type MakeGenericType(Type openType, params Type[] typeParameters)
            => openType.MakeGenericType(typeParameters);

        private static void CaptureMethod<T>(T value, InvocationContext invocationContext)
        {
            invocationContext.InvocationResult = new BoundValueCapturer(value);
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

        private class TesttCommandHandler<T>
        {
            public TesttCommandHandler(T value, InvocationContext invocationContext)
            {
                invocationContext.InvocationResult = new BoundValueCapturer(value);
            }

            public void Invoke() { }
        }

        private class TestModelBase
        {
            public object? Value { get; protected set; }

        }
        private class TestModel<T> : TestModelBase
        {
            public TestModel(T value)
            {
                Value = value;
            }

        }
    }
}
