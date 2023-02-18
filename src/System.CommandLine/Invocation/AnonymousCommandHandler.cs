// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Threading;
using System.Threading.Tasks;

namespace System.CommandLine.Invocation
{
    internal sealed class AnonymousCommandHandler : ICommandHandler
    {
        private readonly Func<InvocationContext, CancellationToken, Task>? _asyncHandle;
        private readonly Action<InvocationContext>? _syncHandle;

        internal AnonymousCommandHandler(Func<InvocationContext, CancellationToken, Task> handle)
            => _asyncHandle = handle ?? throw new ArgumentNullException(nameof(handle));

        internal AnonymousCommandHandler(Action<InvocationContext> handle)
            => _syncHandle = handle ?? throw new ArgumentNullException(nameof(handle));

        public int Invoke(InvocationContext context)
        {
            if (_syncHandle is not null)
            {
                _syncHandle(context);
                return context.ExitCode;
            }

            return SyncUsingAsync(context); // kept in a separate method to avoid JITting

            int SyncUsingAsync(InvocationContext context)
                => InvokeAsync(context, CancellationToken.None).GetAwaiter().GetResult();
        }

        public async Task<int> InvokeAsync(InvocationContext context, CancellationToken cancellationToken)
        {
            if (_syncHandle is not null)
            {
                return Invoke(context);
            }

            Task handler = _asyncHandle!(context, cancellationToken);
            if (handler is Task<int> intReturning)
            {
                return await intReturning;
            }

            await handler;
            return context.ExitCode;
        }
    }
}