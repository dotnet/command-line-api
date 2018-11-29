// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Threading;

namespace System.CommandLine.Invocation
{
    public sealed class InvocationContext : IDisposable
    {
        private readonly IDisposable _onDispose;
        private CancellationTokenSource _cts;
        private Action<CancellationTokenSource> _cancellationHandlingAddedEvent;

        public InvocationContext(
            ParseResult parseResult,
            Parser parser,
            IConsole console = null)
        {
            ParseResult = parseResult ?? throw new ArgumentNullException(nameof(parseResult));
            Parser = parser ?? throw new ArgumentNullException(nameof(parser));

            if (console != null)
            {
                Console = console;
            }
            else
            {
                Console = SystemConsole.Create();
                _onDispose = Console;
            }
        }

        public Parser Parser { get; }

        public ParseResult ParseResult { get; set; }

        public IConsole Console { get; }

        public int ResultCode { get; set; }

        public IInvocationResult InvocationResult { get; set; }

        internal event Action<CancellationTokenSource> CancellationHandlingAdded
        {
            add
            {
                if (_cts != null)
                {
                    throw new InvalidOperationException($"Handlers must be added before adding cancellation handling.");
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
                throw new InvalidOperationException("Cancellation handing was already added.");
            }
            _cts = new CancellationTokenSource();
            _cancellationHandlingAddedEvent?.Invoke(_cts);
            return _cts.Token;
        }

        public void Dispose()
        {
            _onDispose?.Dispose();
        }
    }
}
