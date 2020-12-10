// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace System.CommandLine
{
    /// <summary>
    /// Defines the arity of an argument.
    /// </summary>
    /// <remarks>The arity of an option or command's argument refers to the number of values that can be passed if that
    /// option or command is specified. Arity is expressed with a minimum value and a maximum value.
    /// </remarks>
    public interface IArgumentArity
    {
        /// <summary>
        /// Gets the minimum number of values required for the argument.
        /// </summary>
        int MinimumNumberOfValues { get;  }
 
        /// <summary>
        /// Gets the maximum number of values allowed for the argument.
        /// </summary>
        int MaximumNumberOfValues { get;  }
    }
}
