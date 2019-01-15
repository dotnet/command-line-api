// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace System.CommandLine.Invocation
{
    internal class AnonymousConsoleFactory : IConsoleFactory
    {
        private readonly Func<InvocationContext, IConsole> _create;

        public AnonymousConsoleFactory(Func<InvocationContext, IConsole> create)
        {
            _create = create;
        }

        public IConsole CreateConsole(InvocationContext invocationContext)
        {
            return _create(invocationContext);
        }
    }
}
