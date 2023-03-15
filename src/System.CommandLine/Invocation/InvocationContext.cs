// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.CommandLine.Help;
using System.CommandLine.IO;

namespace System.CommandLine.Invocation
{
    /// <summary>
    /// Supports command invocation by providing access to parse results and other services.
    /// </summary>
    public sealed class InvocationContext
    {
        private HelpBuilder? _helpBuilder;
        private IConsole? _console;

        /// <param name="parseResult">The result of the current parse operation.</param>
        /// <param name="console">The console to which output is to be written.</param>
        public InvocationContext(ParseResult parseResult, IConsole? console = null)
        {
            ParseResult = parseResult;
            _console = console;
        }

        /// <summary>
        /// The console to which output should be written during the current invocation.
        /// </summary>
        public IConsole Console
        {
            get => _console ??= new SystemConsole();
            set => _console = value;
        } 

        /// <summary>
        /// Enables writing help output.
        /// </summary>
        public HelpBuilder HelpBuilder => _helpBuilder ??= ParseResult.Configuration.HelpBuilderFactory(this);

        /// <summary>
        /// The parse result for the current invocation.
        /// </summary>
        public ParseResult ParseResult { get; set; }

        /// <inheritdoc cref="ParseResult.GetValue{T}(Option{T})"/>
        public T? GetValue<T>(Option<T> option)
            => ParseResult.GetValue(option);

        /// <inheritdoc cref="ParseResult.GetValue{T}(Argument{T})"/>
        public T? GetValue<T>(Argument<T> argument)
            => ParseResult.GetValue(argument);
    }
}
