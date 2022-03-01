// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Threading.Tasks;

namespace System.CommandLine.Invocation
{
    internal class AnonymousCommandHandler : ICommandHandler
    {
        private readonly Func<InvocationContext, Task>? _asyncHandle;
        private readonly Action<InvocationContext>? _syncHandle;

        public AnonymousCommandHandler(Func<InvocationContext, Task> handle)
            => _asyncHandle = handle ?? throw new ArgumentNullException(nameof(handle));

        public AnonymousCommandHandler(Action<InvocationContext> handle)
            => _syncHandle = handle ?? throw new ArgumentNullException(nameof(handle));

        public int Invoke(InvocationContext context)
        {
            if (_syncHandle is not null)
            {
                _syncHandle(context);
                return context.ExitCode;
            }

            return SyncUsingAsync(context); // kept in a separate method to avoid JITting
        }

        private int SyncUsingAsync(InvocationContext context) => InvokeAsync(context).GetAwaiter().GetResult();

        public async Task<int> InvokeAsync(InvocationContext context)
        {
            if (_syncHandle is not null)
            {
                return Invoke(context);
            }

            object returnValue = _asyncHandle!(context);

            int ret;

            switch (returnValue)
            {
                case Task<int> exitCodeTask:
                    ret = await exitCodeTask;
                    break;
                case Task task:
                    await task;
                    ret = context.ExitCode;
                    break;
                case int exitCode:
                    ret = exitCode;
                    break;
                default:
                    ret = context.ExitCode;
                    break;
            }

            return ret;
        }
    }
}