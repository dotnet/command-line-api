// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.CommandLine.Binding;
using System.CommandLine.Suggestions;

namespace System.CommandLine
{
    /// <summary>
    /// Represents a value passed to an <see cref="IOption"/> or <see cref="ICommand"/>.
    /// </summary>
    public interface IArgument : 
        ISymbol,
        ISuggestionSource, 
        IValueDescriptor
    {
        /// <summary>
        /// Gets or sets the arity of the argument.
        /// </summary>
        IArgumentArity Arity { get; }
    }
}
