// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Generic;
using System.Threading;

namespace System.CommandLine.Invocation
{
    public sealed class InvocationContext : IDisposable
    {
        private IDisposable _onDispose;
        private CancellationTokenSource _cts;
        private Action<CancellationTokenSource> _cancellationHandlingAddedEvent;
        private readonly Lazy<IDictionary<string, object>> _items = new Lazy<IDictionary<string, object>>(() => new Dictionary<string, object>());
        private IConsole _console;

        public InvocationContext(
            ParseResult parseResult,
            Parser parser,
            IConsole console = null)
        {
            ParseResult = parseResult ?? throw new ArgumentNullException(nameof(parseResult));
            Parser = parser ?? throw new ArgumentNullException(nameof(parser));
            _console = console ?? new SystemConsole();
        }

        public Parser Parser { get; }

        public ParseResult ParseResult { get; set; }

        public IConsole Console
        {
            get
            {
                if (ConsoleFactory != null)
                {
                    var consoleFactory = ConsoleFactory;
                    ConsoleFactory = null;
                    _console = consoleFactory.CreateConsole(this);
                    _onDispose = _console as IDisposable;
                }

                return _console;
            }
        }

        public int ResultCode { get; set; }

        public IInvocationResult InvocationResult { get; set; }

        public IDictionary<string, object> Items => _items.Value;

        internal IConsoleFactory ConsoleFactory { get; set; } 

        internal IHelpBuilder HelpBuilder => (IHelpBuilder)ServiceProvider.GetService(typeof(IHelpBuilder));

        internal IServiceProvider ServiceProvider => new InvocationContextServiceProvider(this);

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
                throw new InvalidOperationException("Cancellation handling was already added.");
            }

            _cts = new CancellationTokenSource();
            _cancellationHandlingAddedEvent?.Invoke(_cts);
            return _cts.Token;
        }

        public void Dispose()
        {
            _onDispose?.Dispose();
        }

        private class InvocationContextServiceProvider : IServiceProvider
        {
            private readonly InvocationContext _context;

            public InvocationContextServiceProvider(InvocationContext context)
            {
                _context = context;
            }

            public object GetService(Type serviceType)
            {
                if (serviceType == typeof(ParseResult))
                {
                    return _context.ParseResult;
                }

                if (serviceType == typeof(InvocationContext))
                {
                    return _context;
                }

                if (serviceType == typeof(IConsole))
                {
                    return _context.Console;
                }

                if (serviceType == typeof(CancellationToken))
                {
                    return _context.AddCancellationHandling();
                }

                if (serviceType == typeof(IHelpBuilder))
                {
                    return _context.Parser.Configuration.HelpBuilderFactory.CreateHelpBuilder(_context);
                }

                return null;
            }
        }
    }
}
