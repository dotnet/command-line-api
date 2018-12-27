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
        private readonly Command testParent;

        public PreBinderTests()
        {
            _console = new TestConsole();
            _testProgram = new TestProgram();
            PreBinderContext.Current.HelpFinder.AddApproach(
                HelpFinder.DescriptionFinderApproach(new DescriptionFinder()));
            PreBinderContext.Current.SubCommandFinder.AddApproach(
                CommandFinder.DerivedTypeApproach(typeof(DotnetJackFruit)));
            testParent = new Command("test");
        }

        [Fact]
        public void Can_retrieve_alias_from_a_type_name()
        {
            var aliases = PreBinderContext.Current.AliasFinder.Get(testParent, typeof(Tool));
            CheckAliasList(aliases, new string[] { "tool" });
            aliases = PreBinderContext.Current.AliasFinder.Get(testParent, typeof(ToolInstall));
            // Without context, this is the correct answer
            CheckAliasList(aliases, new string[] { "tool-install" });
        }

        [Fact]
        public void Can_retrieve_help_for_command_from_description_file()
        {
            var help = PreBinderContext.Current.HelpFinder.Get(testParent, typeof(Tool));
            CheckHelp(help, "Install or manage tools");
        }

        [Fact]
        public void Can_retrieve_arguments_for_type()
        {
            var arguments = PreBinderContext.Current.ArgumentFinder.Get(testParent, typeof(ToolInstall ));
            CheckArguments(arguments, new List<string>() { "package-id" });
        }

        [Fact]
        public void Doesnt_find_arguments_when_there_arent_any()
        {
            var arguments = PreBinderContext.Current.ArgumentFinder.Get(testParent, typeof(Tool));
            CheckArguments(arguments, new List<string>());
        }

        [Fact]
        public void Can_retrieve_parent_arguments_for_subcommands_for_hybrid()
        {
            // SubCommands 
            var commands = PreBinderContext.Current.SubCommandFinder.Get(testParent, typeof(DotnetHybrid));
            var addCommand = commands.Where(x => x.Name == "add").First();
            CheckSubCommands(addCommand, "package", "reference");
            var packageCommand = addCommand.Children.OfType<Command>().Where(c => c.Name == "package").First();
            CheckArguments(addCommand, new string[] { "project-file" });
        }

        [Fact]
        public void Can_retrieve_options_for_type()
        {
            var options = PreBinderContext.Current.OptionFinder.Get(testParent, typeof(Tool));
            CheckOptions(options, new (string, Type)[] { });
        }

        [Fact]
        public void Can_retrieve_handler_for_type()
        {
            var handler = PreBinderContext.Current.OptionFinder.Get(testParent, typeof(ToolInstall));
            handler.Should().NotBeNull();
        }

        [Fact]
        public void Can_retrieve_subCommands_for_type()
        {
            var commands = PreBinderContext.Current.SubCommandFinder.Get(testParent, typeof(DotnetJackFruit));
            CheckSubCommands(commands, "add", "list", "remove", "sln", "tool");
        }

        [Fact]
        public void Can_retrieve_subCommands_via_methodInfo()
        {
            var commands = PreBinderContext.Current.SubCommandFinder.Get(testParent, typeof(DotnetHybrid.Add));
            CheckSubCommands(commands, "package", "reference");
            var packageCommand = commands.Where(c => c.Name == "package").First();
            CheckAliasList(packageCommand.Aliases, new string[] { "package" });
            CheckHelp(packageCommand.Description, "");
            CheckSubCommands(packageCommand, new string[] { });
        }


        [Fact(Skip = "Ignoring because not sure how this should work")]
        public void Can_retrieve_command_structure()
        {
            var commands = PreBinderContext.Current.SubCommandFinder.Get(testParent, typeof(DotnetHybrid.Add));
            CheckSubCommands(commands, "package", "reference");
            var packageCommand = commands.Where(c => c.Name == "package").First();
            CheckAliasList(packageCommand.Aliases, new string[] { "package" });
            // TODO: START HERE Get argument working, then have option check names against argument
            CheckArguments(new Argument[] { packageCommand.Argument }, new string[] { "project-file" });
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
        public void Can_create_root_commands()
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

        private static void CheckArguments(Command actual, IEnumerable<string> expected)
        {
            CheckArguments(new List<Argument>() { actual.Argument }, expected);
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
            CheckSubCommands(childCommands, subCommandNames);
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
    }
}
