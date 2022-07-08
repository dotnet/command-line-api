using FluentAssertions;
using System.CommandLine;
using System.CommandLine.Invocation;
using System.CommandLine.Parsing;
using System.Threading;
using Xunit;

namespace System.CommandLine.Tests.Invocation
{
    public class InvocationContextTests
    {
        [Fact]
        public void InvocationContext_with_cancellation_token_returns_it()
        {
            using CancellationTokenSource cts = new();
            var parseResult = new CommandLineBuilder(new RootCommand())
                .Build()
                .Parse("");
            using InvocationContext context = new(parseResult, cancellationToken: cts.Token);

            var token = context.GetCancellationToken();

            token.IsCancellationRequested.Should().BeFalse();
            cts.Cancel();
            token.IsCancellationRequested.Should().BeTrue();
        }

        [Fact]
        public void InvocationContext_with_linked_cancellation_token_can_cancel_by_passed_token()
        {
            using CancellationTokenSource cts1 = new();
            using CancellationTokenSource cts2 = new();
            var parseResult = new CommandLineBuilder(new RootCommand())
                .Build()
                .Parse("");
            using InvocationContext context = new(parseResult, cancellationToken: cts1.Token);
            context.LinkToken(cts2.Token);

            var token = context.GetCancellationToken();

            token.IsCancellationRequested.Should().BeFalse();
            cts1.Cancel();
            token.IsCancellationRequested.Should().BeTrue();
        }

        [Fact]
        public void InvocationContext_with_linked_cancellation_token_can_cancel_by_linked_token()
        {
            using CancellationTokenSource cts1 = new();
            using CancellationTokenSource cts2 = new();
            var parseResult = new CommandLineBuilder(new RootCommand())
                .Build()
                .Parse("");
            using InvocationContext context = new(parseResult, cancellationToken: cts1.Token);
            context.LinkToken(cts2.Token);

            var token = context.GetCancellationToken();

            token.IsCancellationRequested.Should().BeFalse();
            cts2.Cancel();
            token.IsCancellationRequested.Should().BeTrue();
        }
    }
}
