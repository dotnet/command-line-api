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
        const int ExpectedExitCode = 123;

        [Fact]
        public async Task When_User_Requests_Cancellation_Its_Reflected_By_The_Token_Passed_To_Handler()
        {
            Command command = new("the-command")
            {
                Action = new CustomCliAction()
            };

            using CancellationTokenSource cts = new ();

            Task<int> invokeResult = command.InvokeAsync("the-command", null, cts.Token);

            cts.Cancel();

            (await invokeResult).Should().Be(ExpectedExitCode);
        }

        private sealed class CustomCliAction : CliAction
        {
            public override int Invoke(InvocationContext context) => throw new NotImplementedException();

            public async override Task<int> InvokeAsync(InvocationContext context, CancellationToken cancellationToken = default)
            {
                try
                {
                    await Task.Delay(Timeout.InfiniteTimeSpan, cancellationToken);
                    return ExpectedExitCode * -1;
                }
                catch (OperationCanceledException)
                {
                    return ExpectedExitCode;
                }
            }
        }
    }
}