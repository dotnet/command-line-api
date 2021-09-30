// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Threading.Tasks;

namespace System.CommandLine.Invocation
{
    /// <summary>
    /// A delegate used for adding command handler invocation middleware.
    /// </summary>
    /// <param name="context">The context for the current invocation, which will be passed to each middleware and then to the command handler, unless a middleware short circuits it.</param>
    /// <param name="next">A continuation. Passing the incoming <see cref="InvocationContext"/> to it will execute the next middleware in the pipeline and, at the end of the pipeline, the command handler. Middleware can short circuit the invocation by not calling this continuation.</param>
    public delegate Task InvocationMiddleware(
        InvocationContext context,
        Func<InvocationContext, Task> next);
}
