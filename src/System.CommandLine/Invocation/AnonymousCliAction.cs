// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Threading;
using System.Threading.Tasks;

namespace System.CommandLine.Invocation
{
    internal sealed class AnonymousCliAction : CliAction
    {
        private readonly Func<InvocationContext, CancellationToken, Task>? _asyncAction;
        private readonly Action<InvocationContext>? _syncAction;

        internal AnonymousCliAction(Action<InvocationContext> action)
            => _syncAction = action ?? throw new ArgumentNullException(nameof(action));

        internal AnonymousCliAction(Func<InvocationContext, CancellationToken, Task> action)
            => _asyncAction = action ?? throw new ArgumentNullException(nameof(action));

        public override int Invoke(InvocationContext context)
        {
            if (_syncAction is not null)
            {
                _syncAction(context);
            }
            else
            {
                SyncUsingAsync(context); // kept in a separate method to avoid JITting
            }

            return 0;

            void SyncUsingAsync(InvocationContext context)
                => _asyncAction!(context, CancellationToken.None).GetAwaiter().GetResult();
        }

        public async override Task<int> InvokeAsync(InvocationContext context, CancellationToken cancellationToken)
        {
            if (_asyncAction is not null)
            {
                await _asyncAction(context, cancellationToken);
            }
            else
            {
                _syncAction!(context);
            }

            return 0;
        }
    }
}