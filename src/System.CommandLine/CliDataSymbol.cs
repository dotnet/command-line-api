// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace System.CommandLine;

public abstract class CliDataSymbol : CliSymbol
{
    protected CliDataSymbol(string name, bool allowWhitespace = false)
        : base(name, allowWhitespace)
    { }
        /// <summary>
        /// Gets or sets the <see cref="Type" /> that the argument's parsed tokens will be converted to.
        /// </summary>
        public abstract Type ValueType { get; }
}
