// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace System.CommandLine.Binding
{
    internal class AnonymousConsoleFactory : IConsoleFactory
    {
        private readonly Func<BindingContext, IConsole> _create;

        public AnonymousConsoleFactory(Func<BindingContext, IConsole> create)
        {
            _create = create;
        }

        public IConsole CreateConsole(BindingContext context)
        {
            return _create(context);
        }
    }
}
