// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace System.CommandLine.Invocation
{
    /// <summary>
    /// The result of a command handler invocation.
    /// </summary>
    public interface IInvocationResult
    {
        /// <summary>
        /// Applies the result to the current invocation context.
        /// </summary>
        /// <param name="context">The context for the current invocation.</param>
        void Apply(InvocationContext context);
    }
}
