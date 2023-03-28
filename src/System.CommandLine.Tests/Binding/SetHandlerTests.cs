// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

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

            Task<int> invokeResult = command.Parse("the-command").InvokeAsync(cts.Token);

            cts.Cancel();

            (await invokeResult).Should().Be(ExpectedExitCode);
        }

        private sealed class CustomCliAction : CliAction
        {
            public override int Invoke(ParseResult parseResult) => throw new NotImplementedException();

            public async override Task<int> InvokeAsync(ParseResult context, CancellationToken cancellationToken = default)
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