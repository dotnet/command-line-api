// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Threading;
using System.Threading.Tasks;

namespace System.CommandLine.Invocation
{
    internal sealed class AnonymousCliAction : CliAction
    {
        private readonly Func<InvocationContext, CancellationToken, Task<int>>? _asyncAction;
        private readonly Func<InvocationContext, int>? _syncAction;

        internal AnonymousCliAction(Func<InvocationContext, int> action)
            => _syncAction = action ?? throw new ArgumentNullException(nameof(action));

        internal AnonymousCliAction(Func<InvocationContext, CancellationToken, Task<int>> action)
            => _asyncAction = action ?? throw new ArgumentNullException(nameof(action));

        public override int Invoke(InvocationContext context)
        {
            if (_syncAction is not null)
            {
                return _syncAction(context);
            }
            else
            {
                return SyncUsingAsync(context); // kept in a separate method to avoid JITting
            }

            int SyncUsingAsync(InvocationContext context)
                => _asyncAction!(context, CancellationToken.None).GetAwaiter().GetResult();
        }

        public override async Task<int> InvokeAsync(InvocationContext context, CancellationToken cancellationToken)
        {
            if (_asyncAction is not null)
            {
                return await _asyncAction(context, cancellationToken);
            }
            else
            {
               return _syncAction!(context);
            }
        }
    }
}