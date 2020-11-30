// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.CommandLine.Binding;
using System.CommandLine.Suggestions;

namespace System.CommandLine
{
    /// <summary>
    /// Defines a symbol with an arity.
    /// </summary>
    public interface IArgument : 
        ISymbol,
        ISuggestionSource, 
        IValueDescriptor
    {
        /// <summary>
        /// Gets the arity of the argument.
        /// </summary>
        IArgumentArity Arity { get; }
    }
}
