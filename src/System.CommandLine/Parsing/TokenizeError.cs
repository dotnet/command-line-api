// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace System.CommandLine.Parsing
{
    /// <summary>
    /// Describes an error that occurs while tokenizing command line input.
    /// </summary>
    /// <seealso href="/dotnet/standard/commandline/syntax">Command-line syntax overview</seealso>
    public class TokenizeError
    {
        internal TokenizeError(string message)
        {
            Message = message ?? throw new ArgumentNullException(nameof(message));
        }

        /// <summary>
        /// A message to explain the error to a user.
        /// </summary>
        public string Message { get; }

        /// <inheritdoc />
        public override string ToString() => Message;
    }
}
