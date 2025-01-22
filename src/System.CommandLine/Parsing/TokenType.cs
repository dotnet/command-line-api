// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace System.CommandLine.Parsing
{
    /// <summary>
    /// Identifies the type of a <see cref="Token"/>.
    /// </summary>
    public enum TokenType
    {
        /// <summary>
        /// An argument token.
        /// </summary>
        /// <see cref="CommandLine.Argument"/>
        Argument,

        /// <summary>
        /// A command token.
        /// </summary>
        /// <see cref="CommandLine.Command"/>
        Command,

        /// <summary>
        /// An option token.
        /// </summary>
        /// <see cref="CommandLine.Option"/>
        Option,
        
        /// <summary>
        /// A double dash (<c>--</c>) token, which changes the meaning of subsequent tokens.
        /// </summary>
        DoubleDash,

        /// <summary>
        /// A directive token.
        /// </summary>
        /// <see cref="CommandLine.Directive"/>
        Directive
    }
}
