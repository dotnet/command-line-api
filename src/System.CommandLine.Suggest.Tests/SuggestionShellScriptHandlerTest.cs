// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.IO;
using System.Threading.Tasks;
using FluentAssertions;
using Xunit;

namespace System.CommandLine.Suggest.Tests
{
    public class SuggestionShellScriptHandlerTest
    {
        private readonly CliConfiguration _configuration;

        public SuggestionShellScriptHandlerTest()
        {
            _configuration = new SuggestionDispatcher(new TestSuggestionRegistration()).Configuration;
        }

        [Fact]
        public async Task When_shell_type_is_not_supported_it_throws()
        {
            _configuration.Error = new StringWriter();

            await _configuration.InvokeAsync("script 123");

            _configuration.Error
                    .ToString()
                    .Should()
                    .Contain("Shell '123' is not supported.");
        }

        [Fact]
        public async Task It_should_print_bash_shell_script()
        {
            _configuration.Output = new StringWriter();

            await _configuration.InvokeAsync("script bash");

            _configuration.Output.ToString().Should().Contain("_dotnet_bash_complete()");
            _configuration.Output.ToString().Should().NotContain("\r\n");
        }

        [Fact]
        public async Task It_should_print_powershell_shell_script()
        {
            _configuration.Output = new StringWriter();

            await _configuration.InvokeAsync("script powershell");

            _configuration.Output.ToString().Should().Contain("Register-ArgumentCompleter");
            _configuration.Output.ToString().Should().Contain("\r\n");
        }

        [Fact]
        public async Task It_should_print_zsh_shell_script()
        {
            _configuration.Output = new StringWriter();

            await _configuration.InvokeAsync("script zsh");
            
            _configuration.Output.ToString().Should().Contain("_dotnet_zsh_complete()");
            _configuration.Output.ToString().Should().NotContain("\r\n");
        }
    }
}
