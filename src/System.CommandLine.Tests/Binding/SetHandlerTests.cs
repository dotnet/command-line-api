// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Generic;
using System.CommandLine.Binding;
using System.CommandLine.Invocation;
using System.CommandLine.IO;
using System.CommandLine.Parsing;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Xunit;

namespace System.CommandLine.Tests.Binding
{
    public class SetHandlerTests
    {
        [Fact]
        public async Task Unexpected_return_types_result_in_exit_code_0_if_no_exception_was_thrown()
        {
            var wasCalled = false;

            var command = new Command("wat");

            var handle = (InvocationContext ctx, CancellationToken cancellationToken) =>
            {
                wasCalled = true;
                return Task.FromResult(new { NovelType = true });
            };

            command.SetHandler(handle);

            var exitCode = await command.InvokeAsync("");
            wasCalled.Should().BeTrue();
            exitCode.Should().Be(0);
        }

        [Fact]
        public async Task When_User_Requests_Cancellation_Its_Reflected_By_The_Token_Passed_To_Handler()
        {
            const int ExpectedExitCode = 123;

            Command command = new ("the-command");
            command.SetHandler(async (context, cancellationToken) =>
            {
                try
                {
                    await Task.Delay(Timeout.InfiniteTimeSpan, cancellationToken);
                    context.ExitCode = ExpectedExitCode * -1;
                }
                catch (OperationCanceledException)
                {
                    context.ExitCode = ExpectedExitCode;
                }
            });

            using CancellationTokenSource cts = new ();

            Task<int> invokeResult = command.InvokeAsync("the-command", null, cts.Token);

            cts.Cancel();

            (await invokeResult).Should().Be(ExpectedExitCode);
        }
    }
}