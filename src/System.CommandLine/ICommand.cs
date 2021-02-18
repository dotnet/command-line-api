// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Generic;
using System.CommandLine.Parsing;

namespace System.CommandLine
{
    /// <summary>
    /// A symbol definining an action that can be performed.
    /// </summary>
    /// <remarks>This is also often called a verb.</remarks>
    public interface ICommand : IIdentifierSymbol
    {
        /// <summary>
        /// Gets a value indicating whether unmatched tokens contribute errors to <see cref="ParseResult.Errors"/>>.
        /// </summary>
        bool TreatUnmatchedTokensAsErrors { get; }

        /// <summary>
        /// Gets the <see cref="IArgument">arguments</see> that can be provided to the command.
        /// </summary>
        IReadOnlyList<IArgument> Arguments { get; }

        /// <summary>
        /// Gets the <see cref="IOption">options</see> that can be provided to the command.
        /// </summary>
        IReadOnlyList<IOption> Options { get; }
    }
}
