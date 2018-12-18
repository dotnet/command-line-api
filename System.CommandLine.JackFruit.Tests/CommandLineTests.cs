// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.CommandLine.Tests;
using System.Threading.Tasks;
using FluentAssertions;
using Xunit;
using System.Linq;
using System.Collections.Generic;
using System.IO;

namespace System.CommandLine.JackFruit.Tests
{
    public class CommandLineTests
    {
        private readonly TestConsole _console;
        private readonly TestProgram _testProgram;

        public CommandLineTests()
        {
            _console = new TestConsole();
            _testProgram = new TestProgram();
        }

        [Fact]
        public void Command_for_class_hiearchy_can_be_created_with_top_level_commands()
        {
            var command = HierarchicalTypeCommandBinder<DotnetJackFruit>.GetCommandLine();
            command.Should().NotBeNull();
            CheckSubCommands(command, "add", "list", "remove", "sln", "tool");
        }

        [Fact]
        public void Command_for_class_hiearchy_can_be_created_with_sub_commands()
        {
            var command = HierarchicalTypeCommandBinder<DotnetJackFruit>.GetCommandLine();
            command.Should().NotBeNull();
            var toolCommand = (Command)command.Children["tool"];
            CheckSubCommands(toolCommand, "install", "list", "update", "uninstall");
        }

        [Fact]
        public void Command_for_class_hiearchy_can_be_created_with_sub_sub_commands()
        {
            var command = HierarchicalTypeCommandBinder<DotnetJackFruit>.GetCommandLine();
            command.Should().NotBeNull();
            var toolCommand = (Command)command.Children["tool"];
            var tool2Command = (Command)toolCommand.Children["uninstall"];
            CheckSubCommands(tool2Command, "test", "test2");
        }

        [Fact]
        public void Options_created_for_command()
        {
            var command = HierarchicalTypeCommandBinder<DotnetJackFruit>.GetCommandLine();
            command.Should().NotBeNull();
            var toolCommand = (Command)command.Children["tool"];
            var toolInstallCommand = (Command)toolCommand.Children["install"];
            CheckOptions(toolInstallCommand,
                    ("global", typeof(bool)),
                    ("version", typeof(string)),
                    ("tool-path", typeof(DirectoryInfo)),
                    ("config-file", typeof(FileInfo)),
                    ("add-source", typeof(string)),
                    ("framework", typeof(string)),
                    ("verbosity", typeof(StandardVerbosity)));
        }


        [Fact]
        public void Options_not_created_if_marked_for_skip()
        {
            var command = HierarchicalTypeCommandBinder<DotnetJackFruit>.GetCommandLine();
            command.Should().NotBeNull();
            var toolCommand = (Command)command.Children["tool"];
            var toolInstallCommand = (Command)toolCommand.Children["update"];
            CheckOptions(toolInstallCommand,
                    ("global", typeof(bool)),
                    ("tool-path", typeof(DirectoryInfo)),
                    ("config-file", typeof(FileInfo)),
                    ("add-source", typeof(string)),
                    ("framework", typeof(string)),
                    ("verbosity", typeof(StandardVerbosity)));
        }

        private static void CheckSubCommands(Command command, params string[] subCommandNames)
        {
            var childCommands = command.Children.OfType<Command>();
            childCommands.Count().Should().Be(subCommandNames.Length);
            foreach (var cmdName in subCommandNames)
            {
                command.Children.Contains(cmdName).Should().BeTrue();
            }
        }

        private static void CheckOptions(Command command, params (string Name, Type Type)[] optionInfos)
        {
            var childOptions = command.Children.OfType<Option>();
            childOptions.Count().Should().Be(optionInfos.Length);
            foreach (var opt in optionInfos)
            {
                var option = command.Children[opt.Name];
                option.Should().NotBeNull();
                option.Argument.ArgumentType.Should().Be(opt.Type);
            }
        }

        //[Fact]
        //public async Task It_shows_help_text_based_on_XML_documentation_comments()
        //{
        //    int exitCode = await CommandLine.InvokeMethodAsync(
        //                       new[] { "--help" },
        //                       _console,
        //                       TestProgram.TestMainMethodInfo,
        //                       _testProgram);

        //    exitCode.Should().Be(0);

        //    var stdOut = _console.Out.ToString();

        //    stdOut.Should()
        //          .Contain("--name       Specifies the name option")
        //          .And.Contain("Options:");
        //    stdOut.Should()
        //          .Contain("Help for the test program");
        //}

        //[Fact]
        //public async Task It_executes_method_with_string_option_with_default()
        //{
        //    int exitCode = await CommandLine.InvokeMethodAsync(
        //                       Array.Empty<string>(),
        //                       _console,
        //                       TestProgram.TestMainMethodInfoWithDefault,
        //                       _testProgram);

        //    exitCode.Should().Be(0);
        //    _testProgram.Captured.Should().Be("Bruce");
        //}

        //private void TestMainThatThrows() => throw new InvalidOperationException("This threw an error");

        //[Fact]
        //public async Task It_shows_error_without_invoking_method()
        //{
        //    Action action = TestMainThatThrows;

        //    int exitCode = await CommandLine.InvokeMethodAsync(
        //                       new[] { "--unknown" },
        //                       _console,
        //                       action.Method,
        //                       this);

        //    exitCode.Should().Be(1);
        //    _console.Error.ToString()
        //            .Should().NotBeEmpty()
        //            .And
        //            .Contain("--unknown");
        //    _console.ForegroundColor.Should().Be(ConsoleColor.Red);
        //}

        //[Fact]
        //public async Task It_handles_uncaught_exceptions()
        //{
        //    Action action = TestMainThatThrows;

        //    int exitCode = await CommandLine.InvokeMethodAsync(
        //                       Array.Empty<string>(),
        //                       _console,
        //                       action.Method,
        //                       this);

        //    exitCode.Should().Be(1);
        //    _console.Error.ToString()
        //            .Should().NotBeEmpty()
        //            .And
        //            .Contain("This threw an error");
        //    _console.ForegroundColor.Should().Be(ConsoleColor.Red);
        //}
    }
}
