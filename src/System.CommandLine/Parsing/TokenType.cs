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
        /// <see cref="Argument"/>
        Argument,

        /// <summary>
        /// A command token.
        /// </summary>
        /// <see cref="Command"/>
        Command,
        
        /// <summary>
        /// An option token.
        /// </summary>
        /// <see cref="Option"/>
        Option,
        
        /// <summary>
        /// A double dash (<c>--</c>) token, which changes the meaning of subsequent tokens.
        /// </summary>
        /// <see cref="CommandLineConfiguration.EnableLegacyDoubleDashBehavior"/>
        DoubleDash,

        /// <summary>
        /// A token following <see cref="DoubleDash"/> when <see cref="CommandLineConfiguration.EnableLegacyDoubleDashBehavior"/> is set to <see langword="true"/>.
        /// </summary>
        /// <see cref="CommandLineConfiguration.EnableLegacyDoubleDashBehavior"/>
        Unparsed,
        
        /// <summary>
        /// A directive token.
        /// </summary>
        /// <see cref="DirectiveCollection"/>
        Directive
    }
}
