// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.CommandLine.Binding;
using System.CommandLine.Parsing;
using System.Threading;
using System.Threading.Tasks;

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
            BindingContext.ServiceProvider.AddService(GetCancellationToken);
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

        internal object InvokeResult { get; set; }

        public IInvocationResult InvocationResult { get; set; }

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
            if (_cts == null)
            {
                _cts = new CancellationTokenSource();
                _cancellationHandlingAddedEvent?.Invoke(_cts);
            }

            return _cts.Token;
        }

        /// <summary>
        /// Set <see cref="InvokeResult"/> to the result of the command that was invoked.
        /// </summary>
        /// <param name="value">The result of the command invocation. When a task, it's assumed to already have been awaited.</param>
        internal void SetInvokeResult(object value)
        {
            if (value is Task task)
            {
                var result = task.GetType().GetProperty("Result").GetValue(value);

                if (result != null)
                {
                    InvokeResult = result;
                }
            }
            else if (value != null)
            {
                InvokeResult = value;
            }
        }

        public void Dispose()
        {
            (Console as IDisposable)?.Dispose();
        }
    }
}
