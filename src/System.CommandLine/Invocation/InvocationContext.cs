// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace System.CommandLine.Invocation
{
    /// <summary>
    /// Supports command invocation by providing access to parse results and other services.
    /// </summary>
    public sealed class InvocationContext
    {
        /// <param name="parseResult">The result of the current parse operation.</param>
        public InvocationContext(ParseResult parseResult)
        {
            ParseResult = parseResult;
        }

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
