﻿// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.CommandLine.Tests;
using System.Threading.Tasks;
using FluentAssertions;
using Xunit;
using System.Linq;
using System.Collections.Generic;
using System.IO;
using System.CommandLine.Invocation;

namespace System.CommandLine.JackFruit.Tests
{
    public class PreBinderTests
    {
        private readonly TestConsole _console;
        private readonly TestProgram _testProgram;
        private readonly Command[] testParents;

        public PreBinderTests()
        {
            _console = new TestConsole();
            _testProgram = new TestProgram();
            var helpFinder = (HelpFinder)PreBinderContext.Current.HelpFinder;
            helpFinder.AddDescriptionFinder(new DescriptionFinder());
            helpFinder.AddDescriptionFinder(new HybridModelDescriptionFinder());
            testParents = new Command[] { new Command("test") };
        }

        [Fact]
        public void Can_retrieve_alias_from_a_type_name()
        {
            var aliases = PreBinderContext.Current.AliasFinder.Get(testParents, typeof(Tool));
            TestUtils.CheckAliasList(aliases, new string[] { "tool" });
            aliases = PreBinderContext.Current.AliasFinder.Get(testParents, typeof(ToolInstall));
            // Without context, this is the correct answer
            TestUtils.CheckAliasList(aliases, new string[] { "tool-install" });
        }

        [Fact]
        public void Can_retrieve_help_for_command_from_description_file()
        {
            var help = PreBinderContext.Current.HelpFinder.Get(testParents, typeof(Tool));
            TestUtils.CheckHelp(help, "Install or manage tools");
        }

        [Fact]
        public void Can_retrieve_help_for_command_from_hybrid_description_file()
        {
            var help = PreBinderContext.Current.HelpFinder.Get(testParents, typeof(DotnetHybrid.Tool));
            TestUtils.CheckHelp(help, "Install or manage tools");
        }

        [Fact]
        public void Can_retrieve_arguments_for_type()
        {
            var arguments = PreBinderContext.Current.ArgumentFinder.Get(testParents, typeof(ToolInstall));
            TestUtils.CheckArguments(arguments, new List<string>() { "package-id" });
        }

        [Fact]
        public void Doesnt_find_arguments_when_there_arent_any()
        {
            var arguments = PreBinderContext.Current.ArgumentFinder.Get(testParents, typeof(Tool));
            TestUtils.CheckArguments(arguments, new List<string>());
        }

        [Fact]
        public void Can_retrieve_parent_arguments_for_subcommands_for_hybrid()
        {
            // SubCommands 
            var commands = PreBinderContext.Current.SubCommandFinder.Get(testParents, typeof(DotnetHybrid));
            var addCommand = commands.Where(x => x.Name == "add").First();
            TestUtils.CheckSubCommands(addCommand, "package", "reference");
            var packageCommand = addCommand.Children.OfType<Command>().Where(c => c.Name == "package").First();
            TestUtils.CheckArguments(addCommand, new string[] { "project-file" });
        }

        [Fact]
        public void Can_retrieve_options_for_type()
        {
            var options = PreBinderContext.Current.OptionFinder.Get(testParents, typeof(Tool));
            TestUtils.CheckOptions(options, new (string, Type)[] { });
        }

        [Fact]
        public void Can_retrieve_handler_for_type()
        {
            var handler = PreBinderContext.Current.OptionFinder.Get(testParents, typeof(ToolInstall));
            handler.Should().NotBeNull();
        }

        [Fact]
        public void Can_retrieve_subCommands_for_type()
        {
            var commands = PreBinderContext.Current.SubCommandFinder.Get(testParents, typeof(DotnetJackFruit));
            TestUtils.CheckSubCommands(commands, "add", "list", "remove", "sln", "tool");
        }

        [Fact]
        public void Can_retrieve_subCommands_via_methodInfo()
        {
            var commands = PreBinderContext.Current.SubCommandFinder.Get(testParents, typeof(DotnetHybrid.Add));
            TestUtils.CheckSubCommands(commands, "package", "reference");
            var packageCommand = commands.Where(c => c.Name == "package").First();
            TestUtils.CheckAliasList(packageCommand.Aliases, new string[] { "package" });
            TestUtils.CheckHelp(packageCommand.Description, "Add a NuGet package reference");
            TestUtils.CheckSubCommands(packageCommand, new string[] { });
        }

        [Fact]
        public void Can_retrieve_command_structure()
        {
            var rootCommand = PreBinder.RootCommand<DotnetHybrid>(new HybridModelDescriptionFinder());

            TestUtils.CheckSubCommands(rootCommand, "add", "list", "remove", "sln", "tool");
            var addCommand = rootCommand.Children.OfType<Command>().Single(x => x.Name == "add");
            TestUtils.CheckSubCommands(addCommand, "package", "reference");
            TestUtils.CheckArguments(new Argument[] { addCommand.Argument }, new string[] { "project-file" });

            var packageCommand = addCommand.Children.OfType<Command>().Single(x => x.Name == "package");
            TestUtils.CheckArguments(new Argument[] { packageCommand.Argument }, new string[] { "package-name" });
            TestUtils.CheckHelp(packageCommand.Description, "Add a NuGet package reference to the project.");
            TestUtils.CheckSubCommands(packageCommand, new string[] { });
            TestUtils.CheckOptions(packageCommand,
                            ("framework", typeof(string)),
                            ("source", typeof(string)),
                            ("no-restore", typeof(bool)),
                            ("interactive", typeof(bool)),
                            ("package-directory", typeof(DirectoryInfo))
                        );
        }

        [Fact]
        public void Can_create_root_commands()
        {
            var command = PreBinder.RootCommand<DotnetJackFruit>(new DescriptionFinder());
            command.Should().NotBeNull();
        }

        [Fact]
        public void Can_retrieve_invocation()
        {
            var rootCommand = PreBinder.RootCommand<DotnetHybrid>(new HybridModelDescriptionFinder());
            var toolCommand = rootCommand.Children.OfType<Command>().Single(x => x.Name == "tool");
            toolCommand.Should().NotBeNull();
            var toolInstallCommand = toolCommand.Children.OfType<Command>().Single(x => x.Name == "install");
            toolInstallCommand.Should().NotBeNull();
            var invocation = toolInstallCommand.Handler;
            invocation.Should().NotBeNull();

            var task = rootCommand.InvokeAsync("tool install foo --g");
            task.Result.Should().Be(3);
        }
    }
}
