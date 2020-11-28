// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace System.CommandLine
{
    /// <summary>
    /// Represents the arity of an argument.
    /// </summary>
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
