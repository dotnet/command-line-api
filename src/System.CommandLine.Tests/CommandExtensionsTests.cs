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

        [Fact]
        public void Invoke_extension_method_reuses_implicit_parser_instance()
        {
            List<Parser> parsers = new();
            var command = new Command("x");

            command.SetHandler(context => parsers.Add(context.ParseResult.Parser));

            command.Invoke("");
            command.Invoke("");

            var parser1 = parsers[0];
            var parser2 = parsers[1];

            parser1.Should().BeSameAs(parser2);
        }

        [Fact]
        public void Parse_and_Invoke_extension_methods_use_different_implicit_parsers()
        {
            var command = new Command("x");

            Parser implicitParserForInvoking = null;
            Parser implicitParserForParsing = null;

            command.SetHandler(context => implicitParserForInvoking = context.ParseResult.Parser);

            command.Invoke("");

            implicitParserForParsing = command.Parse("").Parser;

            implicitParserForInvoking.Should().NotBeSameAs(implicitParserForParsing);
        }
    }
}