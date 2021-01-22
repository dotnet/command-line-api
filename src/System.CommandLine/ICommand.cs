// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Generic;

namespace System.CommandLine
{
    /// <summary>
    /// Defines a symbol with arguments and options.
    /// </summary>
    public interface ICommand : IIdentifierSymbol
    {
        /// <summary>
        /// Gets a value that indicates whether unmatched tokens should be treated as errors.
        /// </summary>
        bool TreatUnmatchedTokensAsErrors { get; }

        /// <summary>
        /// Gets the arguments for the command.
        /// </summary>
        IEnumerable<IArgument> Arguments { get; }
        
        /// <summary>
        /// Gets the options for the command.
        /// </summary>
        IEnumerable<IOption> Options { get; }

        IReadOnlyList<IArgument> Arguments { get; }

        IReadOnlyList<IOption> Options { get; }
    }
}
