// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.IO;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Xunit;

namespace System.CommandLine.Suggest.Tests
{
    public class SuggestionShellScriptHandlerTest
    {
        private readonly RootCommand _rootCommand;
        private readonly InvocationConfiguration _configuration;

        public SuggestionShellScriptHandlerTest()
        {
            _rootCommand = new SuggestionDispatcher(new TestSuggestionRegistration()).RootCommand;
            _configuration = new()
            {
                Output = new StringWriter(),
                Error = new StringWriter()
            };
        }

        [Fact]
        public async Task When_shell_type_is_not_supported_it_throws()
        {
            await _rootCommand.Parse("script 123").InvokeAsync(_configuration, CancellationToken.None);

            _configuration.Error
                          .ToString()
                          .Should()
                          .Contain("Shell '123' is not supported.");
        }

        [Fact]
        public async Task It_should_print_bash_shell_script()
        {
            await _rootCommand.Parse("script bash").InvokeAsync(_configuration, CancellationToken.None);

            _configuration.Output.ToString().Should().Contain("_dotnet_bash_complete()");
            _configuration.Output.ToString().Should().NotContain("\r\n");
        }

        [Fact]
        public async Task It_should_print_powershell_shell_script()
        {
            await _rootCommand.Parse("script powershell").InvokeAsync(_configuration, CancellationToken.None);

            _configuration.Output.ToString().Should().Contain("Register-ArgumentCompleter");
            _configuration.Output.ToString().Should().Contain("\r\n");
        }

        [Fact]
        public async Task It_should_print_zsh_shell_script()
        {
            await _rootCommand.Parse("script zsh").InvokeAsync(_configuration, CancellationToken.None);

            _configuration.Output.ToString().Should().Contain("_dotnet_zsh_complete()");
            _configuration.Output.ToString().Should().NotContain("\r\n");
        }
    }
}