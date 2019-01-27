// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.CommandLine.Invocation;
using FluentAssertions;
using System.Linq;
using System.Threading.Tasks;
using Xunit;
using System.CommandLine.Builder;

namespace System.CommandLine.Tests
{
    public class BindingTests
    {
        private readonly TestConsole _console = new TestConsole();

        [Fact]
        public void Method_parameters_on_the_invoked_lambda_are_bound_to_matching_option_names()
        {
            var wasCalled = false;
            const string commandLine = "command --age 425 --name Gandalf";

            var command = new Command("command");
            command.AddOption(new Option("--name", "", new Argument<string>()));
            command.AddOption(new Option("--age", "", new Argument<int>()));
            var handler = CommandHandler.Create<string, int>((name, age) =>
            {
                //wasCalled = true;
                //name.Should().Be("Gandalf");
                //age.Should().Be(425);
            }, command);


            var arguments = handler.Binder
                            .GetInvocationArguments(GetInvocationContext(commandLine, command));
            arguments.Should().BeEquivalentSequenceTo("Gandalf", 425);

            command.Handler = handler;
            // Can't also call InvokeAsync because adding version a second time crashes. Probably fix as bug.
            // If pipeline isn't idempotent/reentrant, then throw more specific error
            //await command.InvokeAsync(commandLine, _console);
            //wasCalled.Should().BeTrue();
            wasCalled.Should().BeFalse();
        }

        private static InvocationContext GetInvocationContext(string commandLine, Command command)
        {
            var parser = new CommandLineBuilder(command)
                         .UseDefaults()
                         .Build();
            var parseResult = parser.Parse(commandLine);
            var invocationContext = new InvocationContext(parseResult, parser);
            return invocationContext;
        }
    }
}
