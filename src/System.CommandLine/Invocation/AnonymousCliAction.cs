// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Threading;
using System.Threading.Tasks;

namespace System.CommandLine.Invocation
{
    internal sealed class AnonymousCliAction : CliAction
    {
        private readonly Func<ParseResult, CancellationToken, Task<int>>? _asyncAction;
        private readonly Func<ParseResult, int>? _syncAction;

        internal AnonymousCliAction(Func<ParseResult, int> action)
            => _syncAction = action;

        internal AnonymousCliAction(Func<ParseResult, CancellationToken, Task<int>> action)
            => _asyncAction = action;

        public override int Invoke(ParseResult parseResult)
        {
            if (_syncAction is not null)
            {
                return _syncAction(parseResult);
            }
            else
            {
                return SyncUsingAsync(parseResult); // kept in a separate method to avoid JITting
            }

            int SyncUsingAsync(ParseResult parseResult)
                => _asyncAction!(parseResult, CancellationToken.None).GetAwaiter().GetResult();
        }

        public override async Task<int> InvokeAsync(ParseResult parseResult, CancellationToken cancellationToken)
        {
            if (_asyncAction is not null)
            {
                return await _asyncAction(parseResult, cancellationToken);
            }
            else
            {
               return _syncAction!(parseResult);
            }
        }
    }
}