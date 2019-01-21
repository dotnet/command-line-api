// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

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
        private readonly Command testParent;

        public PreBinderTests()
        {
            _console = new TestConsole();
            _testProgram = new TestProgram();
            var helpFinder = PreBinderContext.Current.DescriptionStrategies;
            helpFinder.AddStrategy<object>((c, s) => DescriptionFinder.Description(s));
            helpFinder.AddStrategy<object>((c, s) => HybridModelDescriptionFinder.Description(s));
            testParent = new Command("test");
        }

        [Fact]
        public void Can_retrieve_alias_from_a_type_name()
        {
            var aliases = PreBinderContext.Current.AliasStrategies.Get(testParent, typeof(Tool));
            TestUtils.CheckAliasList(aliases, new string[] { "tool" });
            aliases = PreBinderContext.Current.AliasStrategies.Get(testParent, typeof(ToolInstall));
            // Without context, this is the correct answer
            TestUtils.CheckAliasList(aliases, new string[] { "tool-install" });
        }

        [Fact]
        public void Can_retrieve_help_for_command_from_description_file()
        {
            var help = PreBinderContext.Current.DescriptionStrategies.Get(testParent, typeof(Tool));
            TestUtils.CheckHelp(help, "Install or manage tools");
        }

        [Fact]
        public void Can_retrieve_help_for_command_from_hybrid_description_file()
        {
            var help = PreBinderContext.Current.DescriptionStrategies.Get(testParent, typeof(DotnetHybrid.Tool));
            TestUtils.CheckHelp(help, "Install or manage tools");
        }

        [Fact]
        public void Can_retrieve_arguments_for_type()
        {
            var argumentBindings = PreBinderContext.Current.ArgumentBindingStrategies.Get(testParent, typeof(ToolInstall));
            TestUtils.CheckArgumentBindings(argumentBindings, new List<string>() { "package-id" });
        }

        [Fact]
        public void Doesnt_find_arguments_when_there_arent_any()
        {
            var argumentBindings = PreBinderContext.Current.ArgumentBindingStrategies.Get(testParent, typeof(Tool));
            TestUtils.CheckArgumentBindings(argumentBindings, new List<string>());
        }

        [Fact]
        public void Can_retrieve_parent_arguments_for_subcommands_for_hybrid()
        {
            // SubCommands 
            var commands = PreBinderContext.Current.SubCommandStrategies.Get(testParent, typeof(DotnetHybrid));
            var addCommand = commands.Where(x => x.Name == "add").First();
            TestUtils.CheckSubCommands(addCommand, "package", "reference");
            var packageCommand = addCommand.Children.OfType<Command>().Where(c => c.Name == "package").First();
            TestUtils.CheckArguments(addCommand, new string[] { "project-file" });
        }

        [Fact]
        public void Can_retrieve_options_for_type()
        {
            var sourceAndOptions = PreBinderContext.Current.OptionBindingStrategies.Get(testParent, typeof(Tool));
            TestUtils.CheckOptions(sourceAndOptions.Select(x => x.Option), new (string, Type)[] { });
        }

        [Fact]
        public void Can_retrieve_handler_for_type()
        {
            var handler = PreBinderContext.Current.OptionBindingStrategies.Get(testParent, typeof(ToolInstall));
            handler.Should().NotBeNull();
        }

        [Fact]
        public void Can_retrieve_subCommands_for_type()
        {
            var commands = PreBinderContext.Current.SubCommandStrategies.Get(testParent, typeof(DotnetJackFruit));
            TestUtils.CheckSubCommands(commands, "add", "list", "remove", "sln", "tool");
        }

        [Fact]
        public void Can_retrieve_subCommands_via_methodInfo()
        {
            var commands = PreBinderContext.Current.SubCommandStrategies.Get(testParent, typeof(DotnetHybrid.Add));
            TestUtils.CheckSubCommands(commands, "package", "reference");
            var packageCommand = commands.Where(c => c.Name == "package").First();
            TestUtils.CheckAliasList(packageCommand.Aliases, new string[] { "package" });
            TestUtils.CheckHelp(packageCommand.Description, "Add a NuGet package reference");
            TestUtils.CheckSubCommands(packageCommand, new string[] { });
        }

        [Fact]
        public void Can_retrieve_command_structure()
        {
            var rootCommand = PreBinder.RootCommand<DotnetHybrid>((c, s) => HybridModelDescriptionFinder.Description(s));

            TestUtils.CheckSubCommands(rootCommand, "add", "list", "remove", "sln", "tool");
            var addCommand = rootCommand.Children.OfType<Command>().Single(x => x.Name == "add");
            TestUtils.CheckSubCommands(addCommand, "package", "reference");
            TestUtils.CheckArgument(addCommand.Argument, "project-file");

            var packageCommand = addCommand.Children.OfType<Command>().Single(x => x.Name == "package");
            TestUtils.CheckArgument(packageCommand.Argument, "package-name");
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
            var command = PreBinder.RootCommand<DotnetJackFruit>((c, s) => HybridModelDescriptionFinder.Description(s));
            command.Should().NotBeNull();
        }

        [Fact]
        public void Can_retrieve_invocation()
        {
            var rootCommand = PreBinder.RootCommand<DotnetHybrid>((c, s) => HybridModelDescriptionFinder.Description(s));
            var toolCommand = rootCommand.Children.OfType<Command>().Single(x => x.Name == "tool");
            toolCommand.Should().NotBeNull();
            var toolInstallCommand = toolCommand.Children.OfType<Command>().Single(x => x.Name == "install");
            toolInstallCommand.Should().NotBeNull();
            var invocation = toolInstallCommand.Handler;
            invocation.Should().NotBeNull();

            var task = rootCommand.InvokeAsync("tool install foo -g");
            task.Result.Should().Be(3);
        }

        [Fact]
        public void Can_get_report_on_command_structure()
        {
            var rootCommand = PreBinder.RootCommand<DotnetHybrid>((c, s) => HybridModelDescriptionFinder.Description(s));
            var report = Reporter.ReportCommand(rootCommand);
            report.Should().NotBeNull();
        }
    }
}
