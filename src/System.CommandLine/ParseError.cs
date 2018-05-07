// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace System.CommandLine
{
    public class ParseError
    {
        public ParseError(
            string message, 
            string token,
            ParsedSymbol parsedSymbol = null,
            bool canTokenBeRetried = true)
        {
            if (string.IsNullOrWhiteSpace(message))
            {
                throw new ArgumentException("Value cannot be null or whitespace.", nameof(message));
            }
            if (string.IsNullOrWhiteSpace(token))
            {
                throw new ArgumentException("Value cannot be null or whitespace.", nameof(token));
            }

            Message = message;
            ParsedSymbol = parsedSymbol;
            CanTokenBeRetried = canTokenBeRetried;
        }

        public string Message { get; }

        public ParsedSymbol ParsedSymbol { get; }

        internal bool CanTokenBeRetried { get; }

        public override string ToString() => Message;
    }
}
