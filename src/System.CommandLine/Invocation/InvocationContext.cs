// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

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
    public sealed class InvocationContext
    {
        private CancellationTokenSource? _cts;
        private Action<CancellationTokenSource>? _cancellationHandlingAddedEvent;
        private HelpBuilder? _helpBuilder;
        private BindingContext? _bindingContext;
        private IConsole? _console;

        /// <param name="parseResult">The result of the current parse operation.</param>
        /// <param name="console">The console to which output is to be written.</param>
        public InvocationContext(
            ParseResult parseResult,
            IConsole? console = null)
        {
            ParseResult = parseResult;
            _console = console;
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

        internal event Action<CancellationTokenSource> CancellationHandlingAdded
        {
            add
            {
                if (_cts is not null)
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
    }
}
