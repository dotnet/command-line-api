// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.CommandLine.Builder;
using System.CommandLine.Invocation;
using System.Reflection;
using System.Threading.Tasks;
using FluentAssertions;
using Xunit;
using static System.Environment;

namespace System.CommandLine.Tests
{
    public class VersionOptionTests
    {
        private static readonly string version = (Assembly.GetEntryAssembly() ?? Assembly.GetExecutingAssembly())
                                                         .GetCustomAttribute<AssemblyInformationalVersionAttribute>()
                                                         .InformationalVersion;

        [Fact]
        public async Task When_the_version_option_is_specified_then_the_version_is_written_to_standard_out()
        {
            var parser = new CommandLineBuilder()
                         .AddVersionOption()
                         .Build();

            var console = new TestConsole();

            await parser.InvokeAsync("--version", console);

            console.Out.ToString().Should().Be($"{version}{NewLine}");
        }

        [Fact]
        public async Task When_the_version_option_is_specified_then_invocation_is_short_circuited()
        {
            var wasCalled = false;
            var rootCommand = new RootCommand();
            rootCommand.Handler = CommandHandler.Create(() => wasCalled = true);

            var parser = new CommandLineBuilder(rootCommand)
                         .AddVersionOption()
                         .Build();

            var console = new TestConsole();

            await parser.InvokeAsync("--version", console);

            wasCalled.Should().BeFalse();
        }

        [Fact]
        public async Task Version_option_appears_in_help()
        {
            var parser = new CommandLineBuilder()
                         .UseHelp()
                         .AddVersionOption()
                         .Build();

            var console = new TestConsole();

            await parser.InvokeAsync("--help", console);

            console.Out
                   .ToString()
                   .Should()
                   .Match("*Options:*--version*Display version information*");
        }
    }
}
