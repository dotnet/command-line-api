// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.CommandLine.Binding;
using System.CommandLine.Invocation;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Xunit;

namespace System.CommandLine.NamingConventionBinder.Tests;

public class ParameterBindingTests
{
    [Fact]
    public async Task Method_parameters_on_the_invoked_method_are_bound_to_matching_option_names()
    {
        string boundName = default;
        int boundAge = default;

        void Execute(string name, int age)
        {
            boundName = name;
            boundAge = age;
        }

        var command = new CliCommand("command");
        command.Options.Add(new CliOption<string>("--name"));
        command.Options.Add(new CliOption<int>("--age"));
        command.Action = CommandHandler.Create<string, int>(Execute);

        await command.Parse("command --age 425 --name Gandalf").InvokeAsync();

        boundName.Should().Be("Gandalf");
        boundAge.Should().Be(425);
    }
    
    [Fact]
    public async Task Method_parameters_on_the_invoked_method_can_be_bound_to_hyphenated_option_names()
    {
        string boundFirstName = default;

        void Execute(string firstName)
        {
            boundFirstName = firstName;
        }

        var command = new CliCommand("command")
        {
            new CliOption<string>("--first-name")
        };
        command.Action = CommandHandler.Create<string>(Execute);

        await command.Parse("command --first-name Gandalf").InvokeAsync();

        boundFirstName.Should().Be("Gandalf");
    }

    [Fact]
    public async Task Method_parameters_on_the_invoked_method_can_be_bound_to_option_names_case_insensitively()
    {
        string boundName = default;
        int boundAge = default;

        void Execute(string name, int AGE)
        {
            boundName = name;
            boundAge = AGE;
        }

        var command = new CliCommand("command");
        command.Options.Add(new CliOption<string>("--NAME"));
        command.Options.Add(new CliOption<int>("--age"));
        command.Action = CommandHandler.Create<string, int>(Execute);

        await command.Parse("command --age 425 --NAME Gandalf").InvokeAsync();

        boundName.Should().Be("Gandalf");
        boundAge.Should().Be(425);
    }

    [Fact]
    public async Task Method_is_invoked_when_command_line_does_not_specify_matching_options()
    {
        string boundName = default;
        int boundAge = default;

        void Execute(string name, int age)
        {
            boundName = name;
            boundAge = age;
        }

        var command = new CliCommand("command")
        {
            new CliOption<string>("--name"),
            new CliOption<int>("--age")
        };
        command.Action = CommandHandler.Create<string, int>(Execute);

        await command.Parse("command").InvokeAsync();

        boundName.Should().BeNull();
        boundAge.Should().Be(0);
    }

    [Fact]
    public async Task Method_parameters_on_the_invoked_method_can_be_bound_to_option_names_by_alias()
    {
        string boundName = default;
        int boundAge = default;

        void Execute(string name, int age)
        {
            boundName = name;
            boundAge = age;
        }

        var command = new CliCommand("command")
        {
            new CliOption<string>("--NAME", "-n"),
            new CliOption<int>("--age", "-a")
        };
        command.Action = CommandHandler.Create<string, int>(Execute);

        await command.Parse("command -a 425 -n Gandalf").InvokeAsync();

        boundName.Should().Be("Gandalf");
        boundAge.Should().Be(425);
    }

    [Fact]
    public async Task Method_parameters_on_the_invoked_lambda_are_bound_to_matching_option_names()
    {
        string boundName = default;
        int boundAge = default;

        var command = new CliCommand("command")
        {
            new CliOption<string>("--name"),
            new CliOption<int>("--age")
        };
        command.Action = CommandHandler.Create<string, int>((name, age) =>
        {
            boundName = name;
            boundAge = age;
        });

        await command.Parse("command --age 425 --name Gandalf").InvokeAsync();

        boundName.Should().Be("Gandalf");
        boundAge.Should().Be(425);
    }

    [Fact]
    public async Task Nullable_parameters_are_bound_to_correct_value_when_option_is_specified()
    {
        int? boundAge = default;

        var command = new CliCommand("command")
        {
            new CliOption<int?>("--age")
        };
        command.Action = CommandHandler.Create<int?>(age =>
        {
            boundAge = age;
        });

        await command.Parse("command --age 425").InvokeAsync();

        boundAge.Should().Be(425);
    }

    [Fact]
    public async Task Nullable_parameters_are_bound_to_null_when_option_is_not_specified()
    {
        var wasCalled = false;
        int? boundAge = default;

        var command = new CliCommand("command")
        {
            new CliOption<int?>("--age")
        };
        command.Action = CommandHandler.Create<int?>(age =>
        {
            wasCalled = true;
            boundAge = age;
        });

        await command.Parse("command").InvokeAsync();

        wasCalled.Should().BeTrue();
        boundAge.Should().BeNull();
    }

    [Fact]
    public async Task Method_parameters_of_types_having_constructors_accepting_a_single_string_are_bound_using_handler_parameter_name()
    {
        DirectoryInfo boundDirectoryInfo = default;
        var tempPath = Path.GetTempPath();

        var command = new CliCommand("command")
        {
            new CliOption<DirectoryInfo>("--dir")
        };
        command.Action = CommandHandler.Create<DirectoryInfo>(dir =>
        {
            boundDirectoryInfo = dir;
        });

        await command.Parse($"command --dir \"{tempPath}\"").InvokeAsync();

        boundDirectoryInfo.FullName.Should().Be(tempPath);
    }

    [Fact]
    public async Task Method_parameters_of_type_ParseResult_receive_the_current_ParseResult_instance()
    {
        ParseResult boundParseResult = default;

        var option = new CliOption<int>("-x");

        var command = new CliCommand("command")
        {
            option
        };
        command.Action = CommandHandler.Create<ParseResult>(result => { boundParseResult = result; });

        await command.Parse("command -x 123").InvokeAsync();

        boundParseResult.GetValue(option).Should().Be(123);
    }

    [Fact]
    public async Task Method_parameters_of_type_ParseResult_receive_the_current_BindingContext_instance()
    {
        BindingContext boundContext = default;

        var option = new CliOption<int>("-x");
        var command = new CliCommand("command")
        {
            option
        };
        command.Action = CommandHandler.Create<BindingContext>(context => { boundContext = context; });

        await command.Parse("command -x 123").InvokeAsync();

        boundContext.ParseResult.GetValue(option).Should().Be(123);
    }

    private class ExecuteTestClass
    {
        public string boundName = default;
        public int boundAge = default;

        public void Execute(string name, int age)
        {
            boundName = name;
            boundAge = age;
        }
    }

    private delegate void ExecuteTestDelegate(string name, int age);

    [Fact]
    public async Task Method_parameters_on_the_invoked_member_method_are_bound_to_matching_option_names_by_delegate()
    {
        var testClass = new ExecuteTestClass();

        var command = new CliCommand("command")
        {
            new CliOption<string>("--name"),
            new CliOption<int>("--age")
        };
        command.Action = CommandHandler.Create((ExecuteTestDelegate)testClass.Execute);

        await command.Parse("command --age 425 --name Gandalf").InvokeAsync();

        testClass.boundName.Should().Be("Gandalf");
        testClass.boundAge.Should().Be(425);
    }

    [Fact]
    public async Task Method_parameters_on_the_invoked_member_method_are_bound_to_matching_option_names_by_MethodInfo_with_target()
    {
        var testClass = new ExecuteTestClass();

        var command = new CliCommand("command")
        {
            new CliOption<string>("--name"),
            new CliOption<int>("--age")
        };
        command.Action = CommandHandler.Create(
            testClass.GetType().GetMethod(nameof(ExecuteTestClass.Execute)),
            testClass);

        await command.Parse("command --age 425 --name Gandalf").InvokeAsync();

        testClass.boundName.Should().Be("Gandalf");
        testClass.boundAge.Should().Be(425);
    }

    [Fact]
    public async Task Method_parameters_on_the_invoked_method_are_bound_to_matching_argument_names()
    {
        string boundName = default;
        int boundAge = default;

        void Execute(string name, int age)
        {
            boundName = name;
            boundAge = age;
        }

        var command = new CliCommand("command");
        command.Arguments.Add(new CliArgument<int>("age"));
        command.Arguments.Add(new CliArgument<string>("name"));
        command.Action = CommandHandler.Create<string, int>(Execute);

        await command.Parse("command 425 Gandalf").InvokeAsync();

        boundName.Should().Be("Gandalf");
        boundAge.Should().Be(425);
    }

    [Fact]
    public async Task Method_parameters_on_the_invoked_method_can_be_bound_to_hyphenated_argument_names()
    {
        string boundFirstName = default;

        void Execute(string firstName)
        {
            boundFirstName = firstName;
        }

        var command = new CliCommand("command")
        {
            new CliArgument<string>("first-name")
        };
        command.Action = CommandHandler.Create<string>(Execute);

        await command.Parse("command Gandalf").InvokeAsync();

        boundFirstName.Should().Be("Gandalf");
    }

    [Fact]
    public async Task Method_parameters_on_the_invoked_method_can_be_bound_to_argument_names_case_insensitively()
    {
        string boundName = default;
        int boundAge = default;

        void Execute(string name, int AGE)
        {
            boundName = name;
            boundAge = AGE;
        }

        var command = new CliCommand("command");
        command.Arguments.Add(new CliArgument<int>("AGE"));
        command.Arguments.Add(new CliArgument<string>("Name"));
        command.Action = CommandHandler.Create<string, int>(Execute);

        await command.Parse("command 425 Gandalf").InvokeAsync();

        boundName.Should().Be("Gandalf");
        boundAge.Should().Be(425);
    }

    [Fact]
    public async Task Method_parameters_on_the_invoked_method_are_bound_to_matching_argument_names_with_pipe_in()
    {
        string boundName = default;
        int boundAge = default;

        void Execute(string fullnameOrNickname, int age)
        {
            boundName = fullnameOrNickname;
            boundAge = age;
        }

        var command = new CliCommand("command");
        command.Arguments.Add(new CliArgument<int>("age"));
        command.Arguments.Add(new CliArgument<string>("fullname|nickname"));
        command.Action = CommandHandler.Create<string, int>(Execute);

        await command.Parse("command 425 Gandalf").InvokeAsync();

        boundName.Should().Be("Gandalf");
        boundAge.Should().Be(425);
    }

    [Theory]
    [InlineData(typeof(ConcreteTestCommandHandler), 42)]
    [InlineData(typeof(VirtualTestCommandHandler), 42)]
    [InlineData(typeof(OverridenVirtualTestCommandHandler), 41)]
    public async Task Method_invoked_is_matching_to_the_interface_implementation(Type type, int expectedResult)
    {
        var command = new CliCommand("command");
        command.Action = CommandHandler.Create(type.GetMethod(nameof(AsynchronousCliAction.InvokeAsync)));

        int result = await command.Parse("command").InvokeAsync();

        result.Should().Be(expectedResult);
    }

    public abstract class AbstractTestCommandHandler : AsynchronousCliAction
    {
        public abstract Task<int> DoJobAsync();
        
        public override Task<int> InvokeAsync(ParseResult context, CancellationToken cancellationToken)
            => DoJobAsync();
    }

    public sealed class ConcreteTestCommandHandler : AbstractTestCommandHandler
    {
        public override Task<int> DoJobAsync()
            => Task.FromResult(42);
    }

    public class VirtualTestCommandHandler : AsynchronousCliAction
    {
        public override Task<int> InvokeAsync(ParseResult context, CancellationToken cancellationToken)
            => Task.FromResult(42);
    }

    public class OverridenVirtualTestCommandHandler : VirtualTestCommandHandler
    {
        public override Task<int> InvokeAsync(ParseResult context, CancellationToken cancellationToken)
            => Task.FromResult(41);
    }
}