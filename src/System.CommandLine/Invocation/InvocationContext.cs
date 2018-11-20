// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Threading;

namespace System.CommandLine.Invocation
{
    public sealed class InvocationContext : IDisposable
    {
        private readonly IDisposable _onDispose;
        private CancellationTokenSource _cts;
        private bool _isCancellationSupported;
        private readonly object _gate = new object();

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

        /// <summary>
        /// Indicates the invocation can be cancelled.
        /// </summary>
        /// <returns>Token used by the caller to implement cancellation handling.</returns>
        public CancellationToken AddCancellationHandling()
        {
            lock (_gate)
            {
                _isCancellationSupported = true;
                if (_cts == null)
                {
                    _cts = new CancellationTokenSource();
                }
                return _cts.Token;
            }
        }

        /// <summary>
        /// Cancels the invocation.
        /// </summary>
        /// <param name="isCancelling">returns whether the invocation is being cancelled.</param>
        public void Cancel(out bool isCancelling)
        {
            lock (_gate)
            {
                if (_cts == null)
                {
                    _cts = new CancellationTokenSource();
                }
                _cts.Cancel();
                isCancelling = _isCancellationSupported;
            }
        }

        public bool IsCancellationRequested =>
            _cts?.Token.IsCancellationRequested == true;

        public void Dispose()
        {
            _onDispose?.Dispose();
        }
    }
}
