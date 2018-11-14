// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Threading;

namespace System.CommandLine.Invocation
{
    public sealed class InvocationContext : IDisposable
    {
        private readonly IDisposable _onDispose;
        private CancellationTokenSource _cts;
        private bool _isCancellationEnabled;
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

        // Indicates the invocation can be cancelled.
        // This returns a CancellationToken that will be set when the invocation
        // is cancelled. The method may return a CancellationToken that is already
        // cancelled.
        public CancellationToken EnableCancellation()
        {
            lock (_gate)
            {
                _isCancellationEnabled = true;
                if (_cts == null)
                {
                    _cts = new CancellationTokenSource();
                }
                return _cts.Token;
            }
        }

        // The return value indicates if the Invocation has cancellation enabled.
        // When Cancel returns false, the Middleware may decide to forcefully
        // end the process, for example, by calling Environment.Exit.
        public bool Cancel()
        {
            lock (_gate)
            {
                if (_cts == null)
                {
                    _cts = new CancellationTokenSource();
                }
                _cts.Cancel();
                return _isCancellationEnabled;
            }
        }

        public void Dispose()
        {
            _onDispose?.Dispose();
        }
    }
}
