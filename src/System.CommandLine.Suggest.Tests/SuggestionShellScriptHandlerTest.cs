// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using FluentAssertions;
using Xunit;

namespace System.CommandLine.Suggest.Tests
{
    public class SuggestionShellScriptHandlerTest
    {
        [Fact]
        public void When_shellName_does_not_support_it_should_print_error()
        {
            var console = new TestConsole();
            Action a = () => SuggestionShellScriptHandler.Handle(console, "fish");
            a.Should().Throw<SuggestionShellScriptException>().And.Message.Should()
                .Contain("fish shell is not supported.");
        }

        [Fact]
        public void It_should_print_bash_shell_script()
        {
            var console = new TestConsole();
            SuggestionShellScriptHandler.Handle(console, "Bash");
            console.Out.ToString().Should().Contain("_dotnet_bash_complete()");
        }

        [Fact]
        public void It_should_print_powershell_shell_script()
        {
            var console = new TestConsole();
            SuggestionShellScriptHandler.Handle(console, "PowerShell");
            console.Out.ToString().Should().Contain("Register-ArgumentCompleter");
        }
    }
}
