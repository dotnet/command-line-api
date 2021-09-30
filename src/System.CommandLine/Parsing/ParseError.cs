// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace System.CommandLine.Parsing
{
    /// <summary>
    /// Describes an error that occurs while parsing command line input.
    /// </summary>
    public class ParseError
    {
        internal ParseError(
            string message, 
            SymbolResult? symbolResult = null)
        {
            if (string.IsNullOrWhiteSpace(message))
            {
                throw new ArgumentException("Value cannot be null or whitespace.", nameof(message));
            }
          
            Message = message;
            SymbolResult = symbolResult;
        }

        /// <summary>
        /// A message to explain the error to a user.
        /// </summary>
        public string Message { get; }

        /// <summary>
        /// The symbol result detailing the symbol that failed to parse and the tokens involved.
        /// </summary>
        public SymbolResult? SymbolResult { get; }

        /// <inheritdoc />
        public override string ToString() => Message;
    }
}
