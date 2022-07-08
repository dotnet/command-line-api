// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Generic;
using System.CommandLine.Binding;
using System.CommandLine.Help;
using System.CommandLine.IO;
using System.CommandLine.Parsing;
using System.Threading;

namespace System.CommandLine.Invocation
{
    /// <summary>
    /// Supports command invocation by providing access to parse results and other services.
    /// </summary>
    public sealed class InvocationContext : IDisposable
    {
        private HelpBuilder? _helpBuilder;
        private BindingContext? _bindingContext;
        private IConsole? _console;
        private readonly CancellationToken _token; 
        private readonly LinkedList<CancellationTokenRegistration> _registrations = new();
        private volatile CancellationTokenSource? _source;

        /// <param name="parseResult">The result of the current parse operation.</param>
        /// <param name="console">The console to which output is to be written.</param>
        /// <param name="cancellationToken">A cancellation token that can be used to cancel and invocation.</param>
        public InvocationContext(
            ParseResult parseResult,
            IConsole? console = null,
            CancellationToken cancellationToken = default)
        {
            ParseResult = parseResult;
            _console = console;
            
            _source = new CancellationTokenSource();
            _token = _source.Token;
            if (cancellationToken.CanBeCanceled)
            {
                LinkToken(cancellationToken);
            }
        }

        /// <summary>
        /// The binding context for the current invocation.
        /// </summary>
        public BindingContext BindingContext
        {
            get
            {
                if (_bindingContext is null)
                {
                    _bindingContext = new BindingContext(this);
                    _bindingContext.ServiceProvider.AddService(_ => GetCancellationToken());
                    _bindingContext.ServiceProvider.AddService(_ => this);
                }

                return _bindingContext;
            }
        }

        /// <summary>
        /// The console to which output should be written during the current invocation.
        /// </summary>
        public IConsole Console
        {
            get
            {
                if (_console is null)
                {
                    _console = new SystemConsole();
                }

                return _console;
            }
            set => _console = value;
        } 

        /// <summary>
        /// Enables writing help output.
        /// </summary>
        public HelpBuilder HelpBuilder => _helpBuilder ??= Parser.Configuration.HelpBuilderFactory(BindingContext);

        /// <summary>
        /// The parser used to create the <see cref="ParseResult"/>.
        /// </summary>
        public Parser Parser => ParseResult.Parser;

        /// <summary>
        /// Provides localizable strings for help and error messages.
        /// </summary>
        public LocalizationResources LocalizationResources => Parser.Configuration.LocalizationResources;

        /// <summary>
        /// The parse result for the current invocation.
        /// </summary>
        public ParseResult ParseResult { get; set; }

        /// <summary>
        /// A value that can be used to set the exit code for the process.
        /// </summary>
        public int ExitCode { get; set; }

        /// <summary>
        /// The result of the current invocation.
        /// </summary>
        /// <remarks>As the <see cref="InvocationContext"/> is passed through the invocation pipeline to the <see cref="ICommandHandler"/> associated with the invoked command, only the last value of this property will be the one applied.</remarks>
        public IInvocationResult? InvocationResult { get; set; }

        /// <summary>
        /// Gets a cancellation token that can be used to check if cancellation has been requested.
        /// </summary>
        public CancellationToken GetCancellationToken() => _token;

        internal void Cancel()
        {
            using var source = Interlocked.Exchange(ref _source, null);
            source?.Cancel();
        }

        public void LinkToken(CancellationToken token)
        {
            _registrations.AddLast(token.Register(Cancel));
        }

        /// <inheritdoc />
        void IDisposable.Dispose()
        {
            Interlocked.Exchange(ref _source, null)?.Dispose();
            foreach (CancellationTokenRegistration registration in _registrations)
            {
                registration.Dispose();
            }
        }
    }
}
