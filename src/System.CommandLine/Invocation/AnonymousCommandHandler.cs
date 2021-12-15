// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Threading.Tasks;

namespace System.CommandLine.Invocation
{
    internal class AnonymousCommandHandler : ICommandHandler
    {
        private readonly Func<InvocationContext, Task> _handle;

        public AnonymousCommandHandler(Func<InvocationContext, Task> handle)
        {
            _handle = handle ?? throw new ArgumentNullException(nameof(handle));
        }

        public AnonymousCommandHandler(Action<InvocationContext> handle)
        {
            if (handle == null)
            {
                throw new ArgumentNullException(nameof(handle));
            }

            _handle = Handle;

            Task Handle(InvocationContext context)
            {
                handle(context);
                return Task.FromResult(context.ExitCode);
            }
        }

        public async Task<int> InvokeAsync(InvocationContext context)
        {
            object returnValue = _handle(context);

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
                case null:
                    ret = context.ExitCode;
                    break;
                default:
                    throw new NotSupportedException();
            }

            return ret;
        }
    }
}