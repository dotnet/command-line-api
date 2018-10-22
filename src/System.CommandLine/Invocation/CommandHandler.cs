// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Threading.Tasks;

namespace System.CommandLine.Invocation
{
    public abstract class CommandHandler : ICommandHandler
    {
        protected virtual async Task<int> GetResultCodeAsync(object value)
        {
            switch (value)
            {
                case Task<int> resultCodeTask:
                    return await resultCodeTask;
                case Task task:
                    await task;
                    return 0;
                case int resultCode:
                    return resultCode;
                case null:
                    return 0;
                default:
                    throw new NotSupportedException();
            }
        }

        public abstract Task<int> InvokeAsync(InvocationContext context);
    }
}
