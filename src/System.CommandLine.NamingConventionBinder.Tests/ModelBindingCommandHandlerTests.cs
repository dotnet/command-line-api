// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Generic;
using System.CommandLine.Invocation;
using System.CommandLine.Tests.Binding;
using System.CommandLine.Utility;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using FluentAssertions.Execution;
using Xunit;

namespace System.CommandLine.NamingConventionBinder.Tests;

public partial class ModelBindingCommandHandlerTests
{
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

        var command = new CliCommand("command")
        {
            OptionBuilder.CreateOption("-x", parameterType)
        };

        command.Action = handler;

        var parseResult = command.Parse("");

        await handler.InvokeAsync(parseResult, CancellationToken.None);

        BoundValueCapturer.GetBoundValue(parseResult).Should().Be(expectedValue);
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

    [InlineData(typeof(DirectoryInfo), false)]
    [InlineData(typeof(DirectoryInfo), true)]
    [InlineData(typeof(DirectoryInfo[]), false)]
    [InlineData(typeof(DirectoryInfo[]), true)]

    [InlineData(typeof(FileSystemInfo), true, nameof(ExistingFile))]
    [InlineData(typeof(FileSystemInfo), true, nameof(ExistingDirectory))]
    [InlineData(typeof(FileSystemInfo), true, nameof(NonexistentPathWithTrailingSlash))]
    [InlineData(typeof(FileSystemInfo), true, nameof(NonexistentPathWithTrailingAltSlash))]
    [InlineData(typeof(FileSystemInfo), true, nameof(NonexistentPathWithoutTrailingSlash))]

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
        bool useDelegate,
        string variation = null)
    {
        var testCase = BindingCases[(type, variation)];

        AsynchronousCliAction handler;
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

            handler = CommandHandler.Create((Delegate)@delegate);
        }

        var command = new CliCommand("command")
        {
            OptionBuilder.CreateOption("--value", testCase.ParameterType)
        };
        command.Action = handler;

        var commandLine = string.Join(" ", testCase.CommandLineTokens.Select(t => $"--value {t}"));
        var parseResult = command.Parse(commandLine);

        await handler.InvokeAsync(parseResult, CancellationToken.None);

        var boundValue = BoundValueCapturer.GetBoundValue(parseResult);

        boundValue.Should().BeAssignableTo(testCase.ParameterType);

        testCase.AssertBoundValue(boundValue);
    }

    [Fact]
    public async Task When_binding_fails_due_to_parameter_naming_mismatch_then_handler_is_called_and_no_error_is_produced()
    {
        string[] received = { "this should get overwritten" };

        var o = new CliOption<string[]>("-i") { Description = "Path to an image or directory of supported images" };

        var command = new CliCommand("command") { o };
        command.Action = CommandHandler.Create<string[], ParseResult>((nameDoesNotMatch, c) => received = nameDoesNotMatch);
        CliConfiguration config = new(command);
        config.Error = new StringWriter();

        var commandLine = "command -i 1 -i 2 -i 3 ";

        await command.Parse(commandLine, config).InvokeAsync();

        config.Error.ToString().Should().BeEmpty();

        received.Should().BeEmpty();
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
    public async Task Handler_method_receives_command_arguments_bound_to_the_specified_type(Type type)
    {
        var testCase = BindingCases[type];

        var captureMethod = GetType()
                            .GetMethod(nameof(CaptureMethod), BindingFlags.NonPublic | BindingFlags.Static)
                            .MakeGenericMethod(testCase.ParameterType);

        var handler = CommandHandler.Create(captureMethod);

        var command = new CliCommand("command")
        {
            ArgumentBuilder.CreateArgument(type)
        };
        command.Action = handler;

        var commandLine = string.Join(" ", testCase.CommandLineTokens);
        var parseResult = command.Parse(commandLine);

        await handler.InvokeAsync(parseResult, CancellationToken.None);

        var boundValue = BoundValueCapturer.GetBoundValue(parseResult);

        boundValue.Should().BeOfType(testCase.ParameterType);

        testCase.AssertBoundValue(boundValue);
    }

    [Theory]
    [InlineData(typeof(int))]
    [InlineData(typeof(string))]
    [InlineData(typeof(bool))]
    [InlineData(typeof(ClassWithSetter<int>))]
    [InlineData(typeof(ClassWithCtorParameter<string>))]
    [InlineData(typeof(ClassWithSetter<string>))]
    [InlineData(typeof(FileInfo))]
    [InlineData(typeof(FileInfo[]))]
    [InlineData(typeof(string[]))]
    [InlineData(typeof(List<string>))]
    [InlineData(typeof(int[]))]
    [InlineData(typeof(List<int>))]
    public async Task Handler_method_receives_command_arguments_explicitly_bound_to_the_specified_type(
        Type type)
    {
        var testCase = BindingCases[type];

        var captureMethod = GetType()
                            .GetMethod(nameof(CaptureMethod), BindingFlags.NonPublic | BindingFlags.Static)
                            .MakeGenericMethod(testCase.ParameterType);
        var parameter = captureMethod.GetParameters()[0];

        var handler = CommandHandler.Create(captureMethod);

        var argument = ArgumentBuilder.CreateArgument(testCase.ParameterType, "value");

        var command = new CliCommand("command")
        {
            argument
        };
        if (handler is not ModelBindingCommandHandler bindingHandler)
        {
            throw new InvalidOperationException("Cannot bind to this type of handler");
        }
        bindingHandler.BindParameter(parameter, argument);
        command.Action = handler;

        var commandLine = string.Join(" ", testCase.CommandLineTokens);
        var parseResult = command.Parse(commandLine);

        await handler.InvokeAsync(parseResult, CancellationToken.None);

        var boundValue = BoundValueCapturer.GetBoundValue(parseResult);

        boundValue.Should().BeOfType(testCase.ParameterType);

        testCase.AssertBoundValue(boundValue);
    }

    [Theory]
    [InlineData(typeof(int))]
    [InlineData(typeof(string))]
    [InlineData(typeof(bool))]
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
    public async Task Handler_method_receive_option_arguments_explicitly_bound_to_the_specified_type(
        Type type)
    {
        var testCase = BindingCases[type];

        var captureMethod = GetType()
                            .GetMethod(nameof(CaptureMethod), BindingFlags.NonPublic | BindingFlags.Static)
                            .MakeGenericMethod(testCase.ParameterType);
        var parameter = captureMethod.GetParameters()[0];

        var handler = CommandHandler.Create(captureMethod);

        var option = OptionBuilder.CreateOption("--value", testCase.ParameterType);

        var command = new CliCommand("command")
        {
            option
        };
        if (handler is not ModelBindingCommandHandler bindingHandler)
        {
            throw new InvalidOperationException("Cannot bind to this type of handler");
        }
        bindingHandler.BindParameter(parameter, option);
        command.Action = handler;

        var commandLine = string.Join(" ", testCase.CommandLineTokens.Select(t => $"--value {t}"));
        var parseResult = command.Parse(commandLine);

        await handler.InvokeAsync(parseResult, CancellationToken.None);

        var boundValue = BoundValueCapturer.GetBoundValue(parseResult);

        boundValue.Should().BeOfType(testCase.ParameterType);

        testCase.AssertBoundValue(boundValue);
    }

    [Fact]
    public async Task Unexpected_return_types_result_in_exit_code_0_if_no_exception_was_thrown()
    {
        var wasCalled = false;

        Delegate @delegate = () =>
        {
            wasCalled = true;
            return true;
        };

        var command = new CliCommand("wat")
        {
            Action = CommandHandler.Create(@delegate)
        };

        var exitCode = await command.Parse("").InvokeAsync();
        wasCalled.Should().BeTrue();
        exitCode.Should().Be(0);
    }

    private static void CaptureMethod<T>(T value, ParseResult parseResult)
    {
        BoundValueCapturer.Capture(value, parseResult);
    }

    private static Action<T, ParseResult> CaptureDelegate<T>()
        => (value, parseResult) =>
        {
            BoundValueCapturer.Capture(value, parseResult);
        };

    private static class BoundValueCapturer
    {
        private static readonly Dictionary<ParseResult, object> _boundValues = new ();

        public static void Apply(ParseResult context)
        {
        }

        public static void Capture(object value, ParseResult invocationContext)
            => _boundValues.Add(invocationContext, value);

        public static object GetBoundValue(ParseResult context)
            => _boundValues.TryGetValue(context, out var value) ? value : null;
    }

    internal static readonly BindingTestSet BindingCases = new()
    {
        BindingTestCase.Create<int>(
            "123",
            o => o.Should().Be(123)),

        BindingTestCase.Create<string>(
            "123",
            o => o.Should().Be("123")),
        BindingTestCase.Create<bool>(
            "true",
            o => o.Should().BeTrue()),
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
            new[] { 
                Path.Combine(ExistingDirectory(), "file1.txt"), 
                Path.Combine(ExistingDirectory(), "file2.txt")
            } ,
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
            new[] { ExistingDirectory(), ExistingDirectory() },
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
            new[] { "one", "two" },
            o => o.Should().BeEquivalentTo(new[] { "one", "two" })),

        BindingTestCase.Create<List<string>>(
            new[] { "one", "two" },
            o => o.Should().BeEquivalentTo(new List<string> { "one", "two" })),

        BindingTestCase.Create<int[]>(
            new[] { "1", "2" },
            o => o.Should().BeEquivalentTo(new[] { 1, 2 })),

        BindingTestCase.Create<List<int>>(
            new[] { "1", "2" },
            o => o.Should().BeEquivalentTo(new List<int> { 1, 2 }))
    };

    internal static string NonexistentPathWithoutTrailingSlash()
    {
        return Path.Combine(
            ExistingDirectory(),
            "does-not-exist");
    }

    internal static string NonexistentPathWithTrailingSlash() =>
        NonexistentPathWithoutTrailingSlash() + Path.DirectorySeparatorChar;
    internal static string NonexistentPathWithTrailingAltSlash() =>
        NonexistentPathWithoutTrailingSlash() + Path.AltDirectorySeparatorChar;

    internal static string ExistingFile() =>
        Directory.GetFiles(ExistingDirectory()).FirstOrDefault() ??
        throw new AssertionFailedException("No files found in current directory");

    internal static string ExistingDirectory() => Directory.GetCurrentDirectory();
}