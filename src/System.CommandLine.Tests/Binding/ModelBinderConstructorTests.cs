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
        public async Task Handler_constructor_receives_option_arguments_bound_to_the_specified_type(
            Type type)
        {
            var testCase = _bindingCases[type];
            ICommandHandler handler = CommandHandler.Create(
                                                MakeGenericType(typeof(ClassForCaptureMethod<>), testCase.ParameterType)
                                                    .GetMethod(nameof(ClassForCaptureMethod<int>.Invoke)));
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
            boundValue.Should().BeAssignableTo(testCase.ParameterType);
            testCase.AssertBoundValue(boundValue);
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
        public async Task Constructor_receives_option_arguments_bound_to_the_specified_type(
                    Type type)
        {
            var testCase = _bindingCases[type];
            var typeToCreate = MakeGenericType(typeof(ClassForCreate<>), testCase.ParameterType);
            Command command = GetSingleArgumentCommand(testCase);

            var binder = new ModelBinder(typeToCreate);
            var commandLine = $"--value {testCase.CommandLine}";
            var bindingContext = new BindingContext(command.Parse(commandLine));
            var instance = binder.CreateInstance(bindingContext) as ClassForCreateBase;

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

        private class ClassForCaptureMethod<T>
        {
            public ClassForCaptureMethod(T value, InvocationContext invocationContext)
            {
                invocationContext.InvocationResult = new BoundValueCapturer(value);
            }

            public void Invoke() { }
        }

        private class ClassForCreateBase
        {
            public object? Value { get; protected set; }

        }
        private class ClassForCreate<T> : ClassForCreateBase
        {
            public ClassForCreate(T value)
            {
                Value = value;
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
                Path.Combine(ExistingDirectory(), "file1.txt"),
                o => o.FullName
                      .Should()
                      .Be(Path.Combine(ExistingDirectory(), "file1.txt"))),

            BindingTestCase.Create<FileInfo[]>(
                $"{Path.Combine(ExistingDirectory(), "file1.txt")} {Path.Combine(ExistingDirectory(), "file2.txt")}",
                o => o.Select(f => f.FullName)
                      .Should()
                      .BeEquivalentTo(new[]
                      {
                          Path.Combine(ExistingDirectory(), "file1.txt"),
                          Path.Combine(ExistingDirectory(), "file2.txt")
                      })),

            BindingTestCase.Create<DirectoryInfo>(
                ExistingDirectory(),
                fsi => fsi.Should()
                          .BeOfType<DirectoryInfo>()
                          .Which
                          .FullName
                          .Should()
                          .Be(ExistingDirectory())),

            BindingTestCase.Create<DirectoryInfo[]>(
                $"{ExistingDirectory()} {ExistingDirectory()}",
                fsi => fsi.Should()
                          .BeAssignableTo<IEnumerable<DirectoryInfo>>()
                          .Which
                          .Select(d => d.FullName)
                          .Should()
                          .BeEquivalentTo(new[]
                          {
                              ExistingDirectory(),
                              ExistingDirectory()
                          })),

            BindingTestCase.Create<FileSystemInfo>(
                ExistingFile(),
                fsi => fsi.Should()
                          .BeOfType<FileInfo>()
                          .Which
                          .FullName
                          .Should()
                          .Be(ExistingFile()),
                variationName: nameof(ExistingFile)),

            BindingTestCase.Create<FileSystemInfo>(
                ExistingDirectory(),
                fsi => fsi.Should()
                          .BeOfType<DirectoryInfo>()
                          .Which
                          .FullName
                          .Should()
                          .Be(ExistingDirectory()),
                variationName: nameof(ExistingDirectory)),

            BindingTestCase.Create<FileSystemInfo>(
                NonexistentPathWithTrailingSlash(),
                fsi => fsi.Should()
                          .BeOfType<DirectoryInfo>()
                          .Which
                          .FullName
                          .Should()
                          .Be(NonexistentPathWithTrailingSlash()),
                variationName: nameof(NonexistentPathWithTrailingSlash)),

            BindingTestCase.Create<FileSystemInfo>(
                NonexistentPathWithTrailingAltSlash(),
                fsi => fsi.Should()
                          .BeOfType<DirectoryInfo>()
                          .Which
                          .FullName
                          .Should()
                          .Be(NonexistentPathWithTrailingSlash(),
                              "DirectoryInfo replaces Path.AltDirectorySeparatorChar with Path.DirectorySeparatorChar on Windows"),
                variationName: nameof(NonexistentPathWithTrailingAltSlash)),

            BindingTestCase.Create<FileSystemInfo>(
                NonexistentPathWithoutTrailingSlash(),
                fsi => fsi.Should()
                          .BeOfType<FileInfo>()
                          .Which
                          .FullName
                          .Should()
                          .Be(NonexistentPathWithoutTrailingSlash()),
                variationName: nameof(NonexistentPathWithoutTrailingSlash)),

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

        private static string NonexistentPathWithoutTrailingSlash()
        {
            return Path.Combine(
                ExistingDirectory(),
                "does-not-exist");
        }

        private static string NonexistentPathWithTrailingSlash() =>
            NonexistentPathWithoutTrailingSlash() + Path.DirectorySeparatorChar;
        private static string NonexistentPathWithTrailingAltSlash() =>
            NonexistentPathWithoutTrailingSlash() + Path.AltDirectorySeparatorChar;

        private static string ExistingFile() =>
            Directory.GetFiles(ExistingDirectory()).FirstOrDefault() ??
            throw new AssertionFailedException("No files found in current directory");

        private static string ExistingDirectory() => Directory.GetCurrentDirectory();
    }
}
