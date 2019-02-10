// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.CommandLine.Binding;
using System.Threading;

namespace System.CommandLine.Invocation
{
    public sealed class InvocationContext : IDisposable
    {
        private CancellationTokenSource _cts;
        private Action<CancellationTokenSource> _cancellationHandlingAddedEvent;

        public BindingContext BindingContext { get; }

        public InvocationContext(
            ParseResult parseResult,
            IConsole console = null)
        {
            BindingContext = new BindingContext(parseResult, console);
            BindingContext.ServiceProvider.AddService(AddCancellationHandling);
            BindingContext.ServiceProvider.AddService(() => this);
        }

        public IConsole Console => BindingContext.Console;

        public Parser Parser => BindingContext.ParseResult.Parser;

        public ParseResult ParseResult
        {
            get => BindingContext.ParseResult;
            set => BindingContext.ParseResult = value;
        }

        public int ResultCode { get; set; }

        public IInvocationResult InvocationResult { get; set; }

        internal IServiceProvider ServiceProvider => BindingContext.ServiceProvider;

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
        /// Indicates the invocation can be cancelled.
        /// </summary>
        /// <returns>Token used by the caller to implement cancellation handling.</returns>
        internal CancellationToken AddCancellationHandling()
        {
            if (_cts != null)
            {
                throw new InvalidOperationException("Cancellation handling was already added.");
            }

            _cts = new CancellationTokenSource();
            _cancellationHandlingAddedEvent?.Invoke(_cts);
            return _cts.Token;
        }

        public void Dispose()
        {
            (Console as IDisposable)?.Dispose();
        }
    }
}
