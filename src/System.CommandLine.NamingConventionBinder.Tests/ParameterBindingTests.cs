// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.CommandLine;
using System.CommandLine.Binding;
using System.CommandLine.Invocation;
using System.CommandLine.IO;
using System.CommandLine.Parsing;
using System.IO;
using System.Threading.Tasks;
using FluentAssertions;
using Xunit;

namespace System.CommandLine.NamingConventionBinder.Tests;

public class ParameterBindingTests
{
    private readonly TestConsole _console = new();

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

        var command = new Command("command");
        command.AddOption(new Option<string>("--name"));
        command.AddOption(new Option<int>("--age"));
        command.Handler = CommandHandler.Create<string, int>(Execute);

        await command.InvokeAsync("command --age 425 --name Gandalf", _console);

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

        var command = new Command("command")
        {
            new Option<string>("--first-name")
        };
        command.Handler = CommandHandler.Create<string>(Execute);

        await command.InvokeAsync("command --first-name Gandalf", _console);

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

        var command = new Command("command");
        command.AddOption(new Option<string>("--NAME"));
        command.AddOption(new Option<int>("--age"));
        command.Handler = CommandHandler.Create<string, int>(Execute);

        await command.InvokeAsync("command --age 425 --NAME Gandalf", _console);

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

        var command = new Command("command")
        {
            new Option<string>("--name"),
            new Option<int>("--age")
        };
        command.Handler = CommandHandler.Create<string, int>(Execute);

        await command.InvokeAsync("command", _console);

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

        var command = new Command("command")
        {
            new Option<string>(new[] { "-n", "--NAME" }),
            new Option<int>(new[] { "-a", "--age" })
        };
        command.Handler = CommandHandler.Create<string, int>(Execute);

        await command.InvokeAsync("command -a 425 -n Gandalf", _console);

        boundName.Should().Be("Gandalf");
        boundAge.Should().Be(425);
    }

    [Fact]
    public async Task Method_parameters_on_the_invoked_lambda_are_bound_to_matching_option_names()
    {
        string boundName = default;
        int boundAge = default;

        var command = new Command("command")
        {
            new Option<string>("--name"),
            new Option<int>("--age")
        };
        command.Handler = CommandHandler.Create<string, int>((name, age) =>
        {
            boundName = name;
            boundAge = age;
        });

        await command.InvokeAsync("command --age 425 --name Gandalf", _console);

        boundName.Should().Be("Gandalf");
        boundAge.Should().Be(425);
    }

    [Fact]
    public async Task Nullable_parameters_are_bound_to_correct_value_when_option_is_specified()
    {
        int? boundAge = default;

        var command = new Command("command")
        {
            new Option<int?>("--age")
        };
        command.Handler = CommandHandler.Create<int?>(age =>
        {
            boundAge = age;
        });

        await command.InvokeAsync("command --age 425", _console);

        boundAge.Should().Be(425);
    }

    [Fact]
    public async Task Nullable_parameters_are_bound_to_null_when_option_is_not_specified()
    {
        var wasCalled = false;
        int? boundAge = default;

        var command = new Command("command")
        {
            new Option<int?>("--age")
        };
        command.Handler = CommandHandler.Create<int?>(age =>
        {
            wasCalled = true;
            boundAge = age;
        });

        await command.InvokeAsync("command", _console);

        wasCalled.Should().BeTrue();
        boundAge.Should().BeNull();
    }

    [Fact]
    public async Task Method_parameters_of_types_having_constructors_accepting_a_single_string_are_bound_using_handler_parameter_name()
    {
        DirectoryInfo boundDirectoryInfo = default;
        var tempPath = Path.GetTempPath();

        var command = new Command("command")
        {
            new Option<DirectoryInfo>("--dir")
        };
        command.Handler = CommandHandler.Create<DirectoryInfo>(dir =>
        {
            boundDirectoryInfo = dir;
        });

        await command.InvokeAsync($"command --dir \"{tempPath}\"", _console);

        boundDirectoryInfo.FullName.Should().Be(tempPath);
    }

    [Fact]
    public async Task Method_parameters_of_type_ParseResult_receive_the_current_ParseResult_instance()
    {
        ParseResult boundParseResult = default;

        var option = new Option<int>("-x");

        var command = new Command("command")
        {
            option
        };
        command.Handler = CommandHandler.Create<ParseResult>(result => { boundParseResult = result; });

        await command.InvokeAsync("command -x 123", _console);

        boundParseResult.GetValueForOption(option).Should().Be(123);
    }

    [Fact]
    public async Task Method_parameters_of_type_ParseResult_receive_the_current_BindingContext_instance()
    {
        BindingContext boundContext = default;

        var option = new Option<int>("-x");
        var command = new Command("command")
        {
            option
        };
        command.Handler = CommandHandler.Create<BindingContext>(context => { boundContext = context; });

        await command.InvokeAsync("command -x 123", _console);

        boundContext.ParseResult.GetValueForOption(option).Should().Be(123);
    }

    [Fact]
    public async Task Method_parameters_of_type_IConsole_receive_the_current_console_instance()
    {
        var command = new Command("command")
        {
            new Option<int>("-x")
        };
        command.Handler = CommandHandler.Create<IConsole>(console => { console.Out.Write("Hello!"); });

        await command.InvokeAsync("command", _console);

        _console.Out.ToString().Should().Be("Hello!");
    }

    [Fact]
    public async Task Method_parameters_of_type_InvocationContext_receive_the_current_InvocationContext_instance()
    {
        InvocationContext boundContext = default;

        var option = new Option<int>("-x");

        var command = new Command("command")
        {
            option
        };
        command.Handler = CommandHandler.Create<InvocationContext>(context => { boundContext = context; });

        await command.InvokeAsync("command -x 123", _console);

        boundContext.ParseResult.GetValueForOption(option).Should().Be(123);
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

        var command = new Command("command")
        {
            new Option<string>("--name"),
            new Option<int>("--age")
        };
        command.Handler = CommandHandler.Create((ExecuteTestDelegate)testClass.Execute);

        await command.InvokeAsync("command --age 425 --name Gandalf", _console);

        testClass.boundName.Should().Be("Gandalf");
        testClass.boundAge.Should().Be(425);
    }

    [Fact]
    public async Task Method_parameters_on_the_invoked_member_method_are_bound_to_matching_option_names_by_MethodInfo_with_target()
    {
        var testClass = new ExecuteTestClass();

        var command = new Command("command")
        {
            new Option<string>("--name"),
            new Option<int>("--age")
        };
        command.Handler = CommandHandler.Create(
            testClass.GetType().GetMethod(nameof(ExecuteTestClass.Execute)),
            testClass);

        await command.InvokeAsync("command --age 425 --name Gandalf", _console);

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

        var command = new Command("command");
        command.AddArgument(new Argument<int>("age"));
        command.AddArgument(new Argument<string>("name"));
        command.Handler = CommandHandler.Create<string, int>(Execute);

        await command.InvokeAsync("command 425 Gandalf", _console);

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

        var command = new Command("command")
        {
            new Argument<string>("first-name")
        };
        command.Handler = CommandHandler.Create<string>(Execute);

        await command.InvokeAsync("command Gandalf", _console);

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

        var command = new Command("command");
        command.AddArgument(new Argument<int>("AGE"));
        command.AddArgument(new Argument<string>("Name"));
        command.Handler = CommandHandler.Create<string, int>(Execute);

        await command.InvokeAsync("command 425 Gandalf", _console);

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

        var command = new Command("command");
        command.AddArgument(new Argument<int>("age"));
        command.AddArgument(new Argument<string>("fullname|nickname"));
        command.Handler = CommandHandler.Create<string, int>(Execute);

        await command.InvokeAsync("command 425 Gandalf", _console);

        boundName.Should().Be("Gandalf");
        boundAge.Should().Be(425);
    }

    [Theory]
    [InlineData(typeof(ConcreteTestCommandHandler), 42)]
    [InlineData(typeof(VirtualTestCommandHandler), 42)]
    [InlineData(typeof(OverridenVirtualTestCommandHandler), 41)]
    public async Task Method_invoked_is_matching_to_the_interface_implementation(Type type, int expectedResult)
    {
        var command = new Command("command");
        command.Handler = CommandHandler.Create(type.GetMethod(nameof(ICommandHandler.InvokeAsync)));

        var parser = new Parser(command);

        int result = await parser.InvokeAsync("command", _console);

        result.Should().Be(expectedResult);
    }

    public abstract class AbstractTestCommandHandler : ICommandHandler
    {
        public abstract Task<int> DoJobAsync();

        public int Invoke(InvocationContext context) => InvokeAsync(context).GetAwaiter().GetResult();

        public Task<int> InvokeAsync(InvocationContext context)
            => DoJobAsync();
    }

    public sealed class ConcreteTestCommandHandler : AbstractTestCommandHandler
    {
        public override Task<int> DoJobAsync()
            => Task.FromResult(42);
    }

    public class VirtualTestCommandHandler : ICommandHandler
    {
        public int Invoke(InvocationContext context) => 42;

        public virtual Task<int> InvokeAsync(InvocationContext context)
            => Task.FromResult(42);
    }

    public class OverridenVirtualTestCommandHandler : VirtualTestCommandHandler
    {
        public override Task<int> InvokeAsync(InvocationContext context)
            => Task.FromResult(41);
    }
}