// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Threading;
using System.Threading.Tasks;

namespace System.CommandLine.Invocation
{
    internal sealed class AnonymousCliAction : CliAction
    {
        private readonly Func<ParseResult, CancellationToken, Task>? _asyncAction;
        private readonly Action<ParseResult>? _syncAction;

        internal AnonymousCliAction(Action<ParseResult> action)
            => _syncAction = action ?? throw new ArgumentNullException(nameof(action));

        internal AnonymousCliAction(Func<ParseResult, CancellationToken, Task> action)
            => _asyncAction = action ?? throw new ArgumentNullException(nameof(action));

        public override int Invoke(ParseResult parseResult)
        {
            if (_syncAction is not null)
            {
                _syncAction(parseResult);
            }
            else
            {
                SyncUsingAsync(parseResult); // kept in a separate method to avoid JITting
            }

            return 0;

            void SyncUsingAsync(ParseResult parseResult)
                => _asyncAction!(parseResult, CancellationToken.None).GetAwaiter().GetResult();
        }

        public async override Task<int> InvokeAsync(ParseResult parseResult, CancellationToken cancellationToken)
        {
            if (_asyncAction is not null)
            {
                await _asyncAction(parseResult, cancellationToken);
            }
            else
            {
                _syncAction!(parseResult);
            }

            return 0;
        }
    }
}