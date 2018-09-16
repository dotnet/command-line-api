// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.CommandLine.Builder;
using System.CommandLine.Invocation;
using System.ComponentModel;
using System.Diagnostics;
using System.Threading.Tasks;
using FluentAssertions;
using Xunit;
using static System.Environment;

namespace System.CommandLine.Tests
{
    public class RegisterDirectiveTests
    {
        [Fact]
        public async Task Register_directive_complete_successfully()
        {
            bool processRegistered = false;
            Func<ProcessStartInfo, int> testProcess = (_) => { processRegistered = true; return 0; };
            var parser = new CommandLineBuilder()
                         .AddCommand("eat", "")
                         .RegisterDirective(testProcess)
                         .Build();

            var result = parser.Parse("[register]");
            
            var console = new TestConsole();

            var returnCode = await parser.InvokeAsync(result, console);

            processRegistered.Should().BeTrue();
            returnCode.Should().Be(0);
        }

        [Fact]
        public void Register_directive_show_error_when_dotnet_suggest_does_not_exist()
        {
            Func<ProcessStartInfo, int> testProcess = (_) => throw new Win32Exception("fail");
            var parser = new CommandLineBuilder()
                         .AddCommand("eat", "")
                         .RegisterDirective(testProcess)
                         .Build();

            var result = parser.Parse("[register]");
            
            var console = new TestConsole();

            int returnCode = 0;
            Action a = () => { returnCode = parser.InvokeAsync(result, console).Result; };

            a.Should().NotThrow();
            console.Error.ToString().Should().Contain("dotnet-suggest");
            returnCode.Should().Be(1);
        }

        [Fact]
        public void Register_directive_show_error_when_dotnet_suggest_return_non_zero()
        {
            Func<ProcessStartInfo, int> testProcess = (_) => 1;
            var parser = new CommandLineBuilder()
                         .AddCommand("eat", "")
                         .RegisterDirective(testProcess)
                         .Build();

            var result = parser.Parse("[register]");
            
            var console = new TestConsole();

            int returnCode = 0;
            Action a = () => { returnCode = parser.InvokeAsync(result, console).Result; };

            a.Should().NotThrow();
            console.Error.ToString().Should().Contain("Failed to register with dotnet-suggest. Return code 1.");
            returnCode.Should().Be(1);
        }
 
    }
}
