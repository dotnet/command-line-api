// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Generic;
using System.CommandLine.IO;
using System.CommandLine.Parsing;
using System.Threading.Tasks;
using FluentAssertions;
using Xunit;
using System.CommandLine.Tests.Utility;
using Xunit.Abstractions;

namespace System.CommandLine.Tests
{
    public class CommandExtensionsTests
    {
        private ITestOutputHelper _output;

        public CommandExtensionsTests(ITestOutputHelper output)
        {
            _output = output;
        }

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
        public void When_CommandLineBuilder_is_used_then_Command_Invoke_does_not_use_its_configuration()
        {
            var command = new RootCommand();

            new CommandLineBuilder(command)
                .AddMiddleware(context =>
                {
                    context.Console.Out.Write("hello!");
                })
                .Build();

            var console = new TestConsole();

            command.Invoke("", console);

            console.Out
                   .ToString()
                   .Should()
                   .NotContain("hello!");
        }

        [Fact] // https://github.com/dotnet/command-line-api/issues/1589
        public async Task Implicit_parsers_for_Parse_and_Invoke_do_not_affect_one_another()
        {
            RootCommand root = new();

            root.Parse("");

            var console = new TestConsole();
            
            await root.InvokeAsync("-h", console);

            _output.WriteLine(console.Out.ToString());
            
            console.Should().ShowHelp();
        }
    }
}