// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.CommandLine.Binding;

namespace System.CommandLine
{
    /// <summary>
    /// A symbol defining a value that can be passed on the command line to a <see cref="ICommand">command</see> or <see cref="IOption">option</see>.
    /// </summary>
    public interface IArgument : 
        ISymbol,
        IValueDescriptor
    {
        /// <summary>
        /// Gets the arity of the argument.
        /// </summary>
        IArgumentArity Arity { get; }
    }
}
