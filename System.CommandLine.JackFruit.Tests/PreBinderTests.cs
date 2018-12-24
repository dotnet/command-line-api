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
    public class PreBinderTests
    {
        private readonly TestConsole _console;
        private readonly TestProgram _testProgram;

        public PreBinderTests()
        {
            _console = new TestConsole();
            _testProgram = new TestProgram();
            PreBinderContext.Current.HelpFinder.AddApproach(
                HelpFinder.DescriptionFinderApproach(new DescriptionFinder()));
            PreBinderContext.Current.SubCommandFinder.AddApproach(
                CommandFinder.DerivedTypeApproach(typeof(DotnetJackFruit)));
        }


        [Fact]
        public void Can_retrieve_alias_from_a_type_name()
        {
            var aliases = PreBinderContext.Current.AliasFinder.Get(typeof(Tool));
            CheckAliasList(aliases, new string[] { "tool" });
            aliases = PreBinderContext.Current.AliasFinder.Get(typeof(ToolInstall));
            // Without context, this is the correct answer
            CheckAliasList(aliases, new string[] { "tool-install" });
        }

        [Fact]
        public void Can_retrieve_help_for_command_from_description_file()
        {
            var help = PreBinderContext.Current.HelpFinder.Get(typeof(Tool));
            CheckHelp(help, "Install or manage tools");
        }

        [Fact]
        public void Can_retrieve_arguments_for_type()
        {
            var arguments = PreBinderContext.Current.ArgumentFinder.Get(typeof(Tool));
            CheckArguments(arguments, new List<string>());
        }

        [Fact]
        public void Can_retrieve_options_for_type()
        {
            var options = PreBinderContext.Current.OptionFinder.Get(typeof(Tool));
            CheckOptions(options, new (string, Type)[] { });
        }

        [Fact]
        public void Can_retrieve_handler_for_type()
        {
            var handler = PreBinderContext.Current.OptionFinder.Get(typeof(ToolInstall));
            handler.Should().NotBeNull();
        }

        [Fact]
        public void Can_retrieve_subCommands_for_type()
        {
            var commands = PreBinderContext.Current.SubCommandFinder.Get(typeof(DotnetJackFruit));
            CheckSubCommands(commands, "add", "list", "remove", "sln", "tool");
        }

        [Fact]
        public void Can_retrieve_subcCmmands_via_methodInfo()
        {
            var commands = PreBinderContext.Current.SubCommandFinder.Get(typeof(DotnetHybrid.Add));
            CheckSubCommands(commands, "package", "reference");
            var packageCommand = commands.Where(c => c.Name == "package").First();
            CheckAliasList(packageCommand.Aliases, new string[] { "package" });
 //           CheckArguments(new Argument[] { packageCommand.Argument }, new string[] { "project-file" });
            CheckHelp(packageCommand.Description, "");
            CheckSubCommands(packageCommand, new string[] { });
            CheckOptions(packageCommand, 
                            ("package-name", typeof(string)),
                            ("framework", typeof(string)),
                            ("source", typeof(string)),
                            ("no-restore", typeof(string)),
                            ("Interactive", typeof(bool)),
                            ("PackageDirectory", typeof(DirectoryInfo))
                        );
        }

        [Fact]
        public void Can_create_root_commands_for_()
        {
            var command = CommandPreBinder.ParserTreeFromDerivedTypes<DotnetJackFruit>(new DescriptionFinder());
            command.Should().NotBeNull();
        }

        private static void CheckAliasList(IEnumerable<string> actual, IEnumerable<string> expected)
        {
            actual.Should().NotBeNull();
            expected.Count().Should().Be(actual.Count());
            foreach (var s in expected)
            {
                actual.Should().Contain(s);
            }
        }

        private static void CheckArguments(IEnumerable<Argument> actual, IEnumerable<string> expected)
        {
            actual.Should().NotBeNull();
            expected.Count().Should().Be(actual.Count());
            foreach (var s in expected)
            {
                actual
                   .Any(x => x.Name == s)
                   .Should().BeTrue();
            }
        }

        private static void CheckHelp(string actual, string expectedStart)
        {
            if  (string.IsNullOrWhiteSpace(expectedStart ))
            {
                actual.Should().BeNullOrWhiteSpace();
                return;
            }
            actual.Should().NotBeNullOrWhiteSpace();
            if (expectedStart.Length < 15)
            {
                actual.Should().Be(expectedStart);
                return;
            }
            actual.StartsWith(expectedStart).Should().BeTrue();
        }

        private static void CheckSubCommands(Command command, params string[] subCommandNames)
        {

            var childCommands = command.Children.OfType<Command>();
            CheckSubCommands(childCommands);
        }

        private static void CheckSubCommands(IEnumerable<Command> childCommands, params string[] subCommandNames)
        {
            childCommands.Should().NotBeNull();
            childCommands.Count().Should().Be(subCommandNames.Length);
            foreach (var cmdName in subCommandNames)
            {
                childCommands.Any(x => x.Name == cmdName).Should().BeTrue();
            }
        }

        private static void CheckOptions(Command command, params (string Name, Type Type)[] optionInfos)
        {
            var childOptions = command.Children.OfType<Option>();
            CheckOptions(childOptions, optionInfos);
        }

        private static void CheckOptions(IEnumerable<Option> options, params (string Name, Type Type)[] optionInfos)
        {
            options.Should().NotBeNull();
            options.Count().Should().Be(optionInfos.Length);
            foreach (var opt in optionInfos)
            {
                var option = options.Where(x => x.Name == opt.Name).SingleOrDefault();
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
