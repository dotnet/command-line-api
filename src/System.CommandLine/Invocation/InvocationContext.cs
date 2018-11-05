// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Threading;

namespace System.CommandLine.Invocation
{
    public sealed class InvocationContext : IDisposable
    {
        private readonly CancellationTokenSource _cts = new CancellationTokenSource();
        private readonly bool _disposeConsole;
        private readonly Action _cancelAction;

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

            console.CancelKeyPress = () => Cancel();
        }

        public Parser Parser { get; }

        public ParseResult ParseResult { get; set; }

        public IConsole Console { get; }

        public int ResultCode { get; set; }

        public IInvocationResult InvocationResult { get; set; }

        public CancellationToken Canceled => _cts.Token;

        public void Cancel() => _cts.Cancel();

        public void Dispose()
        {
            Console.CancelKeyPress = null;
            if (_disposeConsole)
            {
                Console.Dispose();
            }
        }
    }
}
