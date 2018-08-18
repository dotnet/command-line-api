// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.CommandLine.Invocation;
using System.CommandLine.Tests;
using System.Linq;
using FluentAssertions;
using Xunit;

namespace System.CommandLine.CompletionSuggestions.Tests
{
    public class SuggestionInvokeTests
    {
        [Fact]
        public async void SuggestionParser_invoke_list()
        {
            var testConsole = new TestConsole();
            await SuggestionDispatcher.Parser
                .InvokeAsync(new[] {"list"}, testConsole);
            testConsole.Out.ToString().Should().Contain("dotnet");
        }
    }
}
