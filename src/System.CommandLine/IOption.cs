// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.CommandLine.Binding;

namespace System.CommandLine
{
    /// <summary>
    /// Defines a symbol with an argument. 
    /// </summary>
    public interface IOption : IIdentifierSymbol, IValueDescriptor
    {
        /// <summary>
        /// Gets the argument for the option.
        /// </summary>
        IArgument Argument { get; }

        /// <summary>
        /// Gets a value that indicates whether the option is required.
        /// </summary>
        bool IsRequired { get; }

        /// <summary>
        /// Gets a value that indicates whether multiple argument values are allowed.
        /// </summary>
        bool AllowMultipleArgumentsPerToken { get; }
    }
}
