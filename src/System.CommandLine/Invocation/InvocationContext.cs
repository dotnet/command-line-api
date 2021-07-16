// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.CommandLine.Binding;
using System.CommandLine.Parsing;
using System.Threading;

namespace System.CommandLine.Invocation
{
    public sealed class InvocationContext : IDisposable
    {
        private CancellationTokenSource? _cts;
        private Action<CancellationTokenSource>? _cancellationHandlingAddedEvent;

        public BindingContext BindingContext { get; }

        public InvocationContext(
            ParseResult parseResult,
            IConsole? console = null)
        {
            BindingContext = new BindingContext(parseResult, console);
            BindingContext.ServiceProvider.AddService(_ => GetCancellationToken());
            BindingContext.ServiceProvider.AddService(_ => this);
        }

        public IConsole Console => BindingContext.Console;

        public Parser Parser => BindingContext.ParseResult.Parser;

        public Resources Resources => Parser.Configuration.Resources;

        public ParseResult ParseResult
        {
            get => BindingContext.ParseResult;
            set => BindingContext.ParseResult = value;
        }

        public int ExitCode { get; set; }

        public IInvocationResult? InvocationResult { get; set; }

        internal event Action<CancellationTokenSource> CancellationHandlingAdded
        {
            add
            {
                if (_cts != null)
                {
                    throw new InvalidOperationException("Handlers must be added before adding cancellation handling.");
                }

                _cancellationHandlingAddedEvent += value;
            }
            remove => _cancellationHandlingAddedEvent -= value;
        }

        /// <summary>
        /// Gets token to implement cancellation handling.
        /// </summary>
        /// <returns>Token used by the caller to implement cancellation handling.</returns>
        public CancellationToken GetCancellationToken()
        {
            if (_cts is null)
            {
                _cts = new CancellationTokenSource();
                _cancellationHandlingAddedEvent?.Invoke(_cts);
            }

            return _cts.Token;
        }

        public void Dispose()
        {
            (Console as IDisposable)?.Dispose();
        }
    }
}
