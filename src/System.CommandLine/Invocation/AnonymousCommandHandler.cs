// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Threading.Tasks;

namespace System.CommandLine.Invocation
{
    internal class AnonymousCommandHandler : ICommandHandler
    {
        private readonly Func<InvocationContext, Task> _getResult;

        public AnonymousCommandHandler(Func<InvocationContext, Task> getResult)
        {
            _getResult = getResult;
        }

        public AnonymousCommandHandler(Action<InvocationContext> getResult)
        {
            _getResult = GetResult;

            Task GetResult(InvocationContext context)
            {
                getResult(context);
                return Task.FromResult(0);
            }
        }

        public Task<int> InvokeAsync(InvocationContext context) =>
            CommandHandler.GetExitCodeAsync(_getResult(context), context);
    }
}