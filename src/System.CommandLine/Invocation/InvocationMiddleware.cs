// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Threading.Tasks;

namespace System.CommandLine.Invocation
{
    public delegate Task InvocationMiddleware(
        InvocationContext context,
        Func<InvocationContext, Task> next);
}
