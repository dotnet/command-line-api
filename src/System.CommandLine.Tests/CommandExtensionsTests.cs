// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.CommandLine.Builder;
using System.CommandLine.IO;
using FluentAssertions;
using Xunit;

namespace System.CommandLine.Tests
{
    public class CommandExtensionsTests
    {
        [Fact]
        public void Command_Invoke_can_be_called_more_than_once_for_the_same_command()
        {
            var command = new RootCommand("Root command description")
            {
                new Command("inner")
            };

            var console1 = new TestConsole();

            command.Invoke("-h", console1);

            console1.Out.ToString().Should().Contain(command.Description);
            
            var console2 = new TestConsole();

            command.Invoke("-h", console2);

            console2.Out.ToString().Should().Contain(command.Description);
        }

        [Fact]
        public void When_CommandLineBuilder_is_used_then_Command_Invoke_uses_its_configuration()
        {
            var command = new RootCommand();

            new CommandLineBuilder(command)
                .UseMiddleware(context =>
                {
                    context.Console.Out.Write("hello!");
                })
                .Build();

            var console = new TestConsole();

            command.Invoke("", console);

            console.Out
                   .ToString()
                   .Should()
                   .Contain("hello!");
        }
    }
}