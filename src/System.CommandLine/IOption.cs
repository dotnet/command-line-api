// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.CommandLine.Binding;

namespace System.CommandLine
{
    /// <summary>
    /// A symbol defining a named parameter and a value for that parameter. 
    /// </summary>
    public interface IOption : IIdentifierSymbol, IValueDescriptor
    {
        /// <summary>
        /// Gets the <see cref="IArgument">argument</see> for the option.
        /// </summary>
        IArgument Argument { get; }

        /// <summary>
        /// Gets a value that indicates whether the option is required.
        /// </summary>
        bool IsRequired { get; }

        /// <summary>
        /// Gets a value that indicates whether multiple argument tokens are allowed for each option identifier token.
        /// </summary>
        /// <example>
        /// If set to <see langword="true"/>, the following command line is valid for passing multiple arguments:
        /// <code>
        /// > --opt 1 2 3
        /// </code>
        /// The following is equivalent and is always valid:
        /// <code>
        /// > --opt 1 --opt 2 --opt 3
        /// </code>
        /// </example>
        bool AllowMultipleArgumentsPerToken { get; }
    }
}
