// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace System.CommandLine
{
    /// <summary>
    /// Defines the arity of an option or argument.
    /// </summary>
    /// <remarks>The arity refers to the number of values that can be passed on the command line.
    /// </remarks>
    public interface IArgumentArity
    {
        /// <summary>
        /// Gets the minimum number of values required for an <see cref="IArgument">argument</see>.
        /// </summary>
        int MinimumNumberOfValues { get;  }
 
        /// <summary>
        /// Gets the maximum number of values allowed for an <see cref="IArgument">argument</see>.
        /// </summary>
        int MaximumNumberOfValues { get;  }
    }
}
