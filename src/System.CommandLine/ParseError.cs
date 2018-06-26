// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace System.CommandLine
{
    public class ParseError
    {
        internal ParseError(
            string message, 
            SymbolResult symbolResult = null,
            bool canTokenBeRetried = true)
        {
            if (string.IsNullOrWhiteSpace(message))
            {
                throw new ArgumentException("Value cannot be null or whitespace.", nameof(message));
            }
          
            Message = message;
            SymbolResult = symbolResult;
            CanTokenBeRetried = canTokenBeRetried;
        }

        public string Message { get; }

        public SymbolResult SymbolResult { get; }

        internal bool CanTokenBeRetried { get; }

        public override string ToString() => Message;
    }
}
