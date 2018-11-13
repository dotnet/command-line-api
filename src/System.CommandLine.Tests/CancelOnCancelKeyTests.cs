// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.CommandLine.Builder;
using System.CommandLine.Invocation;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Xunit;

namespace System.CommandLine.Tests
{
    public class CancelOnCancelKeyTests
    {
        private readonly TestConsole _console = new TestConsole();

        [Fact]
        public async Task ConsoleCancelKey_cancels_InvocationContext_CancellationToken()
        {
            const int timeout = 100000;

            bool isCanceled = false;
            bool hasTimedout = false;

            Task<int> invocation = new CommandLineBuilder()
                  .AddCommand(new Command("the-command",
                    handler: CommandHandler.Create<CancellationToken>(async ct => 
                    {
                        try
                        {
                            await Task.Delay(timeout, ct);
                            hasTimedout = true;
                        }
                        catch (TaskCanceledException)
                        {
                            isCanceled = true;
                        }
                    })))
                  .CancelOnCancelKey()
                  .Build()
                  .InvokeAsync("the-command", _console);

            _console.EmitCancelKeyPress();

            var result = await invocation;

            Assert.True(hasTimedout || isCanceled);
            Assert.False(hasTimedout);
            Assert.True(isCanceled);
        }
    }
}
