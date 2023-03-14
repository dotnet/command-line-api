﻿// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Threading;
using System.Threading.Tasks;

namespace System.CommandLine.Invocation
{
    internal sealed class AnonymousCommandHandler : ICommandHandler
    {
        private readonly Func<InvocationContext, CancellationToken, Task<int>>? _asyncHandle;
        private readonly Func<InvocationContext, int>? _syncHandle;

        internal AnonymousCommandHandler(Func<InvocationContext, CancellationToken, Task<int>> handle)
            => _asyncHandle = handle ?? throw new ArgumentNullException(nameof(handle));

        internal AnonymousCommandHandler(Func<InvocationContext, int> handle)
            => _syncHandle = handle ?? throw new ArgumentNullException(nameof(handle));

        public int Invoke(InvocationContext context)
        {
            if (_syncHandle is not null)
            {
                return _syncHandle(context);
            }

            return SyncUsingAsync(context); // kept in a separate method to avoid JITting

            int SyncUsingAsync(InvocationContext context)
                => InvokeAsync(context, CancellationToken.None).GetAwaiter().GetResult();
        }

        public Task<int> InvokeAsync(InvocationContext context, CancellationToken cancellationToken)
            => _asyncHandle is not null
                ? _asyncHandle(context, cancellationToken)
                : Task.FromResult(Invoke(context));
    }
}